import { describe, beforeEach, it, expect } from 'vitest';
import { TimePipe } from './time.pipe';

describe('TimePipe', () => {
  let pipe: TimePipe;

  beforeEach(() => {
    pipe = new TimePipe();
  });

  it('should create an instance', () => {
    expect(pipe).toBeTruthy();
  });

  it('should format seconds less than an hour', () => {
    expect(pipe.transform(45)).toBe('00:45');
    expect(pipe.transform(90)).toBe('01:30');
    expect(pipe.transform(600)).toBe('10:00');
  });

  it('should format seconds more than an hour', () => {
    expect(pipe.transform(3600)).toBe('01:00:00');
    expect(pipe.transform(3661)).toBe('01:01:01');
    expect(pipe.transform(7325)).toBe('02:02:05');
  });

  it('should handle negative values', () => {
    expect(pipe.transform(-45)).toBe('-00:45');
    expect(pipe.transform(-90)).toBe('-01:30');
    expect(pipe.transform(-3661)).toBe('-01:01:01');
  });

  it('should pad numbers with leading zeros', () => {
    expect(pipe.transform(5)).toBe('00:05');
    expect(pipe.transform(65)).toBe('01:05');
    expect(pipe.transform(3605)).toBe('01:00:05');
  });

  it('should handle zero', () => {
    expect(pipe.transform(0)).toBe('00:00');
  });
});
