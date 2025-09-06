import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TrackPositionsComponent } from './track-positions.component';
import { TrackPositionsComponentHarness } from './track-positions.component.harness';
import { TestbedHarnessEnvironment } from '@angular/cdk/testing/testbed';
import { inputBinding, provideZonelessChangeDetection } from '@angular/core';
import { TimingEntry } from './race-data';

describe('TrackPositionsComponent tests', () => {
  let fixture: ComponentFixture<TrackPositionsComponent>;
  let positions: TimingEntry[];

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      providers: [
        provideZonelessChangeDetection(),
      ],
    }).compileComponents();

    positions = [];

    fixture = TestBed.createComponent(TrackPositionsComponent, {
      bindings: [
        inputBinding('positions', () => positions),
      ],
    });
  });

  it('locates the track position elements correctly', async () => {
    positions = [
      {
        lapCompletedPct: 0,
        isInPits: true,
      } as TimingEntry,
      {
        lapCompletedPct: 10,
        isPlayer: true,
      } as TimingEntry,
    ];

    const harness = await TestbedHarnessEnvironment.harnessForFixture(fixture, TrackPositionsComponentHarness);
    const items = await harness.getTrackItems();

    expect(items).toEqual([{ style: 'left: 0%;' }, { style: 'left: 10%;' }]);
  });
});
