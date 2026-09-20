import { LitElement, html, type TemplateResult } from 'lit';
import { customElement } from 'lit/decorators.js';

@customElement('haddy-clock')
export class ClockElement extends LitElement {
  private _currentTime = new Date();
  private _timerId?: number;

  public connectedCallback(): void {
    super.connectedCallback();
    this._timerId = window.setInterval(() => {
      this._currentTime = new Date();
      this.requestUpdate();
    }, 1000);
  }

  public disconnectedCallback(): void {
    super.disconnectedCallback();
    if (this._timerId !== undefined) {
      window.clearInterval(this._timerId);
    }
  }

  protected render(): TemplateResult {
    return html`<span id="currentTime">${this._currentTime.toLocaleTimeString([], {
      hour: '2-digit',
      minute: '2-digit',
      hour12: false,
    })}</span>`;
  }
}
