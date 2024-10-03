import { LapTimePipe } from './laptime.pipe';

describe('LaptimePipe', () => {
  const pipe = new LapTimePipe();

  it('Zero time results in --:--.---', () => {
    const seconds = 0;
    expect(pipe.transform(seconds)).toBe('--:--.---');
  });

  it('Check formatting of laptime 01:34.421', () => {
    const seconds = 94.421;
    expect(pipe.transform(seconds)).toBe('01:34.421');
  });

  it('Check formatting of laptime 02:05.009', () => {
    const seconds = 125.009;
    expect(pipe.transform(seconds)).toBe('02:05.009');
  });

  it('Check formatting of laptime 61:15.123', () => {
    const seconds = 61 * 60 + 15.123;
    expect(pipe.transform(seconds)).toBe('61:15.123');
  });

});
