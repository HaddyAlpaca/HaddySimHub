import { ChangeDetectionStrategy, Component, ViewEncapsulation, computed, effect } from '@angular/core';
import { RaceData } from './race-data';
import { CommonModule } from '@angular/common';
import { IRatingPipe } from './irating.pipe';
import { TrackPositionsComponent } from './track-positions.component';
import { DisplayComponent } from '@displays/display.component';
import { SpeedometerComponent } from '@components/speedometer/speedometer.component';
import { DataElementComponent } from '@components/data-element/data-element.component';
import { OpponentDeltaComponent } from './opponent-delta.component';
import { DeltaTimePipe } from '@components/delta-time/delta-time.pipe';
import { LapTimePipe } from '@components/laptime/laptime.pipe';
import { TimespanPipe } from '@components/timespan/timespan.pipe';

@Component({
  selector: 'app-race-display',
  templateUrl: 'race-display.component.html',
  styleUrl: 'race-display.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  standalone: true,
  imports: [
    CommonModule,
    DeltaTimePipe,
    LapTimePipe,
    TimespanPipe,
    IRatingPipe,
    TrackPositionsComponent,
    SpeedometerComponent,
    DataElementComponent,
    OpponentDeltaComponent,
  ],
})
export class RaceDisplayComponent extends DisplayComponent<RaceData> {
  protected override checkDataType(data: unknown): boolean {
    return (data as RaceData).gear !== undefined;
  }

  protected override createDefaultData(): RaceData {
    return new RaceData();
  }
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

  public driverBehindInfo = computed(() => {
    return {
      name: this.data().driverBehindName,
      license: this.data().driverBehindLicense,
      licenseColor: this.data().driverBehindLicenseColor,
      rating: this.data().driverBehindIRating,
      delta: this.data().driverBehindDelta,
    }
  });

  public driverAheadInfo = computed(() => {
    return {
      name: this.data().driverAheadName,
      license: this.data().driverAheadLicense,
      licenseColor: this.data().driverAheadLicenseColor,
      rating: this.data().driverAheadIRating,
      delta: this.data().driverAheadDelta,
    }
  });


  constructor() {
    super();

    effect(() => {
      const data = this.data();

      this._gapBehindDelta = data.driverBehindDelta - this._lastGapBehind;
      this._lastGapBehind = data.driverBehindDelta;
      this._gapAheadDelta = data.driverAheadDelta - this._lastGapAhead;
      this._lastGapAhead = data.driverAheadDelta;
    });

  }
}
