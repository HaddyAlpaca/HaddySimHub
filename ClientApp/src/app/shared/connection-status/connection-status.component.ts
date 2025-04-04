import { Component, computed, input } from '@angular/core';
import { ConnectionInfo, ConnectionStatus } from 'src/app/game-data.service';

@Component({
  selector: 'app-connection-status',
  templateUrl: './connection-status.component.html',
  styleUrl: './connection-status.component.scss',
})
export class ConnectionStatusComponent {
  public status = input<ConnectionInfo>({ status: ConnectionStatus.Disconnected });

  public connectionStatusDescription = computed(() => {
    const statusDescriptions: Record<ConnectionStatus, string> = {
      [ConnectionStatus.Disconnected]: 'Disconnected',
      [ConnectionStatus.Connecting]: 'Connecting...',
      [ConnectionStatus.ConnectionError]: 'Error connecting',
      [ConnectionStatus.Connected]: 'Connected, waiting for game...',
    };

    return statusDescriptions[this.status().status] || 'Unknown';
  });

  public connectionMessage = computed(() => this.status().message);

  public reloadSeconds = computed(() => this.status().reloadSeconds);
}
