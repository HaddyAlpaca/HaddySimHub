import { describe, expect, it } from 'vitest';
import './race-display.element';
import type { RaceData } from './race-data';
import type { RaceDisplayElement } from './race-display.element';

const data = (overrides: Partial<RaceData> = {}): RaceData => ({
  sessionType: 'Race',
  isLimitedTime: false,
  isLimitedSessionLaps: false,
  currentLap: 2,
  totalLaps: 10,
  sessionTimeRemaining: 120,
  speed: 100,
  gear: '4',
  rpm: 5_000,
  rpmMax: 8_000,
  trackTemp: 32,
  airTemp: 25.3,
  fuelEstLaps: 5,
  currentLapTime: 0,
  lastLapTime: 94.421,
  pitLimiterOn: false,
  brakePct: 10,
  throttlePct: 80,
  steeringPct: 50,
  ...overrides,
});

describe('haddy-race-display', () => {
  it('renders race status and formatted telemetry values', async () => {
    const element = document.createElement('haddy-race-display') as RaceDisplayElement;
    element.data = data({ position: 3, brakeBias: 56 });
    document.body.append(element);
    await element.updateComplete;

    expect(element.querySelector('#position')?.textContent).toBe('P3');
    expect(element.querySelector('#air-temp')?.textContent).toBe('25.3°');
    expect(element.querySelector('#brakeBias')?.textContent).toBe('56.0%');
    expect(element.querySelector('#lastLapTime')?.textContent).toBe('01:34.421');
  });

  it('renders the pit limiter and max RPM state', async () => {
    const element = document.createElement('haddy-race-display') as RaceDisplayElement;
    element.data = data({ rpm: 8_000, pitLimiterOn: true });
    document.body.append(element);
    await element.updateComplete;

    expect(element.querySelector('.speedometer-panel')?.classList.contains('max-rpm')).toBe(true);
    expect(element.querySelector('.pit-limiter')?.textContent).toBe('PIT LIMITER');
  });

  it('hides optional fields when the sim does not provide them', async () => {
    const element = document.createElement('haddy-race-display') as RaceDisplayElement;
    element.data = data();
    document.body.append(element);
    await element.updateComplete;

    expect(element.querySelector('#position')).toBeNull();
    expect(element.querySelector('#fuelRemaining')).toBeNull();
  });
});
