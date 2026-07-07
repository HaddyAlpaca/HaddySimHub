import { inject, OnDestroy, Service, signal } from '@angular/core';
import { filter, interval, Subscription, take, tap } from 'rxjs';
import { RaceData, RallyData, TruckData } from './displays';
import { APP_STORE } from './state/app.store';

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
  data: TruckData | RaceData | RallyData | undefined;
}

@Service()
export class SseService implements OnDestroy {
  private readonly _store = inject(APP_STORE);
  private _eventSource?: EventSource;
  private _reloadSubscription?: Subscription;

  private readonly _connectionStatus = signal<ConnectionInfo>({ status: ConnectionStatus.Disconnected });
  public readonly connectionStatus = this._connectionStatus.asReadonly();

  public constructor() {
    this.connect();
  }

  private connect(): void {
    this._connectionStatus.set({ status: ConnectionStatus.Connecting });

    this._eventSource = new EventSource('/display-data/stream');

    this._eventSource.onopen = (): void => {
      this._connectionStatus.set({ status: ConnectionStatus.Connected });
    };

    this._eventSource.onmessage = (event: MessageEvent): void => {
      const data = event.data as string;
      const update = JSON.parse(data) as DisplayUpdate;
      this._store.updateDisplay(update);
    };

    this._eventSource.onerror = (): void => {
      if (this._eventSource?.readyState === EventSource.CONNECTING) {
        this._connectionStatus.set({ status: ConnectionStatus.Connecting, message: 'Reconnecting...' });
        return;
      }
      this._connectionStatus.set({ status: ConnectionStatus.ConnectionError, message: 'Connection lost' });
      this.startReloadSequence();
    };
  }

  private startReloadSequence(): void {
    this._reloadSubscription?.unsubscribe();
    let countDownSeconds = 10;
    this._connectionStatus.update((value) => ({ ...value, reloadSeconds: countDownSeconds }));
    this._reloadSubscription = interval(1000).pipe(
      take(countDownSeconds + 1),
      tap(() => {
        this._connectionStatus.update((value) => ({ ...value, reloadSeconds: countDownSeconds }));
        countDownSeconds--;
      }),
      filter(() => countDownSeconds <= 0),
      tap(() => window.location.reload()),
    ).subscribe();
  }

  public ngOnDestroy(): void {
    this._reloadSubscription?.unsubscribe();
    this._eventSource?.close();
  }
}
