import { describe, expect, it } from 'vitest';
import { AppStore } from './app.store';
import { DisplayType } from '../sse.service';

describe('AppStore', () => {
  it('publishes display updates to subscribers', () => {
    const store = new AppStore();
    let updates = 0;
    const unsubscribe = store.subscribe(() => updates++);

    store.updateDisplay({ type: DisplayType.RaceDashboard, data: undefined });

    expect(store.displayType).toBe(DisplayType.RaceDashboard);
    expect(updates).toBe(1);
    unsubscribe();
  });
});
