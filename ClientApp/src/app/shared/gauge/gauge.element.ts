import { LitElement, html, type TemplateResult } from 'lit';
import { customElement } from 'lit/decorators.js';

@customElement('haddy-gauge')
export class GaugeElement extends LitElement {
  private _value = 0;
  private _max = 0;
  private _greenStart = 0;
  private _greenEnd = 0;
  private _redlineFraction = 0.95;

  public set value(value: number) {
    this._value = value; this.requestUpdate();
  }
  public set max(value: number) {
    this._max = value; this.requestUpdate();
  }
  public set greenStart(value: number) {
    this._greenStart = value; this.requestUpdate();
  }
  public set greenEnd(value: number) {
    this._greenEnd = value; this.requestUpdate();
  }
  public set redlineFraction(value: number) {
    this._redlineFraction = value; this.requestUpdate();
  }

  protected createRenderRoot(): HTMLElement {
    return this;
  }

  protected render(): TemplateResult {
    const max = this._max;
    const zoneStart = this.angle(Math.min(Math.max(this._redlineFraction, this._greenEnd / Math.max(max, 1)), 1));
    const zones = max > 0 && this._greenStart > 0 && this._greenEnd > this._greenStart
      ? [
        { path: this.arc(this.angle(this._greenStart / max), this.angle(this._greenEnd / max)), color: '#00cc66' },
        { path: this.arc(this.angle(this._greenEnd / max), zoneStart), color: '#ffcc00' },
        { path: this.arc(zoneStart, 360), color: '#ff3300' },
      ]
      : max > 0 && this._redlineFraction < 1
        ? [{ path: this.arc(this.angle(this._redlineFraction), 360), color: '#ff3300' }]
        : [];
    const step = max > 0 ? this.niceStep(max) : 1;
    const ticks = max > 0 ? Array.from({ length: Math.ceil(max / (step / 2)) + 1 }, (_, i) => {
      const rpm = Math.min(i * step / 2, max);
      const point = this.point(this.angle(rpm / max));
      const inner = this.point(this.angle(rpm / max), Math.abs(rpm % step) < 0.001 ? 67 : 71);
      return { ...point, ...inner, major: Math.abs(rpm % step) < 0.001 };
    }) : [];
    const labels = max > 0 ? Array.from({ length: Math.floor(max / step) + 1 }, (_, i) => {
      const rpm = Math.min(i * step, max);
      const point = this.point(this.angle(rpm / max), 58);
      return { ...point, text: (rpm / 100).toFixed(0) };
    }) : [];

    return html`
      <div class="ring ${this._value >= max ? 'max-rpm' : ''}">
        <svg viewBox="0 0 200 200" class="gauge-svg">
          <defs><linearGradient id="needleGrad" x1="0" y1="1" x2="0" y2="0"><stop offset="0%" stop-color="#27293d"/><stop offset="50%" stop-color="#888"/><stop offset="100%" stop-color="white"/></linearGradient></defs>
          <path fill="none" stroke="#27293d" stroke-width="12" stroke-linecap="butt" d=${this.arc(235, 485)} />
          ${zones.map(zone => html`<path fill="none" stroke-width="12" stroke-linecap="butt" d=${zone.path} stroke=${zone.color} class=${zone.color === '#ff3300' ? 'red-zone' : ''} />`)}
          ${ticks.map(tick => html`<line stroke="white" x1=${tick.x1} y1=${tick.y1} x2=${tick.x2} y2=${tick.y2} stroke-width=${tick.major ? 2.5 : 1.5} />`)}
          ${labels.map(label => html`<text text-anchor="middle" dominant-baseline="central" fill="white" font-size="9" font-family="Poppins, sans-serif" x=${label.x1} y=${label.y1}>${label.text}</text>`)}
          <g class="needle-group" transform="rotate(${this._max > 0 ? Math.round(-125 + Math.min(this._value / this._max, 1) * 250) : 0}, 100, 100)"><rect x="95" y="18" width="10" height="82" rx="4" class="needle-shape" /></g>
          <circle cx="100" cy="100" r="10" fill="#27293d" stroke="white" stroke-width="2" />
        </svg>
        <div class="content"><slot></slot></div>
      </div>`;
  }

  private angle(fraction: number): number {
    return 235 + fraction * 250;
  }
  private toSvgDeg(deg: number): number {
    return (deg + 270) % 360;
  }
  private point(deg: number, radius = 75): { x1: number; y1: number; x2?: number; y2?: number } {
    const rad = this.toSvgDeg(deg) * Math.PI / 180;
    return { x1: +(100 + radius * Math.cos(rad)).toFixed(1), y1: +(100 + radius * Math.sin(rad)).toFixed(1) };
  }
  private arc(start: number, end: number, radius = 82): string {
    const a = this.point(start, radius);
    const b = this.point(end, radius);
    return `M ${a.x1} ${a.y1} A ${radius} ${radius} 0 ${end - start > 180 ? 1 : 0} 1 ${b.x1} ${b.y1}`;
  }
  private niceStep(max: number): number {
    const raw = max / 6;
    const magnitude = 10 ** Math.floor(Math.log10(raw));
    const residual = raw / magnitude;
    return residual <= 1.5 ? magnitude : residual <= 3.5 ? 2 * magnitude : residual <= 7.5 ? 5 * magnitude : 10 * magnitude;
  }
}
