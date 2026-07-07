import { Component, computed, inject } from '@angular/core';
import { ConnectionStatus, SseService } from '../../sse.service';

@Component({
  selector: 'app-connection-status',
  templateUrl: './connection-status.component.html',
  styleUrl: './connection-status.component.scss',
})
export class ConnectionStatusComponent {
  private readonly _sseService = inject(SseService);

  protected readonly connectionStatusDescription = computed(() => {
    const statusDescriptions: Record<ConnectionStatus, string> = {
      [ConnectionStatus.Disconnected]: 'Disconnected',
      [ConnectionStatus.Connecting]: 'Connecting...',
      [ConnectionStatus.ConnectionError]: 'Error connecting',
      [ConnectionStatus.Connected]: 'Connected, waiting for game...',
    };

    return statusDescriptions[this._sseService.connectionStatus().status] || 'Unknown';
  });

  protected readonly connectionMessage = computed(() => this._sseService.connectionStatus().message);

  protected readonly reloadSeconds = computed(() => this._sseService.connectionStatus().reloadSeconds);
}
