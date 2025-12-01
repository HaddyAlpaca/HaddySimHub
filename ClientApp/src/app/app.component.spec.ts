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
    mockSignalRService.displayData.set({ type: DisplayType.None, data: undefined } as DisplayUpdate);

    expect(await harness.isTruckDisplayVisible()).toBe(false);
    expect(await harness.isRaceDisplayVisible()).toBe(false);
    expect(await harness.isRallyDisplayVisible()).toBe(false);
    expect(await harness.isConnectionStatusVisible()).toBe(true);
  });

  it('should show the truck display when truck data is available', async () => {
    const harness = await TestbedHarnessEnvironment.harnessForFixture(fixture, AppComponentHarness);
    mockSignalRService.displayData.set({ type: DisplayType.TruckDashboard, data: {} as TruckData } as DisplayUpdate);

    expect(await harness.isTruckDisplayVisible()).toBe(true);
    expect(await harness.isRaceDisplayVisible()).toBe(false);
    expect(await harness.isRallyDisplayVisible()).toBe(false);
    expect(await harness.isConnectionStatusVisible()).toBe(false);
  });

  it('should show the race display when race data is available', async () => {
    const harness = await TestbedHarnessEnvironment.harnessForFixture(fixture, AppComponentHarness);
    mockSignalRService.displayData.set({ type: DisplayType.RaceDashboard, data: {} as RaceData } as DisplayUpdate);

    expect(await harness.isTruckDisplayVisible()).toBe(false);
    expect(await harness.isRaceDisplayVisible()).toBe(true);
    expect(await harness.isRallyDisplayVisible()).toBe(false);
    expect(await harness.isConnectionStatusVisible()).toBe(false);
  });

  it('should show the rally display when rally data is available', async () => {
    const harness = await TestbedHarnessEnvironment.harnessForFixture(fixture, AppComponentHarness);
    mockSignalRService.displayData.set({ type: DisplayType.RallyDashboard, data: { rpmLights: [] } as unknown as RallyData } as DisplayUpdate);

    expect(await harness.isTruckDisplayVisible()).toBe(false);
    expect(await harness.isRaceDisplayVisible()).toBe(false);
    expect(await harness.isRallyDisplayVisible()).toBe(true);
    expect(await harness.isConnectionStatusVisible()).toBe(false);
  });
});
