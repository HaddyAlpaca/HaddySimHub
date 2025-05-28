import { TestbedHarnessEnvironment } from '@angular/cdk/testing/testbed';
import { AppComponent } from './app.component';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AppComponentHarness } from './app.component.harness';
import { ConnectionInfo, ConnectionStatus, GameDataService } from './game-data.service';
import { provideZonelessChangeDetection, signal } from '@angular/core';
import { TruckData, RaceData, RallyData } from './displays';
import { provideCharts, withDefaultRegisterables } from 'ng2-charts';

class MockGameDataService {
  public truckData = signal<TruckData | null>(null);
  public raceData = signal<RaceData | null>(null);
  public rallyData = signal<RallyData | null>(null);
  public connectionStatus = signal<ConnectionInfo>({ status: ConnectionStatus.Disconnected });
  public page = signal(1);
}

describe('AppComponent tests', () => {
  let fixture: ComponentFixture<AppComponent>;
  let mockGameDataService: MockGameDataService;

  beforeEach(async () => {
    mockGameDataService = new MockGameDataService();

    await TestBed.configureTestingModule({
      providers: [
        provideCharts(withDefaultRegisterables()),
        provideZonelessChangeDetection(),
        { provide: GameDataService, useValue: mockGameDataService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(AppComponent);
  });

  it('should show the connection status when all data is null', async () => {
    const harness = await TestbedHarnessEnvironment.harnessForFixture(fixture, AppComponentHarness);

    expect(await harness.isTruckDisplayVisible()).toBeFalse();
    expect(await harness.isRaceDisplayVisible()).toBeFalse();
    expect(await harness.isRallyDisplayVisible()).toBeFalse();
    expect(await harness.isConnectionStatusVisible()).toBeTrue();
  });

  it('should show the truck display when truck data is available', async () => {
    mockGameDataService.truckData.set({} as TruckData);

    const harness = await TestbedHarnessEnvironment.harnessForFixture(fixture, AppComponentHarness);

    expect(await harness.isTruckDisplayVisible()).toBeTrue();
    expect(await harness.isRaceDisplayVisible()).toBeFalse();
    expect(await harness.isRallyDisplayVisible()).toBeFalse();
    expect(await harness.isConnectionStatusVisible()).toBeFalse();
  });

  it('should show the race display when race data is available', async () => {
    mockGameDataService.raceData.set({} as RaceData);

    const harness = await TestbedHarnessEnvironment.harnessForFixture(fixture, AppComponentHarness);

    expect(await harness.isTruckDisplayVisible()).toBeFalse();
    expect(await harness.isRaceDisplayVisible()).toBeTrue();
    expect(await harness.isRallyDisplayVisible()).toBeFalse();
    expect(await harness.isConnectionStatusVisible()).toBeFalse();
  });

  it('should show the rally display when rally data is available', async () => {
    mockGameDataService.rallyData.set({} as RallyData);

    const harness = await TestbedHarnessEnvironment.harnessForFixture(fixture, AppComponentHarness);

    expect(await harness.isTruckDisplayVisible()).toBeFalse();
    expect(await harness.isRaceDisplayVisible()).toBeFalse();
    expect(await harness.isRallyDisplayVisible()).toBeTrue();
    expect(await harness.isConnectionStatusVisible()).toBeFalse();
  });
});
