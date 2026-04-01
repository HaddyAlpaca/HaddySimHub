import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TestbedHarnessEnvironment } from '@angular/cdk/testing/testbed';
import { RallyDisplayComponent } from './rally-display.component';
import { RallyDisplayComponentHarness } from './rally-display.component.harness';
import { provideZonelessChangeDetection } from '@angular/core';
import { provideCharts, withDefaultRegisterables } from 'ng2-charts';
import { describe, beforeEach, it, expect } from 'vitest';
import { MockAppStore } from 'src/testing/mock-app.store';
import { APP_STORE } from 'src/app/state/app.store';

describe('RallyDisplayComponent', () => {
  let fixture: ComponentFixture<RallyDisplayComponent>;
  let mockStore: MockAppStore;

  beforeEach(async () => {
    mockStore = new MockAppStore();

    await TestBed.configureTestingModule({
      providers: [
        provideCharts(withDefaultRegisterables()),
        provideZonelessChangeDetection(),
        { provide: APP_STORE, useValue: mockStore },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(RallyDisplayComponent);
  });

  it('should create', () => {
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('should display rally data', () => {
    mockStore.rallyData.set({
      speed: 120,
      gear: '4',
      rpm: 4500,
      rpmMax: 6000,
      distanceTravelled: 5000,
      completedPct: 35,
      sector1Time: 45000,
      sector2Time: 48000,
      lapTime: 93000,
      position: 2,
    });
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('35');
    expect(compiled.textContent).toContain('5000');
    expect(compiled.textContent).toContain('Sector 1');
    expect(compiled.textContent).toContain('Sector 2');
  });

  it('should apply max-rpm class when rpm >= rpmMax', async () => {
    const harness = await TestbedHarnessEnvironment.harnessForFixture(fixture, RallyDisplayComponentHarness);
    mockStore.rallyData.set({
      speed: 120,
      gear: '4',
      rpm: 6000,
      rpmMax: 6000,
      distanceTravelled: 5000,
      completedPct: 35,
      sector1Time: 45000,
      sector2Time: 48000,
      lapTime: 93000,
      position: 2,
    });
    fixture.detectChanges();

    const hasMaxRpmClass = await harness.hasMaxRpmClass();
    expect(hasMaxRpmClass).toBe(true);
  });

  it('should not apply max-rpm class when rpm < rpmMax', async () => {
    const harness = await TestbedHarnessEnvironment.harnessForFixture(fixture, RallyDisplayComponentHarness);
    mockStore.rallyData.set({
      speed: 100,
      gear: '4',
      rpm: 4000,
      rpmMax: 6000,
      distanceTravelled: 5000,
      completedPct: 35,
      sector1Time: 45000,
      sector2Time: 48000,
      lapTime: 93000,
      position: 2,
    });
    fixture.detectChanges();

    const hasMaxRpmClass = await harness.hasMaxRpmClass();
    expect(hasMaxRpmClass).toBe(false);
  });
});
