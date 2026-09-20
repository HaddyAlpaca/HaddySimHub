/* eslint-disable max-len */
import { LitElement, html, type TemplateResult } from 'lit';
import { customElement } from 'lit/decorators.js';
import { CourseDeviationSource, EngineType, NavToFrom, type FlightData } from './flight-data';

const number = (value: number | undefined, digits = 0): string =>
  new Intl.NumberFormat('en-US', { minimumFractionDigits: digits, maximumFractionDigits: digits }).format(value ?? 0);
const numberNl = (value: number): string => new Intl.NumberFormat('nl-NL', { maximumFractionDigits: 0 }).format(value);
const minutes = (value: number | undefined): string => value == null ? '' : `${Math.round(value / 60)} min`;

@customElement('haddy-flight-display')
export class FlightDisplayElement extends LitElement {
  private _data?: FlightData;
  public set data(value: FlightData | undefined) {
    this._data = value; this.requestUpdate();
  }
  protected createRenderRoot(): HTMLElement {
    return this;
  }

  protected render(): TemplateResult {
    const d = this._data;
    if (!d) {
      return html``;
    }
    const hasEngine = d.engineType !== EngineType.None && (d.engineCount ?? 0) > 0;
    const modes = d.autopilotMaster
      ? [d.headingHold && 'HDG', d.navHold && 'NAV', d.approachHold && 'APR', d.altitudeHold && 'ALT', d.speedHold && 'SPD'].filter(Boolean) as string[]
      : [];
    const guidance = d.deviationSource !== CourseDeviationSource.None;
    const gear = (d.gearPercentExtended ?? 0) >= 99 ? 'DOWN' : (d.gearPercentExtended ?? 0) <= 1 ? 'UP' : 'IN TRANSIT';
    const endurance = d.fuelEnduranceSeconds == null ? '' : `${Math.floor(d.fuelEnduranceSeconds / 3600)}:${Math.floor(d.fuelEnduranceSeconds % 3600 / 60).toString().padStart(2, '0')}`;
    const courseName = d.deviationSource === CourseDeviationSource.Gps ? 'GPS' : d.deviationSource === CourseDeviationSource.Vor ? 'VOR' : d.deviationSource === CourseDeviationSource.Localizer ? 'LOC' : '';
    const crossTrack = d.crossTrackErrorNm == null ? '' : `${Math.abs(d.crossTrackErrorNm).toFixed(2)} NM ${d.crossTrackErrorNm >= 0 ? 'right' : 'left'} of course`;
    return html`
      <div class="container">
        <div class="instrument-row">
          <div class="panel speed-panel ${d.stallWarning ? 'warning' : ''} ${d.overspeedWarning ? 'overspeed' : ''}"><label>Speed</label><div class="primary">${number(d.indicatedAirspeed)}<span class="unit">kts</span></div><div class="secondary"><span><em>GS</em> ${number(d.groundSpeed)}</span><span><em>TAS</em> ${number(d.trueAirspeed)}</span></div>${d.stallWarning ? html`<div class="alert">STALL</div>` : d.overspeedWarning ? html`<div class="alert">OVERSPEED</div>` : ''}</div>
          <div class="panel attitude-panel"><label>Attitude</label>${this.attitude(d.pitchDegrees ?? 0, d.bankDegrees ?? 0, d.slipBall ?? 0)}<div class="secondary"><span><em>PITCH</em> ${number(d.pitchDegrees, 1)}&deg;</span><span><em>BANK</em> ${number(d.bankDegrees, 1)}&deg;</span></div></div>
          <div class="panel altitude-panel"><label>Altitude</label><div class="primary">${numberNl(Math.round(d.indicatedAltitude ?? 0))}<span class="unit">ft</span></div><div class="secondary"><span class="${d.verticalSpeed > 50 ? 'climbing' : d.verticalSpeed < -50 ? 'descending' : ''}"><em>VS</em> ${number(d.verticalSpeed)} fpm</span><span><em>AGL</em> ${numberNl(Math.round(d.altitudeAboveGround ?? 0))}</span><span><em>QNH</em> ${Math.round(d.altimeterSettingHpa ?? 0)}</span></div></div>
        </div>
        <div class="heading-row">
          <div class="panel heading-panel"><label>Heading</label>${this.heading(d.headingMagnetic ?? 0, d.headingBug, d.groundTrack)}<div class="secondary"><span class="strong">${number(d.headingMagnetic, 0)}&deg;</span><span><em>TRK</em> ${number(d.groundTrack, 0)}&deg;</span><span><em>WIND</em> ${number(d.windDirection, 0)}&deg; / ${number(d.windSpeed)} kts</span></div></div>
          <div class="panel course-panel ${guidance ? '' : 'no-guidance'}"><label>Course</label>${guidance ? html`<div class="source-row"><span class="source">${courseName}</span>${d.deviationSourceId ? html`<span class="source-id">${d.deviationSourceId}</span>` : ''}${d.toFrom === NavToFrom.To ? html`<span class="to-from">TO</span>` : d.toFrom === NavToFrom.From ? html`<span class="to-from">FROM</span>` : ''}${d.selectedCourse != null ? html`<span class="reading"><em>CRS</em> ${number(d.selectedCourse)}&deg;</span>` : ''}${d.lateralFullScaleNm != null ? html`<span class="reading"><em>SCALE</em> &plusmn;${d.lateralFullScaleNm} NM</span>` : ''}</div>${this.course(d.lateralDeviation, d.glideslopeDeviation)}` : html`<div class="no-guidance-text">No lateral guidance</div>`}</div>
          <div class="panel autopilot-panel ${d.autopilotMaster ? 'engaged' : ''}"><label>Autopilot</label>${d.autopilotMaster ? html`<div class="mode-row">${(modes.length ? modes : ['CWS']).map(mode => html`<span class="mode ${modes.length ? '' : 'idle'}">${mode}</span>`)}</div><div class="secondary stacked"><span><em>ALT</em> ${numberNl(Math.round(d.altitudeTarget))} ft</span><span><em>HDG</em> ${number(d.headingBug)}&deg;</span>${d.speedHold ? html`<span><em>SPD</em> ${number(d.speedTarget)} kts</span>` : ''}</div>` : html`<div class="mode-row"><span class="mode off">AP OFF</span></div>`}</div>
        </div>
        <div class="panel nav-panel"><label>Navigation</label>${d.hasActiveFlightPlan ? html`<div class="nav-row"><div class="nav-leg"><span class="arrow">&rarr;</span><span class="ident">${d.nextWaypointId}</span><span class="distance">${number(d.distanceToWaypointNm, 1)} NM</span><span class="ete">${minutes(d.waypointEteSeconds)}</span></div><div class="nav-leg destination"><span class="arrow">&#9992;</span><span class="ident">${d.destinationId ?? ''}</span><span class="distance">${number(d.distanceToDestinationNm)} NM</span><span class="ete">${minutes(d.destinationEteSeconds)}</span><span class="eta">ETA ${d.destinationEtaUtcSeconds == null ? '' : `${Math.floor(d.destinationEtaUtcSeconds / 3600) % 24}:${Math.floor(d.destinationEtaUtcSeconds % 3600 / 60).toString().padStart(2, '0')}Z`}</span></div></div>${crossTrack ? html`<div class="xtk"><em>XTK</em> ${crossTrack}</div>` : ''}` : html`<div class="nav-empty">No active flight plan</div>`}</div>
        <div class="bottom-row">
          <div class="panel engine-panel"><label>Engine &amp; fuel</label>${hasEngine ? html`<div class="bar-row"><em>${d.engineType === EngineType.Piston ? '% RPM' : 'N1'}</em><div class="bar"><div class="bar-fill engine" style="width:${d.enginePrimaryPct ?? 0}%"></div></div><span class="bar-value">${number(d.enginePrimaryPct)}%</span></div>` : ''}<div class="bar-row ${d.fuelEnduranceSeconds != null && d.fuelEnduranceSeconds < 2700 ? 'warning' : ''}"><em>Fuel</em><div class="bar"><div class="bar-fill fuel" style="width:${d.fuelCapacityLbs ? Math.max(0, Math.min(100, d.fuelQuantityLbs / d.fuelCapacityLbs * 100)) : 0}%"></div></div><span class="bar-value">${numberNl(Math.round(d.fuelQuantityLbs))} lb</span></div><div class="secondary">${endurance ? html`<span><em>ENDUR</em> ${endurance}</span>` : ''}${hasEngine ? html`<span><em>FF</em> ${number(d.fuelFlowPph)} pph</span><span><em>OIL</em> ${number(d.oilTemperature)}&deg;C / ${number(d.oilPressure)} psi</span>` : ''}</div></div>
          <div class="panel config-panel"><label>Configuration</label><div class="config-grid"><div class="config-item"><em>Flaps</em><span>${(d.flapsHandleIndex ?? 0) === 0 ? 'UP' : d.flapsHandleIndex}<small> / ${Math.max(0, (d.flapsHandlePositions ?? 1) - 1)}</small></span></div><div class="config-item ${gear === 'DOWN' ? 'down' : gear === 'IN TRANSIT' ? 'transit' : ''}"><em>Gear</em><span>${gear}</span></div><div class="config-item ${d.spoilersPct > 0 ? 'active' : ''}"><em>Spoilers</em><span>${d.spoilersArmed && d.spoilersPct <= 0 ? 'ARMED' : `${Math.round(d.spoilersPct)}%`}</span></div><div class="config-item ${d.parkingBrakeOn ? 'active' : ''}"><em>Park brake</em><span>${d.parkingBrakeOn ? 'SET' : 'OFF'}</span></div><div class="config-item"><em>Trim</em><span>${number(d.elevatorTrimPct)}%</span></div><div class="config-item"><em>Aircraft</em><span class="aircraft">${d.aircraftTitle}</span></div></div><div class="light-row">${[['LAND', d.landingLightsOn], ['TAXI', d.taxiLightsOn], ['STROBE', d.strobeLightsOn], ['NAV', d.navLightsOn], ['BCN', d.beaconOn]].map(([name, on]) => html`<span class="light ${on ? 'on' : ''}">${name}</span>`)}</div></div>
        </div>
      </div>`;
  }

  private attitude(pitch: number, bank: number, slip: number): TemplateResult {
    const rungs = [-30, -20, -10, 10, 20, 30];
    return html`<svg viewBox="0 0 200 200" class="attitude"><defs><clipPath id="attitude-face"><circle cx="100" cy="100" r="88"/></clipPath></defs><g clip-path="url(#attitude-face)" transform="rotate(${-bank} 100 100) translate(0 ${(pitch * 2.4).toFixed(1)})"><rect x="-200" y="-500" width="600" height="600" class="sky"/><rect x="-200" y="100" width="600" height="600" class="ground"/><line x1="-200" y1="100" x2="400" y2="100" class="horizon-line"/>${rungs.map(degrees => html`<line class="rung" x1=${100 - (Math.abs(degrees) === 10 ? 14 : Math.abs(degrees) === 20 ? 22 : 30)} y1=${100 - degrees * 2.4} x2=${100 + (Math.abs(degrees) === 10 ? 14 : Math.abs(degrees) === 20 ? 22 : 30)} y2=${100 - degrees * 2.4}/>` )}</g><circle cx="100" cy="100" r="88" class="bezel"/><g class="aircraft"><line x1="52" y1="100" x2="84" y2="100"/><line x1="116" y1="100" x2="148" y2="100"/><circle cx="100" cy="100" r="3.5"/></g><circle cy="180" r="5.5" class="slip-ball" cx=${100 + Math.max(-1, Math.min(1, slip)) * 22}/></svg>`;
  }

  private heading(heading: number, bug: number, track: number): TemplateResult {
    const marker = (value: number): number => 300 + Math.max(-48, Math.min(48, ((value - heading + 540) % 360) - 180)) * 6;
    return html`<svg viewBox="0 0 600 64" class="heading-tape"><rect x="0" y="14" width="600" height="34" rx="4" class="tape-bg"/><polygon points="300,11 293,0 307,0" class="centre-pointer"/><line x1="300" y1="14" x2="300" y2="48" class="centre-line"/><polygon points="-7,9 7,9 7,16 0,21 -7,16" class="bug-marker" transform="translate(${marker(bug)} 0)"/><polygon points="0,43 6,49 0,55 -6,49" class="track-marker" transform="translate(${marker(track)} 0)"/></svg>`;
  }

  private course(lateral: number | undefined, glideslope: number | undefined): TemplateResult {
    const x = 170 - Math.max(-1, Math.min(1, lateral ?? 0)) * 120;
    return html`<svg viewBox="0 0 440 90" class="cdi"><line class="scale-line" x1="38" y1="45" x2="302" y2="45"/><circle class="dot" r="4.5" cx="110" cy="45"/><circle class="dot" r="4.5" cx="230" cy="45"/><line class="needle" y1="8" y2="82" x1=${x} x2=${x}/><g class="aircraft"><line x1="148" y1="45" x2="192" y2="45"/><line x1="170" y1="33" x2="170" y2="57"/></g>${glideslope != null ? html`<polygon class="glideslope-marker" points="0,-9 13,0 0,9 -13,0" transform="translate(390 ${45 + Math.max(-1, Math.min(1, glideslope)) * 32})"/>` : ''}</svg>`;
  }
}
