import { TestbedHarnessEnvironment } from '@angular/cdk/testing/testbed';
import { AppComponent } from './app.component';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AppComponentHarness } from './app.component.harness';
import { provideZonelessChangeDetection } from '@angular/core';
import { provideCharts, withDefaultRegisterables } from 'ng2-charts';
import { DisplayType, SignalRService } from './signalr.service';
import { APP_STORE } from './state/app.store';
import { MockAppStore } from 'src/testing/mock-app.store';
import { MockedObject } from 'vitest';
import { mock } from 'vitest-mock-extended';

describe('AppComponent tests', () => {
  let fixture: ComponentFixture<AppComponent>;
  let mockStore: MockAppStore;
  let mockSignalRService: MockedObject<SignalRService>;

  beforeEach(async () => {
    mockStore = new MockAppStore();
    mockSignalRService = mock<SignalRService>();
    mockSignalRService.connectionStatus.mockReturnValue({ status: 0 });

    await TestBed.configureTestingModule({
      providers: [
        provideCharts(withDefaultRegisterables()),
        provideZonelessChangeDetection(),
        { provide: APP_STORE, useValue: mockStore },
        { provide: SignalRService, useValue: mockSignalRService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(AppComponent);
  });

  it('should show the connection status when display type is None', async () => {
    const harness = await TestbedHarnessEnvironment.harnessForFixture(fixture, AppComponentHarness);
    mockStore.displayType.set(DisplayType.None);
    fixture.detectChanges();

    expect(await harness.isTruckDisplayVisible()).toBe(false);
    expect(await harness.isRaceDisplayVisible()).toBe(false);
    expect(await harness.isRallyDisplayVisible()).toBe(false);
    expect(await harness.isConnectionStatusVisible()).toBe(true);
  });

  it('should show the truck display when truck data is available', async () => {
    const harness = await TestbedHarnessEnvironment.harnessForFixture(fixture, AppComponentHarness);
    mockStore.displayType.set(DisplayType.TruckDashboard);
    fixture.detectChanges();

    expect(await harness.isTruckDisplayVisible()).toBe(true);
    expect(await harness.isRaceDisplayVisible()).toBe(false);
    expect(await harness.isRallyDisplayVisible()).toBe(false);
    expect(await harness.isConnectionStatusVisible()).toBe(false);
  });

  it('should show the race display when race data is available', async () => {
    const harness = await TestbedHarnessEnvironment.harnessForFixture(fixture, AppComponentHarness);
    mockStore.displayType.set(DisplayType.RaceDashboard);
    fixture.detectChanges();

    expect(await harness.isTruckDisplayVisible()).toBe(false);
    expect(await harness.isRaceDisplayVisible()).toBe(true);
    expect(await harness.isRallyDisplayVisible()).toBe(false);
    expect(await harness.isConnectionStatusVisible()).toBe(false);
  });

  it('should show the rally display when rally data is available', async () => {
    const harness = await TestbedHarnessEnvironment.harnessForFixture(fixture, AppComponentHarness);
    mockStore.rallyData.set({
      speed: 0,
      gear: 'N',
      rpm: 0,
      rpmLights: [],
      rpmMax: 0,
      distanceTravelled: 0,
      completedPct: 0,
      timeElapsed: 0,
      sector1Time: 0,
      sector2Time: 0,
      lapTime: 0,
      sector: 0,
      position: 0,
      brakeTempFl: 0,
      brakeTempFr: 0,
      brakeTempRl: 0,
      brakeTempRr: 0,
      tyrePressFl: 0,
      tyrePressFr: 0,
      tyrePressRl: 0,
      tyrePressRr: 0,
    });
    mockStore.displayType.set(DisplayType.RallyDashboard);
    fixture.detectChanges();

    expect(await harness.isTruckDisplayVisible()).toBe(false);
    expect(await harness.isRaceDisplayVisible()).toBe(false);
    expect(await harness.isRallyDisplayVisible()).toBe(true);
    expect(await harness.isConnectionStatusVisible()).toBe(false);
  });
});
