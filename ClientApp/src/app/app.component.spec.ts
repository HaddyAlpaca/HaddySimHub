import { TestbedHarnessEnvironment } from '@angular/cdk/testing/testbed';
import { AppComponent } from './app.component';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AppComponentHarness } from './app.component.harness';
import { provideZonelessChangeDetection } from '@angular/core';
import { provideCharts, withDefaultRegisterables } from 'ng2-charts';
import { MockSignalRService } from 'src/testing/mock-signalr.service';
import { DisplayType, DisplayUpdate, SignalRService } from './signalr.service';
import { RaceData, RallyData, TruckData } from './displays';

describe('AppComponent tests', () => {
  let fixture: ComponentFixture<AppComponent>;
  let mockSignalRService: MockSignalRService;

  beforeEach(async () => {
    mockSignalRService = new MockSignalRService();

    await TestBed.configureTestingModule({
      providers: [
        provideCharts(withDefaultRegisterables()),
        provideZonelessChangeDetection(),
        { provide: SignalRService, useValue: mockSignalRService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(AppComponent);
  });

  it('should show the connection status when display type is None', async () => {
    const harness = await TestbedHarnessEnvironment.harnessForFixture(fixture, AppComponentHarness);
    mockSignalRService.displayData.set({ type: DisplayType.None, data: undefined, page: 1 } as DisplayUpdate);

    expect(await harness.isTruckDisplayVisible()).toBeFalse();
    expect(await harness.isRaceDisplayVisible()).toBeFalse();
    expect(await harness.isRallyDisplayVisible()).toBeFalse();
    expect(await harness.isConnectionStatusVisible()).toBeTrue();
  });

  it('should show the truck display when truck data is available', async () => {
    const harness = await TestbedHarnessEnvironment.harnessForFixture(fixture, AppComponentHarness);
    mockSignalRService.displayData.set({ type: DisplayType.TruckDashboard, data: {} as TruckData, page: 1 } as DisplayUpdate);

    expect(await harness.isTruckDisplayVisible()).toBeTrue();
    expect(await harness.isRaceDisplayVisible()).toBeFalse();
    expect(await harness.isRallyDisplayVisible()).toBeFalse();
    expect(await harness.isConnectionStatusVisible()).toBeFalse();
  });

  it('should show the race display when race data is available', async () => {
    const harness = await TestbedHarnessEnvironment.harnessForFixture(fixture, AppComponentHarness);
    mockSignalRService.displayData.set({ type: DisplayType.RaceDashboard, data: {} as RaceData, page: 1 } as DisplayUpdate);

    expect(await harness.isTruckDisplayVisible()).toBeFalse();
    expect(await harness.isRaceDisplayVisible()).toBeTrue();
    expect(await harness.isRallyDisplayVisible()).toBeFalse();
    expect(await harness.isConnectionStatusVisible()).toBeFalse();
  });

  it('should show the rally display when rally data is available', async () => {
    const harness = await TestbedHarnessEnvironment.harnessForFixture(fixture, AppComponentHarness);
    mockSignalRService.displayData.set({ type: DisplayType.RallyDashboard, data: {} as RallyData, page: 1 } as DisplayUpdate);

    expect(await harness.isTruckDisplayVisible()).toBeFalse();
    expect(await harness.isRaceDisplayVisible()).toBeFalse();
    expect(await harness.isRallyDisplayVisible()).toBeTrue();
    expect(await harness.isConnectionStatusVisible()).toBeFalse();
  });
});
