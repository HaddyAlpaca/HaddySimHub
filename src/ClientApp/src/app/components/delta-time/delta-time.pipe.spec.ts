import { DeltaTimePipe } from './delta-time.pipe';

describe('DeltaTimePipe', () => {
  const pipe = new DeltaTimePipe();

  it('Format delta: 0.593', () => {
    expect(pipe.transform(.593)).toBe('+0.593');
  });

  it('Format delta: -0.593', () => {
    expect(pipe.transform(-.593)).toBe('-0.593');
  });

  it('Format delta: 0.5931', () => {
    expect(pipe.transform(.5931)).toBe('+0.593');
  });

  it('Format delta: 0.5937', () => {
    expect(pipe.transform(.5937)).toBe('+0.594');
  });

  it('Format delta: -0.5931', () => {
    expect(pipe.transform(-.5931)).toBe('-0.593');
  });

  it('Format delta: -0.5937', () => {
    expect(pipe.transform(-.5937)).toBe('-0.594');
  });

  it('Format delta: 5.492', () => {
    expect(pipe.transform(5.492)).toBe('+5.492');
  });

  it('Format delta: -5.492', () => {
    expect(pipe.transform(-5.492)).toBe('-5.492');
  });

  it('Sign can be supressed', () => {
    expect(pipe.transform(-5.492, false)).toBe('5.492');
    expect(pipe.transform(5.492, false)).toBe('5.492');
  })
});
