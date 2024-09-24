import { NumberNlPipe } from './number-nl.pipe';

describe('NumberNlPipe tests', () => {
  const pipe = new NumberNlPipe();

  it('Format: 1,12', () => {
    expect(pipe.transform(1.12)).toBe('1.12');
  });
});
