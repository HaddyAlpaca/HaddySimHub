import { LitElement, html, type TemplateResult } from 'lit';
import { customElement } from 'lit/decorators.js';
import { DisplayType, SseService, type ConnectionInfo } from './sse.service';
import { AppStore } from './state/app.store';
import './shared/clock/clock.element';
import './shared/connection-status/connection-status.element';
import './displays/rally-display/rally-display.element';
import './displays/race-display/race-display.element';
import './displays/truck-display/truck-display.element';
import './displays/flight-display/flight-display.element';

@customElement('haddy-app')
export class AppElement extends LitElement {
  private readonly _store = new AppStore();
  private readonly _sse = new SseService((update) => this._store.updateDisplay(update));
  private _connectionInfo: ConnectionInfo = { status: 0 };
  private _unsubscribeStore?: () => void;
  private _unsubscribeSse?: () => void;

  public connectedCallback(): void {
    super.connectedCallback();
    this._unsubscribeStore = this._store.subscribe(() => this.requestUpdate());
    this._unsubscribeSse = this._sse.subscribe((info) => {
      this._connectionInfo = info;
      this.requestUpdate();
    });
  }

  public disconnectedCallback(): void {
    this._unsubscribeStore?.();
    this._unsubscribeSse?.();
    this._sse.dispose();
    super.disconnectedCallback();
  }

  protected createRenderRoot(): HTMLElement {
    return this;
  }

  protected render(): TemplateResult {
    const data = this._store.data;
    return html`
      <div class="app-layout">
        <div class="header"><haddy-clock></haddy-clock></div>
        <div class="display">
          ${this._store.displayType === DisplayType.TruckDashboard ? html`<haddy-truck-display .data=${data}></haddy-truck-display>` : ''}
          ${this._store.displayType === DisplayType.RaceDashboard ? html`<haddy-race-display .data=${data}></haddy-race-display>` : ''}
          ${this._store.displayType === DisplayType.RallyDashboard ? html`<haddy-rally-display .data=${data}></haddy-rally-display>` : ''}
          ${this._store.displayType === DisplayType.FlightDashboard ? html`<haddy-flight-display .data=${data}></haddy-flight-display>` : ''}
          ${this._store.displayType === DisplayType.None ? html`<haddy-connection-status .connectionInfo=${this._connectionInfo}></haddy-connection-status>` : ''}
        </div>
      </div>
    `;
  }
}
