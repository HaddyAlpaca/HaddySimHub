// HACK: This is a workaround for a bug in typescript-eslint where it doesn't
// correctly resolve the vitest types from tsconfig.spec.json.
/// <reference types="vitest/globals" />

import { NumberFlexDigitPipe } from './number-flex-digit.pipe';

describe('NumberFlexDigitPipe tests', () => {
  const pipe = new NumberFlexDigitPipe();

  it('Displays number correctly', () => {
    expect(pipe.transform(1.12)).toBe('1.12');
    expect(pipe.transform(10.12)).toBe('10');
  });
});
