import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { ConnectionInfo, ConnectionStatus } from 'src/app/game-data.service';

@Component({
  selector: 'app-connection-status',
  templateUrl: './connection-status.component.html',
  styleUrl: './connection-status.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ConnectionStatusComponent {
  public status = input<ConnectionInfo>({ status: ConnectionStatus.Disconnected });

  protected connectionStatusDescription = computed(() => {
    const statusDescriptions: Record<ConnectionStatus, string> = {
      [ConnectionStatus.Disconnected]: 'Disconnected',
      [ConnectionStatus.Connecting]: 'Connecting...',
      [ConnectionStatus.ConnectionError]: 'Error connecting',
      [ConnectionStatus.Connected]: 'Connected, waiting for game...',
    };

    return statusDescriptions[this.status().status] || 'Unknown';
  });

  protected connectionMessage = computed(() => this.status().message);

  protected reloadSeconds = computed(() => this.status().reloadSeconds);
}
