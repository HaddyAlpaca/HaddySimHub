import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TrackPositionsComponent } from './track-positions.component';
import { TrackPositionsComponentHarness } from './track-positions.component.harness';
import { TestbedHarnessEnvironment } from '@angular/cdk/testing/testbed';
import { Component, provideExperimentalZonelessChangeDetection } from '@angular/core';
import { TimingEntry } from './race-data';

describe('TrackPositionsComponent tests', () => {
  let fixture: ComponentFixture<TrackPositionsTestHostComponent>;
  let component: TrackPositionsTestHostComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      providers: [
        provideExperimentalZonelessChangeDetection(),
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(TrackPositionsTestHostComponent);
    component = fixture.componentInstance;
  });

  it('locates the track position elements correctly', async () => {
    component.positions = [
      {
        distancePct: 0,
        isInPits: true,
      } as TimingEntry,
      {
        distancePct: 10,
        isPlayer: true,
      } as TimingEntry,
    ];

    const harness = await TestbedHarnessEnvironment.harnessForFixture(fixture, TrackPositionsComponentHarness);
    const items = await harness.getTrackItems();

    expect(items).toEqual([{ style: 'left: 0%;' }, { style: 'left: 10%;' }]);
  });
});

@Component({
  template: '<app-track-positions [positions]="positions" />',
  imports: [TrackPositionsComponent],
})
class TrackPositionsTestHostComponent {
  public positions: TimingEntry[] = [];
}
