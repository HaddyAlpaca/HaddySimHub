/* eslint-disable max-len */
import { LitElement, html, type TemplateResult } from 'lit';
import { customElement } from 'lit/decorators.js';
import type { TruckData } from './truck-data';
import '../../shared/gauge/gauge.element';

const minutesPerDay = 24 * 60;

@customElement('haddy-truck-display')
export class TruckDisplayElement extends LitElement {
  private _data?: TruckData;
  public set data(value: TruckData | undefined) {
    this._data = value; this.requestUpdate();
  }
  protected createRenderRoot(): HTMLElement {
    return this;
  }

  protected render(): TemplateResult {
    const data = this._data;
    if (!data) {
      return html``;
    }
    const brightness = Math.max(0.3, Math.min(data.dashboardBacklight || 0.3, 1));
    this.style.setProperty('--dashboard-brightness', `${brightness}`);
    const arrival = data.timeRemaining > 0 ? this.clock(data.gameTime + data.timeRemaining) : '';
    const restRatio = data.timeRemaining > 0 ? Math.min(data.restTimeRemaining / data.timeRemaining, 1) : 1;
    const gearAdvice = data.recommendedGear && data.recommendedGear !== data.gear ? data.recommendedGear : '';
    const shift = gearAdvice && Number(gearAdvice) > Number(data.gear) ? 'up' : gearAdvice ? 'down' : '';
    return html`
      <div class="top-cards">
        ${this.card('Route', html`<div class="card-header"><h2 class="card-title">Route</h2><span class="game-time" id="gameTime">${this.clock(data.gameTime)}</span></div>
          ${this.row('Vertrekpunt', this.waypoint(data.sourceCity, data.sourceCompany))}
          ${this.row('Bestemming', this.waypoint(data.destinationCity, data.destinationCompany))}
          ${this.row('Resterend', data.timeRemaining ? html`${this.timespan(data.timeRemaining)} / ${this.number(data.distanceRemaining)} km <span class="arrival-time" id="arrivalTime">&rarr; aankomst ${arrival}</span>` : '-')}
          ${this.row('Volgend rustmoment', data.restTimeRemaining ? html`${this.timespan(data.restTimeRemaining)}<div class="rest-bar-track"><div class="rest-bar-fill ${restRatio < 1 ? 'rest-critical' : 'rest-ok'}" style="width:${restRatio * 100}%"></div></div>` : '-')} `)}
        ${this.card('Opdracht', html`
          ${this.row('Truck', html`<span id="truckName">${data.truckName || '-'}</span>`)}
          ${this.row('Deadline', data.jobTimeRemaining ? this.timespan(data.jobTimeRemaining) : '-')}
          ${this.row('Inkomen', html`<span id="jobIncome">${data.jobIncome ? `€ ${this.number(data.jobIncome)}` : '-'}</span>`)}
          ${this.row('Lading', html`<span id="jobCargoName">${data.jobCargoName ? `${data.jobCargoName} (${this.number(data.jobCargoMass)} kg) · ${data.jobCargoDamage}% schade` : '-'}</span>`)}
        `)}
        ${this.damageCard('Truck schade', [['Motor', data.damageTruckEngine, 'damageTruckEngine'], ['Transmissie', data.damageTruckTransmission, 'damageTruckTransmission'], ['Cabine', data.damageTruckCabin, 'damageTruckCabin'], ['Chassis', data.damageTruckChassis, 'damageTruckChassis'], ['Wielen', data.damageTruckWheels, 'damageTruckWheels']], data.odometer)}
        ${data.numberOfTrailersAttached > 0 ? this.damageCard('Trailer schade', [['Chassis', data.damageTrailerChassis, 'damageTrailerChassis'], ['Opbouw', data.damageTrailerBody, 'damageTrailerBody'], ['Wielen', data.damageTrailerWheels, 'damageTrailerWheels'], ['Lading', data.damageTrailerCargo, 'damageTrailerCargo']]) : this.card('Trailer schade', 'Geen trailer gekoppeld')}
      </div>
      <div class="metrics-row">
        <div class="card gauge-card"><haddy-gauge .value=${data.rpm} .max=${data.rpmMax} .greenStart=${1250} .greenEnd=${2000} .redlineFraction=${0.85}><label class="unit text-center rpm-scale">x 100 RPM</label><div class="gear-row"><div class="gear text-center">${data.gear}</div><div id="gear-advice" class="gear-advice ${gearAdvice ? '' : 'hidden'}"><span id="shift-arrow" class="shift-arrow shift-arrow-${shift}"></span>${gearAdvice}</div></div></haddy-gauge></div>
        <div class="card gauge-card"><div class="speed-display"><div class="speed text-center">${data.speed}</div><label class="unit text-center">Km/h</label><div class="speed-limit ${data.speedLimit && data.speed > data.speedLimit ? 'over-speed-limit' : ''}">${data.speedLimit || '--'}</div></div></div>
      </div>
      <div class="bottom-row lights-full-row"><div class="card lights-card">${this.light('parking-lights', data.parkingLightsOn)}${this.light('low-beam', data.lowBeamOn)}${this.light('high-beam', data.highBeamOn)}${this.light('indicator-left', data.blinkerLeftOn)}${this.light('hazard-lights', data.hazardLightsOn)}${this.light('indicator-right', data.blinkerRightOn)}${this.light('brake-warning', data.parkingBrakeOn)}${this.light('wipers', data.wipersOn)}${this.light('differentialLock', data.differentialLock)}${this.light('engine', data.engineOn)}${this.light('motor-brake', data.motorBrakeOn)}${this.light('beacon', data.beaconOn)}${this.light('lift-axle', data.liftAxleIndicatorOn)}${this.light('air-pressure', data.airPressureWarningOn || data.airPressureEmergencyOn)}</div></div>
      <div class="bottom-row"><div class="card combined-controls-card">${this.row('Fuel', html`<span id="fuel">${this.number(data.fuelDistance)} km (${this.number(data.fuelAmount)} l)</span><div class="fuel-bar-track"><div class="fuel-bar-fill ${data.fuelDistance < data.distanceRemaining ? 'fuel-range-too-short' : ''}" style="width:${this.percent(data.fuelAmount, data.fuelCapacity)}%"></div></div>`)}${this.row('AdBlue', html`<span id="adBlue">${this.number(data.adBlueAmount)} l</span>`)}${this.row('Acceleration', `${data.throttle} %`)}</div><div class="card lights-card">${this.row('Battery', `${this.number(data.batteryVoltage)} V`)}${this.row('Water', `${this.number(data.waterTemp)} °C`)}${this.row('Oil pressure', `${this.number(data.oilPressure)} PSI`)}${this.row('Brake air', `${this.number(data.brakeAirPressure)} PSI`)}</div></div>
    `;
  }

  private card(title: string, content: unknown): TemplateResult {
    return html`<div class="card"><div class="card-header"><h2 class="card-title">${title}</h2></div><div class="card-body">${content}</div></div>`;
  }
  private row(label: string, value: unknown): TemplateResult {
    return html`<div class="data-row"><div class="data-label">${label}</div><div class="data-item">${value}</div></div>`;
  }
  private waypoint(city: string, company: string): string {
    return city ? `${city}${company ? ` (${company})` : ''}` : '-';
  }
  private damageCard(title: string, values: [string, number, string][], odometer?: number): TemplateResult {
    return this.card(title, html`<div class="damage-grid">${values.map(([label, value, id]) => html`<div><div class="data-label">${label}</div><div class="data-item ${this.damageClass(value)}" id=${id}>${value} %</div></div>`)}</div>${odometer !== undefined ? this.row('Kilometerteller', odometer > 0 ? `${this.number(odometer)} km` : '-') : ''}`);
  }
  private light(id: string, on: boolean): TemplateResult {
    return html`<span class="dashboard-light ${on ? 'filter-green' : ''}" id=${id}>●</span>`;
  }
  private damageClass(value: number): string {
    return value >= 50 ? 'damage-critical' : value >= 25 ? 'damage-warning' : '';
  }
  private percent(value: number, capacity: number): number {
    return capacity > 0 ? Math.round(value / capacity * 100) : 0;
  }
  private number(value: number): string {
    return new Intl.NumberFormat('nl-NL', { maximumFractionDigits: 1 }).format(value);
  }
  private timespan(value: number): string {
    return `${Math.floor(value / 60)}:${(value % 60).toString().padStart(2, '0')}`;
  }
  private clock(value: number): string {
    const minutes = ((value % minutesPerDay) + minutesPerDay) % minutesPerDay; return `${Math.floor(minutes / 60).toString().padStart(2, '0')}:${(minutes % 60).toString().padStart(2, '0')}`;
  }
}
