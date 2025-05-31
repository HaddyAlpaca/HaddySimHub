import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { ConnectionStatus, SignalRService } from 'src/app/signalr.service';

@Component({
  selector: 'app-connection-status',
  templateUrl: './connection-status.component.html',
  styleUrl: './connection-status.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ConnectionStatusComponent {
  private readonly _signalRService = inject(SignalRService);

  protected connectionStatusDescription = computed(() => {
    const statusDescriptions: Record<ConnectionStatus, string> = {
      [ConnectionStatus.Disconnected]: 'Disconnected',
      [ConnectionStatus.Connecting]: 'Connecting...',
      [ConnectionStatus.ConnectionError]: 'Error connecting',
      [ConnectionStatus.Connected]: 'Connected, waiting for game...',
    };

    return statusDescriptions[this._signalRService.connectionStatus().status] || 'Unknown';
  });

  protected connectionMessage = computed(() => this._signalRService.connectionStatus().message);

  protected reloadSeconds = computed(() => this._signalRService.connectionStatus().reloadSeconds);
}
