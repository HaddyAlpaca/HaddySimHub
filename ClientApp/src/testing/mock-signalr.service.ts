import { signal } from '@angular/core';
import { ConnectionInfo, ConnectionStatus } from 'src/app/signalr.service';

export class MockSignalRService {
  public readonly connectionStatus = signal<ConnectionInfo>({ status: ConnectionStatus.Disconnected });
}
