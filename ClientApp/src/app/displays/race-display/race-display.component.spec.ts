// HACK: This is a workaround for a bug in typescript-eslint where it doesn't
// correctly resolve the vitest types from tsconfig.spec.json.
/// <reference types="vitest/globals" />

import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TestbedHarnessEnvironment } from '@angular/cdk/testing/testbed';
import { provideZonelessChangeDetection } from '@angular/core';
import { RaceData } from './race-data';
import { provideCharts, withDefaultRegisterables } from 'ng2-charts';
import { MockSignalRService } from 'src/testing/mock-signalr.service';
import { DisplayType, SignalRService } from 'src/app/signalr.service';
import { RaceDisplayComponentHarness } from './race-display.component.harness';
import { RaceDisplayComponent } from './race-display.component';

describe('Race display component tests', () => {
  let fixture: ComponentFixture<RaceDisplayComponent>;
  let harness: RaceDisplayComponentHarness;
  let mockSignalRService: MockSignalRService;

  beforeEach(async () => {
    mockSignalRService = new MockSignalRService();

    await TestBed.configureTestingModule({
      providers: [
        provideZonelessChangeDetection(),
        provideCharts(withDefaultRegisterables()),
        { provide: SignalRService, useValue: mockSignalRService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(RaceDisplayComponent);
    harness = await TestbedHarnessEnvironment.harnessForFixture(fixture, RaceDisplayComponentHarness);
  });

  describe('Laps remaining tests', () => {
    it('When session is a timed session, display only laps completed', async () => {
      patchData({
        isTimedSession: true,
        currentLap: 2,
      });

      expect(await harness.getElementText('#laps')).toEqual('2');
    });

    it('When session is not a timed session, display laps completed and total laps', async () => {
      patchData({
        isTimeSession: false,
        currentLap: 2,
        totalLaps: 10,
      });

      expect(await harness.getElementText('#laps')).toEqual('2/10');
    });
  });

  it('Gear is displayed', async () => {
    patchData({ gear: 4 });

    const speedoHarness = await harness.getSpeedoHarness();
    expect(await speedoHarness.getGear()).toEqual('4');
  });

  it('Speed is displayed', async () => {
    patchData({ speed: 271 });

    const speedoHarness = await harness.getSpeedoHarness();
    expect(await speedoHarness.getSpeed()).toEqual('271');
  });

  it('RPM is displayed', async () => {
    patchData({ rpm: 8256 });

    const speedoHarness = await harness.getSpeedoHarness();
    expect(await speedoHarness.getRpm()).toEqual('8256');
  });

  describe('Brake bias tests', () => {
    it('brake bias is displayed', async () => {
      patchData({ brakeBias: 56.2 });

      expect(await harness.getElementText('#brakeBias')).toEqual('56.2');
    });

    it('brake bias without decimal places is displayed with one decimal place', async () => {
      patchData({ brakeBias: 56 });

      expect(await harness.getElementText('#brakeBias')).toEqual('56.0');
    });

    it('brake bias with 2 decimal places is displayed with one decimal place', async () => {
      patchData({ brakeBias: 56.28 });

      expect(await harness.getElementText('#brakeBias')).toEqual('56.3');
    });
  });

  it('Air temp is displayed', async () => {
    patchData({ airTemp: 25.3 });

    expect(await harness.getElementText('#air-temp')).toEqual('25.3 °C');
  });

  it('Track temp are displayed', async () => {
    patchData({ trackTemp: 32 });

    expect(await harness.getElementText('#track-temp')).toEqual('32.0 °C');
  });

  describe('Last laptime tests', () => {
    it('empty laptime', async () => {
      patchData({ lastLapTime: 0 });

      expect(await harness.getElementText('#lastLapTime')).toEqual('--:--.---');

    });

    it('valid laptime', async () => {
      patchData({ lastLapTime: 94.421 });

      expect(await harness.getElementText('#lastLapTime')).toEqual('01:34.421');

    });
  });

  describe('Best laptime tests', () => {
    it('empty laptime', async () => {
      patchData({ bestLapTime: 0 });

      expect(await harness.getElementText('#bestLapTime')).toEqual('--:--.---');
    });

    it('valid laptime', async () => {
      patchData({ bestLapTime: 92.876 });

      expect(await harness.getElementText('#bestLapTime')).toEqual('01:32.876');
    });
  });

  it('Position is displayed', async () => {
    patchData({ position: 3 });

    expect(await harness.getElementText('#position')).toEqual('3');
  });

  it('Fuel remaining is displayed', async () => {
    patchData({ fuelRemaining: 14.2 });

    expect(await harness.getElementText('#fuelRemaining')).toEqual('14.2 L');
  });

  describe('Fuel estimated laps warning', () => {
    it('Shows red when laps remaining is larger than estimated laps', async () => {
      // totalLaps - currentLap = 5, fuelEstLaps = 3 -> remaining laps (5) > est (3)
      patchData({ currentLap: 2, totalLaps: 7, fuelEstLaps: 3 });

      expect(await harness.elementHasClass('#fuelEstLaps', 'text-red')).toBe(true);
    });

    it('Does not show red when estimated laps covers remaining laps', async () => {
      // totalLaps - currentLap = 3, fuelEstLaps = 4 -> remaining laps (3) <= est (4)
      patchData({ currentLap: 2, totalLaps: 5, fuelEstLaps: 4 });

      expect(await harness.elementHasClass('#fuelEstLaps', 'text-red')).toBe(false);
    });
  });

  describe('Best lap delta time', () => {
    it('Delta time is displayed', async () => {
      patchData({ bestLapTimeDelta: 0.231 });

      expect(await harness.getElementText('#bestLapTimeDelta')).toEqual('+0.231');
    });

    it('Delta time is not green and not red when 0', async () => {
      patchData({ bestLapTimeDelta: 0 });

      expect(await harness.elementHasClass('#bestLapTimeDelta', 'text-green')).toBe(false);
      expect(await harness.elementHasClass('#bestLapTimeDelta', 'text-red')).toBe(false);

    });

    it('Delta time is red when > 0', async () => {
      patchData({ bestLapTimeDelta: 0.234 });

      expect(await harness.elementHasClass('#bestLapTimeDelta', 'text-red')).toBe(true);
    });

    it('Delta time is green when < 0', async () => {
      patchData({ bestLapTimeDelta: -0.234 });

      expect(await harness.elementHasClass('#bestLapTimeDelta', 'text-green')).toBe(true);
    });
  });

  describe('Pit limiter tests', () => {
    it('Pit limter is not shown when off', async () => {
      patchData({ pitLimiterOn: false });

      expect(await harness.hasElement('.pit-limiter')).toBe(false);
    });

    it('Pit limter is not shown when on', async () => {
      patchData({ pitLimiterOn: true });

      expect(await harness.hasElement('.pit-limiter')).toBe(true);
    });
  });

  describe('Incidents tests', () => {
    it('Without max incidents only incidents are shown', async () => {
      patchData({ incidents: 5, maxIncidents: 999 });

      expect(await harness.getElementText('#incidents')).toEqual('5');
    });

    it('Max incidents are shown when set', async () => {
      patchData({ incidents: 3, maxIncidents: 17 });

      expect(await harness.getElementText('#incidents')).toEqual('3/17');
    });
  });

  const patchData = (value: Record<string, unknown>): void => {
    mockSignalRService.displayData.set({
      type: DisplayType.RaceDashboard,
      data: value as unknown as RaceData,
    });
  };
});
