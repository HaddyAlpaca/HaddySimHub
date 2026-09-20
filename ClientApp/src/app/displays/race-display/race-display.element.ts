import { LitElement, html, type TemplateResult } from 'lit';
import { customElement } from 'lit/decorators.js';
import type { RaceData } from './race-data';
import '../../shared/speedometer/speedometer.element';
import './telemetry-trace.element';

@customElement('haddy-race-display')
export class RaceDisplayElement extends LitElement {
  private _data?: RaceData;

  public set data(value: RaceData | undefined) {
    this._data = value;
    if (value) {
      this._telemetrySample = {
        brakePct: value.brakePct,
        throttlePct: value.throttlePct,
        steeringPct: value.steeringPct,
      };
    }
    this.requestUpdate();
  }

  private _telemetrySample = { brakePct: 0, throttlePct: 0, steeringPct: 50 };

  protected createRenderRoot(): HTMLElement {
    return this;
  }

  protected render(): TemplateResult {
    const data = this._data;
    if (!data) {
      return html``;
    }

    const laps = data.isLimitedTime && !data.isLimitedSessionLaps
      ? `${data.currentLap}`
      : data.totalLaps > 0 ? `${data.currentLap}/${data.totalLaps}` : `${data.currentLap}`;
    const remainingFuelWarning = data.totalLaps > 0 && data.totalLaps - data.currentLap > data.fuelEstLaps;

    return html`
      <div class="dashboard-container">
        <div class="top-bar">
          ${this.topItem('Session', data.sessionType)}
          ${data.strengthOfField != null ? this.topItem('SOF', data.strengthOfField) : ''}
          ${data.safetyRating != null ? this.topItem('Safety Rating', data.safetyRating, 'safetyRating') : ''}
          ${this.topItem('Time left', formatTime(data.sessionTimeRemaining))}
          ${this.topItem('Air', `${formatNumber(data.airTemp)}°`, 'air-temp')}
          ${this.topItem('Track', `${formatNumber(data.trackTemp)}°`, 'track-temp')}
          ${data.windSpeed != null ? this.topItem('Wind', html`${formatNumber(data.windSpeed, 1)}<small>m/s</small>`, 'wind-speed') : ''}
          ${data.rainIntensity != null && data.rainIntensity > 0
            ? this.topItem('Rain', rainLabel(data.rainIntensity), 'rain-intensity', 'text-rain') : ''}
          ${data.trackGripStatus != null
            ? this.topItem('Grip', gripLabel(data.trackGripStatus), 'track-grip', data.trackGripStatus === 3 ? 'text-warning' : '') : ''}
        </div>
        <div class="main-area">
          <div class="main-grid">
            <div class="panel info-panel">
              ${data.incidents !== undefined ? html`<div class="info-row"><label>Incidents</label><span id="incidents">${
                data.maxIncidents !== undefined && data.maxIncidents !== 999
                  ? `${data.incidents}/${data.maxIncidents}` : data.incidents
              }</span></div>` : ''}
              ${data.irating !== undefined ? html`<div class="info-row"><label>iR</label><span id="irating">${formatIRating(data.irating)}</span></div>` : ''}
              ${data.position !== undefined ? html`<div class="info-row"><label>Pos</label><span id="position" class="text-accent">P${data.position}</span></div>` : ''}
              ${data.expectedPosition !== undefined
                ? html`<div class="info-row"><label>Expected pos</label><span id="expectedPosition"
                  class="text-accent">P${data.expectedPosition}</span></div>` : ''}
              <div class="info-row"><label>Lap</label><span id="laps">${laps}</span></div>
              ${data.currentLapTime ? this.lapRow('Current lap', formatLapTime(data.currentLapTime), 'currentLapTime') : ''}
              ${this.lapRow('Last lap', formatLapTime(data.lastLapTime), 'lastLapTime', data.lastLapTimeDelta)}
              ${data.bestLapTime !== undefined
                ? this.lapRow('Best lap', formatLapTime(data.bestLapTime), 'bestLapTime', data.bestLapTimeDelta, 'best') : ''}
            </div>
            <div class="panel speedometer-panel ${data.rpm >= data.rpmMax ? 'max-rpm' : ''}">
              <haddy-speedometer .rpm=${data.rpm} .gear=${data.gear} .speed=${data.speed}></haddy-speedometer>
            </div>
            <div class="panel fuel-panel">
              ${data.fuelRemaining !== undefined ? html`<div class="fuel-primary"><label>Fuel</label><span id="fuelRemaining">${formatNumber(data.fuelRemaining)}<small>L</small></span></div>` : ''}
              <div class="info-row"><label>Est. laps</label><span id="fuelEstLaps" class="${remainingFuelWarning ? 'text-danger' : ''}">${formatNumber(data.fuelEstLaps)}</span></div>
              ${data.fuelLastLap !== undefined ? html`<div class="info-row"><label>Last lap</label><span id="fuelLastLap">${formatNumber(data.fuelLastLap)}L</span></div>` : ''}
              ${data.fuelAvgLap !== undefined ? html`<div class="info-row"><label>Avg lap</label><span id="fuelAvgLap">${formatNumber(data.fuelAvgLap)}L</span></div>` : ''}
              ${data.brakeBias !== undefined ? html`<div class="fuel-primary"><label>Brake bias</label><span id="brakeBias">${formatNumber(data.brakeBias)}<small>%</small></span></div>` : ''}
            </div>
          </div>
          <haddy-telemetry-trace .telemetrySample=${this._telemetrySample}></haddy-telemetry-trace>
        </div>
      </div>
      ${data.pitLimiterOn ? html`<div class="pit-limiter">PIT LIMITER</div>` : ''}
    `;
  }

  private topItem(label: string, value: unknown, id?: string, className?: string): TemplateResult {
    return html`<div class="top-bar-item"><label>${label}</label><span id=${id ?? ''} class=${className ?? ''}>${value}</span></div>`;
  }

  private lapRow(label: string, value: string, id: string, delta?: number, extraClass = ''): TemplateResult {
    const deltaClass = delta === undefined ? 'delta-placeholder' : delta < 0 ? 'text-positive' : delta > 0 ? 'text-negative' : '';
    const deltaMarkup = label === 'Current lap'
      ? html`<span class="delta-value delta-placeholder"></span>`
      : delta !== undefined ? html`<span class="delta-value ${deltaClass}" id=${id}Delta>${formatDelta(delta)}</span>` : '';

    return html`
      <div class="lap-row">
        <label>${label}</label>
        <span class="lap-values">
          <span class="lap-value ${extraClass}" id=${id}>${value}</span>${deltaMarkup}
        </span>
      </div>`;
  }
}

const formatNumber = (value: number, fractionDigits = 1): string => new Intl.NumberFormat('en-US', { minimumFractionDigits: fractionDigits, maximumFractionDigits: fractionDigits }).format(value);
const formatDelta = (value: number): string => `${value >= 0 ? '+' : ''}${new Intl.NumberFormat('en-US', { minimumFractionDigits: 3 }).format(value)}`;
const formatLapTime = (seconds: number): string => {
  if (!seconds) {
    return '--:--.---';
  }
  const minutes = Math.floor(seconds / 60);
  return `${minutes.toString().padStart(2, '0')}:${(seconds % 60).toFixed(3).padStart(6, '0')}`;
};
const formatTime = (seconds: number): string => {
  if (seconds === 168 * 3600) {
    return '--:--:--';
  }
  const sign = seconds < 0 ? '-' : '';
  const total = Math.abs(seconds);
  const hours = Math.floor(total / 3600);
  const minutes = Math.floor(total % 3600 / 60);
  const secs = Math.floor(total % 60);
  if (hours) {
    return `${sign}${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
  }

  return `${sign}${minutes.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
};
const formatIRating = (value: number): string => value < 1_000 ? `${value}` : `${(value / 1_000).toFixed(1).replace('.0', '')}k`;
const rainLabel = (value: number): string => value >= 3 ? 'Heavy' : value >= 2 ? 'Medium' : 'Light';
const gripLabel = (value: number): string => ['Green', 'Fast', 'Optimum', 'Wet'][value] ?? 'Unknown';
