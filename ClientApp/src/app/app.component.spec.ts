import { TestbedHarnessEnvironment } from '@angular/cdk/testing/testbed';
import { AppComponent } from './app.component';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AppComponentHarness } from './app.component.harness';
import { ConnectionInfo, ConnectionStatus, GameDataService } from './game-data.service';
import { TruckData } from '@displays/truck-display/truck-data';
import { RaceData } from '@displays/race-display/race-data';
import { RallyData } from '@displays/rally-display/rally-data';
import { provideExperimentalZonelessChangeDetection, signal } from '@angular/core';

class MockGameDataService {
  public truckData = signal<TruckData | null>(null);
  public raceData = signal<RaceData | null>(null);
  public rallyData = signal<RallyData | null>(null);
  public connectionStatus = signal<ConnectionInfo>({ status: ConnectionStatus.Disconnected });
}

describe('AppComponent tests', () => {
  let fixture: ComponentFixture<AppComponent>;
  let mockGameDataService: MockGameDataService;

  beforeEach(async () => {
    mockGameDataService = new MockGameDataService();

    await TestBed.configureTestingModule({
      providers: [
        provideExperimentalZonelessChangeDetection(),
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
    mockGameDataService.truckData.set(new TruckData());

    const harness = await TestbedHarnessEnvironment.harnessForFixture(fixture, AppComponentHarness);

    expect(await harness.isTruckDisplayVisible()).toBeTrue();
    expect(await harness.isRaceDisplayVisible()).toBeFalse();
    expect(await harness.isRallyDisplayVisible()).toBeFalse();
    expect(await harness.isConnectionStatusVisible()).toBeFalse();
  });

  it('should show the race display when race data is available', async () => {
    mockGameDataService.raceData.set(new RaceData());

    const harness = await TestbedHarnessEnvironment.harnessForFixture(fixture, AppComponentHarness);

    expect(await harness.isTruckDisplayVisible()).toBeFalse();
    expect(await harness.isRaceDisplayVisible()).toBeTrue();
    expect(await harness.isRallyDisplayVisible()).toBeFalse();
    expect(await harness.isConnectionStatusVisible()).toBeFalse();
  });

  it('should show the rally display when rally data is available', async () => {
    mockGameDataService.rallyData.set(new RallyData());

    const harness = await TestbedHarnessEnvironment.harnessForFixture(fixture, AppComponentHarness);

    expect(await harness.isTruckDisplayVisible()).toBeFalse();
    expect(await harness.isRaceDisplayVisible()).toBeFalse();
    expect(await harness.isRallyDisplayVisible()).toBeTrue();
    expect(await harness.isConnectionStatusVisible()).toBeFalse();
  });
});
