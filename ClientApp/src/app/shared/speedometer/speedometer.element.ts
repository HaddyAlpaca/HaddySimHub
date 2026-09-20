import { LitElement, html, type TemplateResult } from 'lit';
import { customElement } from 'lit/decorators.js';

@customElement('haddy-speedometer')
export class SpeedometerElement extends LitElement {
  private _rpm: number | string = 0;
  private _gear: string | number = '';
  private _speed: number | string = 0;

  public set rpm(value: number | string) {
    this._rpm = value;
    this.requestUpdate();
  }

  public set gear(value: string | number) {
    this._gear = value;
    this.requestUpdate();
  }

  public set speed(value: number | string) {
    this._speed = value;
    this.requestUpdate();
  }

  protected createRenderRoot(): HTMLElement {
    return this;
  }

  protected render(): TemplateResult {
    return html`
      <div class="rpm text-center">${this._rpm}</div>
      <label>RPM</label>
      <div class="gear">${this._gear}</div>
      <label>Gear</label>
      <div class="speed text-center">${this._speed}</div>
      <label>Km/h</label>
    `;
  }
}
