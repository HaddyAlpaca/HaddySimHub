import type { FlightData } from './displays/flight-display/flight-data';
import type { RaceData } from './displays/race-display/race-data';
import type { RallyData } from './displays/rally-display/rally-data';
import type { TruckData } from './displays/truck-display/truck-data';

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
  FlightDashboard,
}

export interface DisplayUpdate {
  type: DisplayType;
  data: TruckData | RaceData | RallyData | FlightData | undefined;
}

export interface ConnectionInfo {
  status: ConnectionStatus;
  message?: string;
  reloadSeconds?: number;
}

export class SseService {
  private _eventSource?: EventSource;
  private _reloadTimer?: number;
  private _connectionInfo: ConnectionInfo = { status: ConnectionStatus.Disconnected };
  private readonly _listeners = new Set<(info: ConnectionInfo) => void>();

  public constructor(private readonly _onDisplayUpdate: (update: DisplayUpdate) => void) {
    this.connect();
  }

  public subscribe(listener: (info: ConnectionInfo) => void): () => void {
    this._listeners.add(listener);
    listener(this._connectionInfo);
    return () => this._listeners.delete(listener);
  }

  public dispose(): void {
    if (this._reloadTimer !== undefined) {
      window.clearInterval(this._reloadTimer);
    }
    this._eventSource?.close();
  }

  private connect(): void {
    this.setConnectionInfo({ status: ConnectionStatus.Connecting });
    this._eventSource = new EventSource('/display-data/stream');
    this._eventSource.onopen = (): void => this.setConnectionInfo({ status: ConnectionStatus.Connected });
    this._eventSource.onmessage = (event: MessageEvent<string>): void => {
      this._onDisplayUpdate(JSON.parse(event.data) as DisplayUpdate);
    };
    this._eventSource.onerror = (): void => {
      if (this._eventSource?.readyState === EventSource.CONNECTING) {
        this.setConnectionInfo({ status: ConnectionStatus.Connecting, message: 'Reconnecting...' });
      } else {
        this.setConnectionInfo({ status: ConnectionStatus.ConnectionError, message: 'Connection lost' });
        this.startReloadSequence();
      }
    };
  }

  private startReloadSequence(): void {
    if (this._reloadTimer !== undefined) {
      window.clearInterval(this._reloadTimer);
    }
    let seconds = 10;
    this.setConnectionInfo({ ...this._connectionInfo, reloadSeconds: seconds });
    this._reloadTimer = window.setInterval(() => {
      seconds--;
      this.setConnectionInfo({ ...this._connectionInfo, reloadSeconds: seconds });
      if (seconds <= 0) {
        window.location.reload();
      }
    }, 1000);
  }

  private setConnectionInfo(info: ConnectionInfo): void {
    this._connectionInfo = info;
    this._listeners.forEach((listener) => listener(info));
  }
}
