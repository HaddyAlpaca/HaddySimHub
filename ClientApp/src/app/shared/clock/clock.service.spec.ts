import { describe, expect, it } from 'vitest';
import { ClockService } from './clock.service';

describe('ClockService', () => {
  it('should return a Date from currentTime', () => {
    const service = new ClockService();
    const time = service.currentTime();
    expect(time).toBeInstanceOf(Date);
  });
});
