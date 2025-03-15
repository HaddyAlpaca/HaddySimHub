import { ChangeDetectionStrategy, Component, ViewEncapsulation, computed, effect, input } from '@angular/core';
import { RaceData } from './race-data';
import { CommonModule } from '@angular/common';
import { TrackPositionsComponent } from './track-positions.component';
import { OpponentDeltaComponent } from './opponent-delta.component';
import { DataElementComponent, DataType, DeltaTimePipe, LapTimePipe, SpeedometerComponent } from 'src/app/shared';

@Component({
  selector: 'app-race-display',
  templateUrl: 'race-display.component.html',
  styleUrl: 'race-display.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  imports: [
    CommonModule,
    DeltaTimePipe,
    LapTimePipe,
    TrackPositionsComponent,
    SpeedometerComponent,
    DataElementComponent,
    OpponentDeltaComponent,
  ],
})
export class RaceDisplayComponent {
  public readonly DataType = DataType;
  public data = input.required<RaceData>({});

  private _lastGapBehind = 0;
  private _lastGapAhead = 0;

  private _gapBehindDelta = 0;
  public get gapBehindDelta(): number {
    return this._gapBehindDelta;
  }

  private _gapAheadDelta = 0;
  public get gapAheadDelta(): number {
    return this._gapAheadDelta;
  }

  public driverBehindInfo = computed(() => ({
    name: this.data().driverBehindName,
    license: this.data().driverBehindLicense,
    licenseColor: this.data().driverBehindLicenseColor,
    rating: this.data().driverBehindIRating,
    delta: this.data().driverBehindDelta,
  }));

  public driverAheadInfo = computed(() => ({
    name: this.data().driverAheadName,
    license: this.data().driverAheadLicense,
    licenseColor: this.data().driverAheadLicenseColor,
    rating: this.data().driverAheadIRating,
    delta: this.data().driverAheadDelta,
  }));


  public constructor() {
    effect(() => {
      const data = this.data();

      this._gapBehindDelta = data.driverBehindDelta - this._lastGapBehind;
      this._lastGapBehind = data.driverBehindDelta;
      this._gapAheadDelta = data.driverAheadDelta - this._lastGapAhead;
      this._lastGapAhead = data.driverAheadDelta;
    });

  }
}
