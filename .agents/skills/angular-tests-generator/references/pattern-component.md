# Component Test Pattern

## Basic Component Test

```typescript
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TestbedHarnessEnvironment } from '@angular/cdk/testing/testbed';
import { provideZonelessChangeDetection } from '@angular/core';
import { describe, it, expect, beforeEach } from 'vitest';
import { ComponentNameComponent } from './component-name.component';
import { ComponentNameHarness } from './component-name.component.harness';

describe('ComponentNameComponent tests', () => {
  let fixture: ComponentFixture<ComponentNameComponent>;
  let harness: ComponentNameHarness;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      providers: [
        provideZonelessChangeDetection(),
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ComponentNameComponent);
    harness = await TestbedHarnessEnvironment.harnessForFixture(
      fixture,
      ComponentNameHarness
    );
  });

  it('should create', () => {
    expect(fixture.componentInstance).toBeTruthy();
  });
});
```

## Complete Component Test Example

```typescript
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TestbedHarnessEnvironment } from '@angular/cdk/testing/testbed';
import { provideZonelessChangeDetection } from '@angular/core';
import { describe, it, expect, beforeEach } from 'vitest';
import { RaceDisplayComponent } from './race-display.component';
import { RaceDisplayHarness } from './race-display.component.harness';
import { MockAppStore } from 'src/testing/mock-app.store';
import { DisplayType } from 'src/app/signalr.service';

describe('RaceDisplayComponent', () => {
  let fixture: ComponentFixture<RaceDisplayComponent>;
  let harness: RaceDisplayHarness;
  let mockStore: MockAppStore;

  beforeEach(async () => {
    mockStore = new MockAppStore();
    mockStore.displayType.set(DisplayType.Race);

    await TestBed.configureTestingModule({
      providers: [
        provideZonelessChangeDetection(),
        { provide: AppStore, useValue: mockStore },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(RaceDisplayComponent);
    harness = await TestbedHarnessEnvironment.harnessForFixture(
      fixture,
      RaceDisplayHarness
    );
  });

  it('should create', () => {
    expect(fixture.componentInstance).toBeTruthy();
  });
});
```
