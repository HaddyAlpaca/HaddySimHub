import { describe, expect, it } from 'vitest';
import './flight-display.element';
import { CourseDeviationSource, EngineType, NavToFrom, type FlightData } from './flight-data';
import type { FlightDisplayElement } from './flight-display.element';

const data = (overrides: Partial<FlightData> = {}): FlightData => ({
  indicatedAirspeed: 268, trueAirspeed: 318, groundSpeed: 312, machNumber: 0.62, stallWarning: false, overspeedWarning: false,
  pitchDegrees: 3, bankDegrees: 12, slipBall: 0.1, turnRate: 1.2, indicatedAltitude: 24000, altitudeAboveGround: 23860,
  verticalSpeed: 720, altimeterSettingHpa: 1013, onGround: false, headingMagnetic: 94, headingTrue: 96, groundTrack: 97,
  windDirection: 270, windSpeed: 34, autopilotMaster: true, headingHold: true, headingBug: 106, altitudeHold: true,
  altitudeTarget: 24000, speedHold: false, speedTarget: 270, verticalSpeedTarget: 0, navHold: true, approachHold: false,
  hasActiveFlightPlan: true, nextWaypointId: 'ARTIP', distanceToWaypointNm: 18.4, waypointEteSeconds: 240, destinationId: 'EHAM',
  distanceToDestinationNm: 87, destinationEteSeconds: 2040, destinationEtaUtcSeconds: 51120, crossTrackErrorNm: 0.12,
  deviationSource: CourseDeviationSource.Gps, deviationSourceId: 'ARTIP', lateralDeviation: 0.2, lateralFullScaleNm: 2,
  glideslopeDeviation: undefined, toFrom: NavToFrom.Off, engineCount: 2, engineType: EngineType.Jet, enginePrimaryPct: 78,
  fuelFlowPph: 640, oilTemperature: 92, oilPressure: 45, manifoldPressure: undefined, fuelQuantityLbs: 4320,
  fuelCapacityLbs: 6000, fuelEnduranceSeconds: 10800, flapsHandleIndex: 0, flapsHandlePositions: 5, gearPercentExtended: 0,
  gearHandleDown: false, spoilersPct: 0, spoilersArmed: false, parkingBrakeOn: false, elevatorTrimPct: 4,
  landingLightsOn: false, taxiLightsOn: false, strobeLightsOn: true, navLightsOn: true, beaconOn: true,
  aircraftTitle: 'Citation', simTimeUtcSeconds: 43200, ...overrides,
});

describe('haddy-flight-display', () => {
  it('renders flight, navigation and engine panels', async () => {
    const element = document.createElement('haddy-flight-display') as FlightDisplayElement;
    element.data = data();
    document.body.append(element);
    await element.updateComplete;
    expect(element.querySelector('.speed-panel')?.textContent).toContain('268');
    expect(element.querySelector('.nav-panel')?.textContent).toContain('ARTIP');
    expect(element.querySelector('.engine-panel')?.textContent).toContain('N1');
    expect(element.querySelector('.config-panel')?.textContent).toContain('Citation');
  });

  it('renders warnings and optional guidance state', async () => {
    const element = document.createElement('haddy-flight-display') as FlightDisplayElement;
    element.data = data({ stallWarning: true, glideslopeDeviation: 0.3, deviationSource: CourseDeviationSource.Localizer });
    document.body.append(element);
    await element.updateComplete;
    expect(element.querySelector('.speed-panel')?.classList.contains('warning')).toBe(true);
    expect(element.querySelector('.course-panel .needle')).toBeTruthy();
    expect(element.querySelector('.glideslope-marker')).toBeTruthy();
  });

  it('renders piston and no-engine configuration correctly', async () => {
    const element = document.createElement('haddy-flight-display') as FlightDisplayElement;
    element.data = data({ engineType: EngineType.Piston, enginePrimaryPct: 65, engineRpm: 2400 });
    document.body.append(element);
    await element.updateComplete;
    expect(element.querySelector('.engine-panel')?.textContent).toContain('% RPM');

    element.data = data({ engineType: EngineType.None, engineCount: 0 });
    await element.updateComplete;
    expect(element.querySelector('.bar-fill.engine')).toBeNull();
  });
});
