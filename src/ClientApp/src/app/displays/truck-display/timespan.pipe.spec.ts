import { TimespanPipe } from './timespan.pipe';

describe('TimespanPipe', () => {
  const pipe = new TimespanPipe();

  it('Passing one hour and 7 minutes returns 1:07', () => {
    expect(pipe.transform(67)).toBe('1h 7min');
  });

  it('Format 42 minutes', () => {
    expect(pipe.transform(42)).toBe('42min');
  });

  it('Format  25 hours and 14 minutes', () => {
    expect(pipe.transform((25 * 60) + 14)).toBe('25h 14min');
  });

  it('Format a negative value of 1 hour and 15 minutes', () => {
    expect(pipe.transform(-75)).toBe('-1h 15min');
  });

  it('Format a negative value of 1 day 2 hours and 15 minutes', () => {
    expect(pipe.transform(-((26 * 60) + 15))).toBe('-26h 15min');
  });
});
