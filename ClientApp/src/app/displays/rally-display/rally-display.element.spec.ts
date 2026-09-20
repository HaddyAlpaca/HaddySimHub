import { describe, expect, it } from 'vitest';
import './rally-display.element';
import type { RallyDisplayElement } from './rally-display.element';

const data = {
  speed: 120,
  gear: '4',
  rpm: 4500,
  rpmMax: 6000,
  distanceTravelled: 5000,
  completedPct: 35,
  sector1Time: 45,
  sector2Time: 48,
  lapTime: 93,
  position: 2,
};

describe('haddy-rally-display', () => {
  it('renders progress, speedometer and sector data', async () => {
    const element = document.createElement('haddy-rally-display') as RallyDisplayElement;
    element.data = data;
    document.body.append(element);
    await element.updateComplete;

    expect(element.textContent).toContain('35%');
    expect(element.textContent).toContain('5000 m');
    expect(element.querySelector('.gear')?.textContent).toBe('4');
    expect(element.textContent).toContain('Sector 1');
    expect(element.textContent).toContain('00:45.000');
  });

  it('marks the speedometer when the engine reaches the limit', async () => {
    const element = document.createElement('haddy-rally-display') as RallyDisplayElement;
    element.data = { ...data, rpm: 6000 };
    document.body.append(element);
    await element.updateComplete;

    expect(element.querySelector('.speedometer-section')?.classList.contains('max-rpm')).toBe(true);
  });

  it('renders the empty lap placeholder', async () => {
    const element = document.createElement('haddy-rally-display') as RallyDisplayElement;
    element.data = { ...data, sector1Time: 0 };
    document.body.append(element);
    await element.updateComplete;

    expect(element.textContent).toContain('--:--.---');
  });
});
