import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TestbedHarnessEnvironment } from '@angular/cdk/testing/testbed';
import { provideZonelessChangeDetection } from '@angular/core';
import { describe, beforeEach, it, expect } from 'vitest';
import { FlightDisplayComponent } from './flight-display.component';
import { FlightDisplayComponentHarness } from './flight-display.component.harness';
import { EngineType, FlightData } from './flight-data';
import { MockAppStore } from '../../../testing/mock-app.store';
import { APP_STORE } from '../../state/app.store';

/** A jet in the cruise with an active flight plan -- the baseline every test varies from. */
const createFlightData = (overrides: Partial<FlightData> = {}): FlightData => ({
  indicatedAirspeed: 268,
  trueAirspeed: 318,
  groundSpeed: 312,
  machNumber: 0.62,
  stallWarning: false,
  overspeedWarning: false,

  pitchDegrees: 3,
  bankDegrees: 12,
  slipBall: 0.1,
  turnRate: 1.2,

  indicatedAltitude: 24000,
  altitudeAboveGround: 23860,
  verticalSpeed: 720,
  altimeterSettingHpa: 1013,
  onGround: false,

  headingMagnetic: 94,
  headingTrue: 96,
  groundTrack: 97,
  windDirection: 270,
  windSpeed: 34,

  autopilotMaster: true,
  headingHold: true,
  headingBug: 106,
  altitudeHold: true,
  altitudeTarget: 24000,
  speedHold: false,
  speedTarget: 270,
  verticalSpeedTarget: 0,
  navHold: true,
  approachHold: false,

  hasActiveFlightPlan: true,
  nextWaypointId: 'ARTIP',
  distanceToWaypointNm: 18.4,
  waypointEteSeconds: 240,
  destinationId: 'EHAM',
  distanceToDestinationNm: 87,
  destinationEteSeconds: 2040,
  destinationEtaUtcSeconds: 14 * 3600 + 12 * 60,
  crossTrackErrorNm: 0.12,

  engineCount: 2,
  engineType: EngineType.Jet,
  enginePrimaryPct: 78,
  fuelFlowPph: 640,
  oilTemperature: 92,
  oilPressure: 45,

  fuelQuantityLbs: 4320,
  fuelCapacityLbs: 6000,
  fuelEnduranceSeconds: 3 * 3600,

  flapsHandleIndex: 0,
  flapsHandlePositions: 5,
  gearPercentExtended: 0,
  gearHandleDown: false,
  spoilersPct: 0,
  spoilersArmed: false,
  parkingBrakeOn: false,
  elevatorTrimPct: 4,

  landingLightsOn: false,
  taxiLightsOn: false,
  strobeLightsOn: true,
  navLightsOn: true,
  beaconOn: true,

  aircraftTitle: 'Cessna Citation Longitude',
  simTimeUtcSeconds: 12 * 3600,

  ...overrides,
});

describe('FlightDisplayComponent', () => {
  let fixture: ComponentFixture<FlightDisplayComponent>;
  let mockStore: MockAppStore;
  let harness: FlightDisplayComponentHarness;

  beforeEach(async () => {
    mockStore = new MockAppStore();

    await TestBed.configureTestingModule({
      providers: [
        provideZonelessChangeDetection(),
        { provide: APP_STORE, useValue: mockStore },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(FlightDisplayComponent);
    harness = await TestbedHarnessEnvironment.harnessForFixture(fixture, FlightDisplayComponentHarness);
  });

  const render = (overrides: Partial<FlightData> = {}): void => {
    mockStore.flightData.set(createFlightData(overrides));
    fixture.detectChanges();
  };

  it('should create', () => {
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should show airspeed, ground speed and true airspeed', async () => {
    render();

    const text = await harness.getSpeedPanelText();
    expect(text).toContain('268');
    expect(text).toContain('312');
    expect(text).toContain('318');
  });

  it('should show altitude, vertical speed and altimeter setting', async () => {
    render();

    const text = await harness.getAltitudePanelText();
    expect(text).toContain('24.000');
    expect(text).toContain('720');
    expect(text).toContain('1013');
  });

  it('should flag a stall', async () => {
    render({ stallWarning: true });

    expect(await harness.hasStallWarning()).toBe(true);
    expect(await harness.getSpeedPanelText()).toContain('STALL');
  });

  it('should flag an overspeed', async () => {
    render({ overspeedWarning: true });

    expect(await harness.hasOverspeedWarning()).toBe(true);
    expect(await harness.getSpeedPanelText()).toContain('OVERSPEED');
  });

  it('should list only the engaged autopilot modes', async () => {
    render({ headingHold: true, navHold: true, altitudeHold: true, speedHold: false, approachHold: false });

    expect(await harness.getAutopilotModes()).toEqual(['HDG', 'NAV', 'ALT']);
  });

  it('should report the autopilot off regardless of the individual mode flags', async () => {
    render({ autopilotMaster: false, headingHold: true, altitudeHold: true });

    expect(await harness.getAutopilotModes()).toEqual(['AP OFF']);
  });

  it('should show both legs of the flight plan', async () => {
    render();

    const text = await harness.getNavPanelText();
    expect(text).toContain('ARTIP');
    expect(text).toContain('18.4');
    expect(text).toContain('EHAM');
    expect(text).toContain('87');
    expect(text).toContain('14:12');
  });

  it('should name the side of course the aircraft has drifted to', async () => {
    render({ crossTrackErrorNm: -0.35 });

    const text = await harness.getNavPanelText();
    expect(text).toContain('0.35 NM left of course');
  });

  it('should say so when there is no flight plan', async () => {
    render({ hasActiveFlightPlan: false, nextWaypointId: undefined, destinationId: undefined });

    expect(await harness.isFlightPlanShown()).toBe(false);
    expect(await harness.getNavPanelText()).toContain('No active flight plan');
  });

  it('should label the primary engine readout N1 for a turbine', async () => {
    render({ engineType: EngineType.Turboprop });

    expect(await harness.getEnginePanelText()).toContain('N1');
  });

  it('should label the primary engine readout as RPM for a piston and add its piston-only gauges', async () => {
    render({
      engineType: EngineType.Piston,
      engineCount: 1,
      enginePrimaryPct: 65,
      engineRpm: 2400,
      manifoldPressure: 24.5,
    });

    const text = await harness.getEnginePanelText();
    expect(text).toContain('% RPM');
    expect(text).toContain('24.5');
    expect(text).toContain('2.400');
  });

  it('should hide the engine readouts for an aircraft without an engine', async () => {
    render({ engineType: EngineType.None, engineCount: 0, enginePrimaryPct: undefined });

    expect(await harness.isEnginePanelShown()).toBe(false);
    expect(await harness.getEnginePanelText()).not.toContain('N1');
  });

  it('should warn below the 45 minute fuel reserve', async () => {
    render({ fuelEnduranceSeconds: 40 * 60 });

    expect(await harness.hasLowFuelWarning()).toBe(true);
  });

  it('should not warn above the fuel reserve', async () => {
    render({ fuelEnduranceSeconds: 50 * 60 });

    expect(await harness.hasLowFuelWarning()).toBe(false);
  });

  it('should show the gear as in transit while it travels', async () => {
    render({ gearPercentExtended: 45 });

    expect(await harness.getConfigPanelText()).toContain('IN TRANSIT');
  });

  it('should show the gear as down once fully extended', async () => {
    render({ gearPercentExtended: 100, gearHandleDown: true });

    expect(await harness.getConfigPanelText()).toContain('DOWN');
  });

  it('should show armed spoilers as armed until they deploy', async () => {
    render({ spoilersArmed: true, spoilersPct: 0 });
    expect(await harness.getConfigPanelText()).toContain('ARMED');

    render({ spoilersArmed: true, spoilersPct: 60 });
    expect(await harness.getConfigPanelText()).toContain('60%');
  });

  it('should light only the lights that are on', async () => {
    render({ landingLightsOn: true, taxiLightsOn: false, strobeLightsOn: true, navLightsOn: true, beaconOn: false });

    expect(await harness.getLitLights()).toEqual(['LAND', 'STROBE', 'NAV']);
  });
});
