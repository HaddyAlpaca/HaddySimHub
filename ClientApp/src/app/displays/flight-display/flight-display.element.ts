import { LitElement, html, svg, type TemplateResult } from 'lit';
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
          <div class="panel heading-panel"><label>Heading</label>${this.heading(d.headingMagnetic ?? 0, d.headingBug, d.groundTrack)}<div class="secondary"><span class="strong">${number(d.headingMagnetic, 0)}&deg;</span><span><em>TRK</em> ${number(d.groundTrack)}&deg;</span><span><em>WIND</em> ${number(d.windDirection)}&deg; / ${number(d.windSpeed)} kts</span></div></div>
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
    const rungs = [-10, -5, 5, 10];
    const rollTicks = Array.from({ length: 13 }, (_, index) => {
      const angle = -60 + index * 10;
      const radians = angle * Math.PI / 180;
      const major = angle % 30 === 0;
      const point = (radius: number): { x: number; y: number } => ({
        x: +(100 + Math.sin(radians) * radius).toFixed(2),
        y: +(100 - Math.cos(radians) * radius).toFixed(2),
      });
      return { start: point(86), end: point(major ? 74 : 79) };
    });
    const pitchLadder = rungs.map(degrees => {
      const major = Math.abs(degrees) === 10;
      const halfWidth = major ? 34 : 18;
      const y = 100 - degrees * 4.2;
      const labelOffset = major ? 45 : 28;
      return {
        degrees: Math.abs(degrees),
        major,
        y,
        halfWidth,
        leftLabelX: 100 - labelOffset,
        rightLabelX: 100 + labelOffset,
      };
    });
    const slipPosition = 100 + Math.max(-1, Math.min(1, slip)) * 22;

    return html`
      <svg viewBox="0 0 200 200" class="attitude" role="img"
        aria-label="Pitch ${number(pitch, 1)} degrees, bank ${number(bank, 1)} degrees">
        <defs>
          <clipPath id="attitude-face"><circle cx="100" cy="100" r="87"></circle></clipPath>
        </defs>
        <circle cx="100" cy="100" r="98" class="bezel-background"></circle>
        <g class="attitude-window" clip-path="url(#attitude-face)">
          <g class="attitude-card"
          transform="rotate(${-bank} 100 100) translate(0 ${(pitch * 4.2).toFixed(1)})">
          <rect x="-200" y="-500" width="600" height="600" class="sky"></rect>
          <rect x="-200" y="100" width="600" height="600" class="ground"></rect>
          <path class="horizon-band"
            d="M0 100H32Q41 100 48 104Q53 107 60 107H140Q147 107 152 104Q159 100 168 100H200"></path>
          <path class="horizon-trim"
            d="M0 100H32Q41 100 48 104Q53 107 60 107H140Q147 107 152 104Q159 100 168 100H200"></path>
          ${pitchLadder.map(rung => svg`
            <line class="rung ${rung.major ? 'major' : ''}"
              x1=${100 - rung.halfWidth} y1=${rung.y}
              x2=${100 + rung.halfWidth} y2=${rung.y}></line>
            <text class="pitch-label" x=${rung.leftLabelX} y=${rung.y}>${rung.degrees}</text>
            <text class="pitch-label" x=${rung.rightLabelX} y=${rung.y}>${rung.degrees}</text>
          `)}
          </g>
        </g>
        <line class="flight-reference" x1="100" y1="18" x2="100" y2="182"></line>
        <g class="roll-scale">
          ${rollTicks.map(tick => svg`
            <line class="roll-tick" x1=${tick.start.x} y1=${tick.start.y}
              x2=${tick.end.x} y2=${tick.end.y}></line>
          `)}
        </g>
        <path class="roll-index" d="M100 9 89 22h22Z"></path>
        <circle cx="100" cy="100" r="88" class="bezel"></circle>
        <g class="aircraft">
          <circle class="aircraft-disc" cx="100" cy="100" r="9"></circle>
          <path class="aircraft-symbol"
            d="M100 90 103 98 119 100 119 103 103 102 103 106 109 110 109 112 100 109 91 112 91 110 97 106 97 102 81 103 81 100 97 98Z"></path>
        </g>
        <circle class="slip-ball" cx=${slipPosition} cy="180" r="5.5"></circle>
      </svg>
    `;
  }

  private heading(heading: number, bug: number | undefined, track: number): TemplateResult {
    const normalizedHeading = ((heading % 360) + 360) % 360;
    const polarPoint = (bearing: number, radius: number): { x: number; y: number } => {
      const radians = (bearing - 90) * Math.PI / 180;
      return {
        x: +(100 + Math.cos(radians) * radius).toFixed(2),
        y: +(100 + Math.sin(radians) * radius).toFixed(2),
      };
    };
    const ticks = Array.from({ length: 72 }, (_, index) => {
      const bearing = index * 5;
      const major = bearing % 10 === 0;
      const start = polarPoint(bearing, 84);
      const end = polarPoint(bearing, major ? 72 : 79);
      const label = polarPoint(bearing, 60);
      const text = bearing % 90 === 0
        ? ['N', 'E', 'S', 'W'][bearing / 90]
        : bearing % 30 === 0 ? `${bearing / 10}`.padStart(2, '0') : undefined;
      return { bearing, start, end, label, text };
    });
    const marker = (value: number): { x: number; y: number; rotation: number } => {
      const relativeBearing = ((value - normalizedHeading + 540) % 360) - 180;
      const point = polarPoint(relativeBearing, 84);
      return { ...point, rotation: relativeBearing };
    };
    const trackMarker = marker(track);
    const headingBug = bug === undefined ? undefined : marker(bug);

    return html`
      <svg viewBox="0 0 200 200" class="heading-compass"
        role="img" aria-label="Heading ${Math.round(normalizedHeading).toString().padStart(3, '0')} degrees">
        <circle class="compass-bezel" cx="100" cy="100" r="98"></circle>
        <circle class="compass-face" cx="100" cy="100" r="86"></circle>
        <g class="compass-card" transform="rotate(${-normalizedHeading} 100 100)">
          ${ticks.map(tick => svg`
            <line class="compass-tick ${tick.bearing % 10 === 0 ? 'major' : ''}"
              x1=${tick.start.x} y1=${tick.start.y} x2=${tick.end.x} y2=${tick.end.y}></line>
            ${tick.text ? svg`
              <text class="compass-label ${tick.text.length === 1 ? 'cardinal' : ''}"
                x=${tick.label.x} y=${tick.label.y}
                transform="rotate(${normalizedHeading} ${tick.label.x} ${tick.label.y})"
                text-anchor="middle" dominant-baseline="central">${tick.text}</text>
            ` : ''}
          `)}
        </g>
        ${headingBug ? svg`
          <polygon class="heading-bug-marker" points="0,-8 6,6 -6,6"
            transform="translate(${headingBug.x} ${headingBug.y}) rotate(${headingBug.rotation})"></polygon>
        ` : ''}
        <polygon class="ground-track-marker" points="0,-7 5,5 -5,5"
          transform="translate(${trackMarker.x} ${trackMarker.y}) rotate(${trackMarker.rotation})"></polygon>
        <polygon class="compass-pointer" points="100,16 94,32 106,32"></polygon>
        <path class="compass-aircraft"
          d="M100 66 106 94 135 97 135 103 106 101 104 117 116 122 116 126 100 123 84 126 84 122 96 117 94 101 65 103 65 97 94 94Z"></path>
      </svg>
    `;
  }

  private course(lateral: number | undefined, glideslope: number | undefined): TemplateResult {
    const x = 170 - Math.max(-1, Math.min(1, lateral ?? 0)) * 120;
    return html`<svg viewBox="0 0 440 90" class="cdi"><line class="scale-line" x1="38" y1="45" x2="302" y2="45"/><circle class="dot" r="4.5" cx="110" cy="45"/><circle class="dot" r="4.5" cx="230" cy="45"/><line class="needle" y1="8" y2="82" x1=${x} x2=${x}/><g class="aircraft"><line x1="148" y1="45" x2="192" y2="45"/><line x1="170" y1="33" x2="170" y2="57"/></g>${glideslope != null ? svg`<polygon class="glideslope-marker" points="0,-9 13,0 0,9 -13,0" transform="translate(390 ${45 + Math.max(-1, Math.min(1, glideslope)) * 32})"/>` : ''}</svg>`;
  }
}
