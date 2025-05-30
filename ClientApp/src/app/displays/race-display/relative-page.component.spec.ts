import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RelativePageComponent } from './relative-page.component';
import { TestbedHarnessEnvironment } from '@angular/cdk/testing/testbed';
import { RelativePageComponentHarness } from './relative-page.component.harness';
import { provideZonelessChangeDetection } from '@angular/core';
import { RaceData, TimingEntry } from './race-data';
import { MockSignalRService } from 'src/testing/mock-signalr.service';
import { DisplayType, DisplayUpdate, SignalRService } from 'src/app/signalr.service';

describe('RelativePageComponent tests', () => {
  let fixture: ComponentFixture<RelativePageComponent>;
  let mockSignalRService: MockSignalRService;

  beforeEach(async () => {
    mockSignalRService = new MockSignalRService();

    await TestBed.configureTestingModule({
      providers: [
        provideZonelessChangeDetection(),
        { provide: SignalRService, useValue: mockSignalRService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(RelativePageComponent);
  });

  it('Check order when player is in the middle', async () => {
    mockSignalRService.displayData.set({
      type: DisplayType.RaceDashboard,
      data : {
        timingEntries: [
          {
            position: 6,
            carNumber: 18,
            driverName: 'Lap behind',
            license: 'D',
            iRating: 500,
            timeToPlayer: 15,
            isLapBehind: true,
          } as TimingEntry,
          {
            position: 1,
            carNumber: 61,
            driverName: 'Lap ahead',
            license: 'A',
            iRating: 4500,
            timeToPlayer: 10,
            isLapAhead: true,
          } as TimingEntry,
          {
            position: 2,
            carNumber: 2,
            driverName: 'Same lap 1',
            license: 'B',
            iRating: 2000,
            timeToPlayer: 5,
          } as TimingEntry,
          {
            position: 3,
            carNumber: 3,
            driverName: 'Player',
            license: 'C',
            iRating: 3000,
            timeToPlayer: 0,
            isPlayer: true,
          } as TimingEntry,
          {
            position: 4,
            carNumber: 4,
            driverName: 'Same lap 2',
            license: 'D',
            iRating: 4000,
            timeToPlayer: -5,
          } as TimingEntry,
          {
            position: 5,
            carNumber: 5,
            driverName: 'Same lap 3',
            license: 'D',
            iRating: 5000,
            timeToPlayer: -10,
            isInPits: true,
          } as TimingEntry,
          {
            position: 0,
            carNumber: 0,
            driverName: 'Safety Car',
            license: '',
            iRating: 0,
            timeToPlayer: 20,
            isSafetyCar: true,
          } as TimingEntry,
        ],
      } as RaceData,
      page: 2,
    } as DisplayUpdate);

    const harness = await TestbedHarnessEnvironment.harnessForFixture(fixture, RelativePageComponentHarness);

    expect(await harness.getRows()).toEqual([
      {
        position: '',
        carNumber: '',
        driverName: 'Safety Car',
        license: '',
        iRating: '',
        timeToPlayer: '+20.0',
        isSafetyCar: true,
        isInPits: false,
        isPlayer: false,
        isLapBehind: false,
        isLapAhead: false,
      },
      {
        position: '6',
        carNumber: '#18',
        driverName: 'Lap behind',
        license: 'D',
        iRating: '500',
        timeToPlayer: '+15.0',
        isSafetyCar: false,
        isInPits: false,
        isPlayer: false,
        isLapBehind: true,
        isLapAhead: false,
      },
      {
        position: '1',
        carNumber: '#61',
        driverName: 'Lap ahead',
        license: 'A',
        iRating: '4500',
        timeToPlayer: '+10.0',
        isSafetyCar: false,
        isInPits: false,
        isPlayer: false,
        isLapBehind: false,
        isLapAhead: true,
      },
      {
        position: '2',
        carNumber: '#2',
        driverName: 'Same lap 1',
        license: 'B',
        iRating: '2000',
        timeToPlayer: '+5.0',
        isSafetyCar: false,
        isInPits: false,
        isPlayer: false,
        isLapBehind: false,
        isLapAhead: false,
      },
      {
        position: '3',
        carNumber: '#3',
        driverName: 'Player',
        license: 'C',
        iRating: '3000',
        timeToPlayer: '',
        isSafetyCar: false,
        isInPits: false,
        isPlayer: true,
        isLapBehind: false,
        isLapAhead: false,
      },
      {
        position: '4',
        carNumber: '#4',
        driverName: 'Same lap 2',
        license: 'D',
        iRating: '4000',
        timeToPlayer: '-5.0',
        isSafetyCar: false,
        isInPits: false,
        isPlayer: false,
        isLapBehind: false,
        isLapAhead: false,
      },
      {
        position: '5',
        carNumber: '#5',
        driverName: 'Same lap 3',
        license: 'D',
        iRating: '5000',
        timeToPlayer: '-10.0',
        isSafetyCar: false,
        isInPits: true,
        isPlayer: false,
        isLapBehind: false,
        isLapAhead: false,
      },
    ]);
  });
});
