# Pipe Test Pattern

Simple pipes without dependencies can be tested directly without TestBed.

```typescript
import { describe, it, expect } from 'vitest';
import { PipeNamePipe } from './pipe-name.pipe';

describe('PipeNamePipe', () => {
  const pipe = new PipeNamePipe();

  it('should transform value', () => {
    expect(pipe.transform('input')).toBe('expected');
  });

  it('should handle edge cases', () => {
    expect(pipe.transform('')).toBe('-');
  });
});
```
