import { describe, expect, it } from 'vitest';
import './clock.element';
import type { ClockElement } from './clock.element';

describe('haddy-clock', () => {
  it('renders the current time', async () => {
    const element = document.createElement('haddy-clock') as ClockElement;
    document.body.append(element);
    await element.updateComplete;

    expect(element.shadowRoot?.querySelector('#currentTime')?.textContent).toMatch(/^\d{2}:\d{2}$/);
  });
});
