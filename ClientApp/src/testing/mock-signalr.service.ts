import { signal } from '@angular/core';
import { ConnectionInfo, ConnectionStatus, DisplayUpdate } from 'src/app/signalr.service';

export class MockSignalRService {
  public readonly connectionStatus = signal<ConnectionInfo>({ status: ConnectionStatus.Disconnected });
  public readonly displayData = signal({}as DisplayUpdate);
}
