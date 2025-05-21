import { Injectable, signal } from '@angular/core';
import { filter, interval, take, tap } from 'rxjs';
import { HttpTransportType, HubConnection, HubConnectionBuilder, IHttpConnectionOptions, LogLevel } from '@microsoft/signalr';
import { RaceData, RallyData, TruckData } from './displays';

export interface ConnectionInfo {
  status: ConnectionStatus;
  message?: string;
  reloadSeconds?: number;
}

export enum ConnectionStatus {
  Disconnected,
  Connecting,
  ConnectionError,
  Connected,
}

enum DisplayType {
  None,
  TruckDashboard,
  RaceDashboard,
  RallyDashboard,
}

interface DisplayUpdate {
  type: DisplayType;
  data: TruckData | RaceData | RallyData | undefined;
  page: number;
}

@Injectable({
  providedIn: 'root',
})
export class GameDataService {
  private readonly _hubConnection: HubConnection;

  private readonly _connectionStatus = signal<ConnectionInfo>({ status: ConnectionStatus.Disconnected });
  public readonly connectionStatus = this._connectionStatus.asReadonly();

  private readonly _truckData = signal<TruckData | null>(null);
  public readonly truckData = this._truckData.asReadonly();

  private readonly _raceData = signal<RaceData | null>(null);
  public readonly raceData = this._raceData.asReadonly();

  private readonly _rallyData = signal<RallyData | null>(null);
  public readonly rallyData = this._rallyData.asReadonly();

  private readonly _page = signal<number>(1);
  public readonly page = this._page.asReadonly();

  public constructor() {
    const connectionOptions: IHttpConnectionOptions = {
      transport: HttpTransportType.WebSockets,
      skipNegotiation: false,
      logMessageContent: false,
    };

    this._hubConnection = new HubConnectionBuilder()
      .withUrl('display-data', connectionOptions)
      .configureLogging(LogLevel.Error) // Warning => then if the frontend receives messages but isn't subscribed to a topic shows a warning.
      .withAutomaticReconnect()
      .build();

    this._connectionStatus.set({ status: ConnectionStatus.Connecting });
    this._hubConnection.start().then(() => {
      this._connectionStatus.set({ status: ConnectionStatus.Connected });
    }).catch((error) => {
      this._connectionStatus.set({ status: ConnectionStatus.ConnectionError, message: error as string });
      this.startReloadSequence();
    });

    this._hubConnection.onreconnecting((error) => this._connectionStatus.set({ status: ConnectionStatus.Connecting, message: error?.message }));
    this._hubConnection.onreconnected(() => this._connectionStatus.set({ status: ConnectionStatus.Connected }));
    this._hubConnection.onclose((error) => {
      this._connectionStatus.set({ status: ConnectionStatus.Disconnected, message: error?.message });
      this.startReloadSequence();
    });

    //Monitor emmited data
    this._hubConnection.on('displayUpdate', (update: DisplayUpdate) => {
      let truckData = null;
      let raceData = null;
      let rallyData = null;

      if (update?.data) {
        switch(update.type) {
          case DisplayType.TruckDashboard:
            truckData = update.data as TruckData;
            break;

          case DisplayType.RaceDashboard:
            raceData = update.data as RaceData;
            break;

          case DisplayType.RallyDashboard:
            rallyData = update.data as RallyData;
            break;
        }
      }

      this._truckData.set(truckData);
      this._raceData.set(raceData);
      this._rallyData.set(rallyData);
      this._page.set(update.page);
    });
  }

  private startReloadSequence(): void {
    let countDownSeconds = 10;
    this._connectionStatus.update((value) => ({ ...value, reloadSeconds: countDownSeconds}));
    interval(1000).pipe(
      take(countDownSeconds + 1),
      tap(() => {
        this._connectionStatus.update((value) => ({ ...value, reloadSeconds: countDownSeconds }));
        countDownSeconds--;
      }),
      filter(() => countDownSeconds <= 0),
      tap(() => window.location.reload()),
    ).subscribe();
  }
}
