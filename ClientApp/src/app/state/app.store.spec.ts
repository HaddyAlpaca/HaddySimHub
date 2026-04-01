import { describe, expect, it } from 'vitest';
import { DisplayType } from '../signalr.service';
import { TruckData, RaceData, RallyData } from '../displays';
import { MockAppStore } from '../../testing/mock-app.store';

describe('MockAppStore', () => {
  const store = new MockAppStore();

  describe('initial state', () => {
    it('should have None as default display type', () => {
      expect(store.displayType()).toBe(DisplayType.None);
    });
  });

  describe('updateDisplay', () => {
    it('should update display with truck data', () => {
      const truckData: TruckData = {
        sourceCity: 'Berlin',
        sourceCompany: 'Test Company',
        destinationCity: 'Munich',
        destinationCompany: 'Dest Company',
        distanceRemaining: 500,
        timeRemaining: 36000,
        timeRemainingIrl: 1800,
        restTimeRemaining: 1800,
        restTimeRemainingIrl: 900,
        truckName: 'Volvo FH',
        jobTimeRemaining: 36000,
        jobTimeRemainingIrl: 1800,
        jobIncome: 1500,
        jobCargoName: 'Electronics',
        jobCargoMass: 20000,
        jobCargoDamage: 0,
        rpm: 1500,
        rpmMax: 2400,
        gear: 4,
        speed: 85,
        speedLimit: 90,
        parkingLightsOn: false,
        lowBeamOn: true,
        highBeamOn: false,
        blinkerLeftOn: false,
        blinkerRightOn: false,
        hazardLightsOn: false,
        parkingBrakeOn: false,
        wipersOn: false,
        differentialLock: false,
        cruiseControlOn: true,
        cruiseControlSpeed: 85,
        fuelWarningOn: false,
        fuelDistance: 350,
        fuelAmount: 45,
        adBlueWarningOn: false,
        adBlueAmount: 25,
        batteryVoltageWarningOn: false,
        batteryVoltage: 24.5,
        waterTempWarningOn: false,
        waterTemp: 85,
        oilPressureWarningOn: false,
        oilPressure: 45,
        oilTemp: 95,
        retarderStepCount: 4,
        retarderLevel: 0,
        throttle: 45,
        fuelAverageConsumption: 35,
        damageTruckEngine: 0,
        damageTruckTransmission: 0,
        damageTruckCabin: 0,
        damageTruckChassis: 0,
        damageTruckWheels: 0,
        numberOfTrailersAttached: 1,
        damageTrailerChassis: 0,
        damageTrailerBody: 0,
        damageTrailerWheels: 0,
        damageTrailerCargo: 0,
        damageTrailer: 0,
        gameTime: new Date(),
      };

      store.updateDisplay({ type: DisplayType.TruckDashboard, data: truckData });

      expect(store.displayType()).toBe(DisplayType.TruckDashboard);
      expect(store.truckData()).toEqual(truckData);
    });

    it('should update display with race data', () => {
      const raceData: RaceData = {
        sessionType: 'Race',
        strengthOfField: 2500,
        airTemp: 22,
        trackTemp: 28,
        currentLap: 5,
        totalLaps: 20,
        isLimitedTime: false,
        isLimitedSessionLaps: false,
        sessionTimeRemaining: 1800000,
        position: 3,
        incidents: 0,
        maxIncidents: 12,
        rpm: 8500,
        rpmMax: 9500,
        gear: '5',
        speed: 185,
        fuelLastLap: 3.2,
        fuelAvgLap: 3.1,
        fuelRemaining: 85,
        fuelEstLaps: 25,
        lastLapTime: 95000,
        lastLapTimeDelta: -250,
        bestLapTime: 94500,
        bestLapTimeDelta: 0,
        carNumber: '22',
        brakeBias: 53.5,
        pitLimiterOn: false,
        brakePct: 0,
        throttlePct: 50,
        steeringPct: 50,
      };

      store.updateDisplay({ type: DisplayType.RaceDashboard, data: raceData });

      expect(store.displayType()).toBe(DisplayType.RaceDashboard);
      expect(store.raceData()).toEqual(raceData);
    });

    it('should update display with rally data', () => {
      const rallyData: RallyData = {
        speed: 120,
        gear: '4',
        rpm: 4500,
        rpmMax: 6000,
        distanceTravelled: 5000,
        completedPct: 35,
        sector1Time: 45000,
        sector2Time: 48000,
        lapTime: 93000,
        position: 2,
      };

      store.updateDisplay({ type: DisplayType.RallyDashboard, data: rallyData });

      expect(store.displayType()).toBe(DisplayType.RallyDashboard);
      expect(store.rallyData()).toEqual(rallyData);
    });

    it('should set display type to None', () => {
      store.updateDisplay({ type: DisplayType.None, data: undefined });

      expect(store.displayType()).toBe(DisplayType.None);
    });
  });
});
