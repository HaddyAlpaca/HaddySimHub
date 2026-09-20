import { LitElement, html, type TemplateResult } from 'lit';
import { customElement } from 'lit/decorators.js';
import { ConnectionStatus, type ConnectionInfo } from '../../sse.service';

@customElement('haddy-connection-status')
export class ConnectionStatusElement extends LitElement {
  private _info: ConnectionInfo = { status: ConnectionStatus.Disconnected };

  public set connectionInfo(info: ConnectionInfo) {
    this._info = info;
    this.requestUpdate();
  }

  protected createRenderRoot(): HTMLElement {
    return this;
  }

  protected render(): TemplateResult {
    const descriptions: Record<ConnectionStatus, string> = {
      [ConnectionStatus.Disconnected]: 'Disconnected',
      [ConnectionStatus.Connecting]: 'Connecting...',
      [ConnectionStatus.ConnectionError]: 'Error connecting',
      [ConnectionStatus.Connected]: 'Connected, waiting for game...',
    };
    return html`
      <div class="connection-state">${descriptions[this._info.status] ?? 'Unknown'}</div>
      ${this._info.message ? html`<code class="message">${this._info.message}</code>` : ''}
      ${this._info.reloadSeconds ? html`<div class="refresh-countdown">Refreshing page in ${this._info.reloadSeconds}...</div>` : ''}
    `;
  }
}
