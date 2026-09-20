import { describe, expect, it } from 'vitest';
import './truck-display.element';
import type { TruckData } from './truck-data';
import type { TruckDisplayElement } from './truck-display.element';

const data = (overrides: Partial<TruckData> = {}): TruckData => ({
  sourceCity: 'Rotterdam', sourceCompany: 'Cargo', destinationCity: 'Utrecht', destinationCompany: 'Depot',
  timeRemaining: 90, timeRemainingIrl: 0, distanceRemaining: 120, restTimeRemaining: 60, restTimeRemainingIrl: 0,
  fuelDistance: 800, fuelAmount: 500, fuelCapacity: 1_000, adBlueAmount: 100, adBlueCapacity: 200, adBlueWarningOn: false,
  jobTimeRemaining: 600, jobTimeRemainingIrl: 0, jobIncome: 32_145, jobCargoName: 'Helicopter', jobCargoMass: 2_500, jobCargoDamage: 30,
  damageTruckCabin: 0, damageTruckWheels: 0, damageTruckTransmission: 30, damageTruckEngine: 10, damageTruckChassis: 0,
  numberOfTrailersAttached: 1, damageTrailerChassis: 0, damageTrailerCargo: 60, damageTrailerBody: 0, damageTrailerWheels: 0, damageTrailer: 0,
  speed: 80, speedLimit: 90, rpm: 1_500, rpmMax: 2_500, cruiseControlOn: false, cruiseControlSpeed: 0, gear: '7', recommendedGear: '9',
  parkingLightsOn: true, lowBeamOn: false, highBeamOn: false, parkingBrakeOn: false, batteryVoltageWarningOn: false, batteryVoltage: 24,
  truckName: 'Volvo', hazardLightsOn: false, fuelWarningOn: false, blinkerLeftOn: false, blinkerRightOn: false, gameTime: 780,
  wipersOn: false, fuelAverageConsumption: 30, throttle: 20, differentialLock: false, oilPressure: 30, oilPressureWarningOn: false,
  oilTemp: 80, waterTemp: 90, waterTempWarningOn: false, brakeTemp: 50, brakeAirPressure: 8, retarderLevel: 0, retarderStepCount: 5,
  airPressureWarningOn: false, airPressureEmergencyOn: false, engineOn: true, motorBrakeOn: false, beaconOn: false, liftAxleIndicatorOn: false,
  odometer: 100, dashboardBacklight: 1, ...overrides,
});

describe('haddy-truck-display', () => {
  it('renders route, job, damage and gauge data', async () => {
    const element = document.createElement('haddy-truck-display') as TruckDisplayElement;
    element.data = data();
    document.body.append(element);
    await element.updateComplete;
    expect(element.querySelector('#gameTime')?.textContent).toBe('13:00');
    expect(element.querySelector('#jobIncome')?.textContent).toContain('32.145');
    expect(element.querySelector('#damageTrailerCargo')?.classList.contains('damage-critical')).toBe(true);
    expect(element.querySelector('#gear-advice')?.textContent).toContain('9');
  });

  it('marks unreachable fuel range and damage warning', async () => {
    const element = document.createElement('haddy-truck-display') as TruckDisplayElement;
    element.data = data({ fuelDistance: 50, distanceRemaining: 100, damageTruckEngine: 30 });
    document.body.append(element);
    await element.updateComplete;
    expect(element.querySelector('.fuel-range-too-short')).toBeTruthy();
    expect(element.querySelector('#damageTruckEngine')?.classList.contains('damage-warning')).toBe(true);
  });

  it('shows the no-trailer state', async () => {
    const element = document.createElement('haddy-truck-display') as TruckDisplayElement;
    element.data = data({ numberOfTrailersAttached: 0 });
    document.body.append(element);
    await element.updateComplete;
    expect(element.textContent).toContain('Geen trailer gekoppeld');
  });
});
