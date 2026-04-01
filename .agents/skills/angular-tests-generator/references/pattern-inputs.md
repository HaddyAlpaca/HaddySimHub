# Input Bindings Pattern

Use `inputBinding` to provide input values to components.

```typescript
import { inputBinding } from '@angular/core';

let inputValue: string;

fixture = TestBed.createComponent(ComponentComponent, {
  bindings: [
    inputBinding('inputName', () => inputValue),
  ],
});

inputValue = 'new value';
fixture.detectChanges();

// Read the updated value
expect(await harness.getValue()).toBe('new value');
```

## Multiple Input Bindings

```typescript
let speed: number;
let rpm: number;
let gear: string;

fixture = TestBed.createComponent(SpeedometerComponent, {
  bindings: [
    inputBinding('speed', () => speed),
    inputBinding('rpm', () => rpm),
    inputBinding('gear', () => gear),
  ],
});

// Update inputs
speed = 120;
rpm = 3500;
gear = '4';
fixture.detectChanges();
```
