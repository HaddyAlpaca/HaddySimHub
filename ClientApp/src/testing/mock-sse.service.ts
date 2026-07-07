import { signal } from '@angular/core';
import { ConnectionInfo, ConnectionStatus } from '../app/sse.service';

export class MockSseService {
  public readonly connectionStatus = signal<ConnectionInfo>({ status: ConnectionStatus.Disconnected });
}
