import { ComponentFixture, TestBed } from '@angular/core/testing';
import { TrackPosition, TrackPositionStatus, TrackPositionsComponent } from './track-positions.component';
import { TrackPositionsComponentHarness } from './track-positions.component.harness';
import { TestbedHarnessEnvironment } from '@angular/cdk/testing/testbed';
import { Component } from '@angular/core';

describe('TrackPositionsComponent tests', () => {
  let fixture: ComponentFixture<TrackPositionsTestHostComponent>;
  let component: TrackPositionsTestHostComponent;

  beforeEach(async () => {
    fixture = TestBed.createComponent(TrackPositionsTestHostComponent);
    component = fixture.componentInstance;
  });

  it('locates the track position elements correctly', async () => {
    component.positions = [
      {
        lapDistPct: 0,
        status: TrackPositionStatus.InPits,
      },
      {
        lapDistPct: 10,
        status: TrackPositionStatus.IsPlayer,
      },
    ]

    const harness = await TestbedHarnessEnvironment.harnessForFixture(fixture, TrackPositionsComponentHarness);
    const items = await harness.getTrackItems();

    expect(items).toEqual([{ style: 'left: 0%;' }, { style: 'left: 10%;' }]);
  });
});

@Component({
  template: '<app-track-positions [positions]="positions" />',
  standalone: true,
  imports: [TrackPositionsComponent],
})
class TrackPositionsTestHostComponent {
  public positions: TrackPosition[] = [];
}
