import { computed, Injectable, signal } from '@angular/core';
import { filter, interval, take, tap } from 'rxjs';
import { HttpTransportType, HubConnection, HubConnectionBuilder, IHttpConnectionOptions, LogLevel } from '@microsoft/signalr';

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

export enum DisplayType {
  None,
  TruckDashboard,
  RaceDashboard,
  RallyDashboard,
}

export interface DisplayUpdate {
  type: DisplayType;
  data?: unknown;
}

@Injectable({
  providedIn: 'root',
})
export class GameDataService {
  private _hubConnection: HubConnection;

  private _connectionStatus = signal<ConnectionInfo>({ status: ConnectionStatus.Disconnected });
  public connectionStatus = computed(() => this._connectionStatus());

  private readonly _displayUpdate = signal<DisplayUpdate>({ type: DisplayType.None });
  public readonly displayUpdate = computed(() => this._displayUpdate());

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
      this._displayUpdate.set(update);
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
