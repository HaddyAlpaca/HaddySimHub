import { LitElement, html, type TemplateResult } from 'lit';
import { customElement } from 'lit/decorators.js';
import type { RallyData } from './rally-data';
import '../../shared/speedometer/speedometer.element';

const emptyData: RallyData = {
  speed: 0,
  gear: '',
  rpm: 0,
  rpmMax: 0,
  distanceTravelled: 0,
  completedPct: 0,
  sector1Time: 0,
  sector2Time: 0,
  lapTime: 0,
  position: 0,
};

const lapTime = (seconds: number): string => {
  if (seconds === 0) {
    return '--:--.---';
  }

  const minutes = Math.floor(seconds / 60);
  const fullSeconds = Math.floor(seconds % 60);
  const fraction = Math.round((seconds - fullSeconds - (minutes * 60)) * 1000);
  return `${minutes.toString().padStart(2, '0')}:${fullSeconds.toString().padStart(2, '0')}.${fraction.toString().padStart(3, '0')}`;
};

@customElement('haddy-rally-display')
export class RallyDisplayElement extends LitElement {
  private _data: RallyData = emptyData;

  public set data(value: RallyData) {
    this._data = value;
    this.requestUpdate();
  }

  protected createRenderRoot(): HTMLElement {
    return this;
  }

  protected render(): TemplateResult {
    const data = this._data;
    return html`
      <div class="container">
        <div class="progress-section">
          <div class="progress-header">
            <label>Stage Progress</label>
            <span class="completion-percentage">${data.completedPct}%</span>
          </div>
          <div class="progress-bar-container">
            <div class="progress-bar" style="width: ${data.completedPct}%"></div>
            <div class="progress-bar-glow"></div>
          </div>
          <div class="distance-info">
            <span class="label">Distance</span>
            <span class="value">${data.distanceTravelled} m</span>
          </div>
        </div>

        <div class="speedometer-section ${data.rpm >= data.rpmMax ? 'max-rpm' : ''}">
          <haddy-speedometer .rpm=${data.rpm} .gear=${data.gear} .speed=${data.speed}></haddy-speedometer>
        </div>

        <div class="sectors-section">
          <div class="sector-card">
            <label>Sector 1</label>
            <div class="sector-time">${lapTime(data.sector1Time)}</div>
          </div>
          <div class="sector-card">
            <label>Sector 2</label>
            <div class="sector-time">${lapTime(data.sector2Time)}</div>
          </div>
          <div class="sector-card stage-running">
            <label>Stage Running</label>
            <div class="sector-time">${lapTime(data.lapTime)}</div>
          </div>
        </div>
      </div>
    `;
  }
}
