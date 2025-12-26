// HACK: This is a workaround for a bug in typescript-eslint where it doesn't
// correctly resolve the vitest types from tsconfig.spec.json.
/// <reference types="vitest/globals" />

import { IRatingPipe } from './irating.pipe';

describe('IRating pipe tests', () => {
  const pipe = new IRatingPipe();

  it('formats values under 1000', () => {
    expect(pipe.transform(654)).toEqual('654');
  });

  it('formats values over 1000', () => {
    expect(pipe.transform(1448)).toEqual('1.4k');
    expect(pipe.transform(1452)).toEqual('1.5k');
  });
});
