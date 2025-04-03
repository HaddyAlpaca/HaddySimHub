import { ChangeDetectionStrategy, Component, ViewEncapsulation, computed, effect, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TrackPosition, TrackPositionsComponent } from './track-positions.component';
import { OpponentDeltaComponent } from './opponent-delta.component';
import { DataElementComponent, DataType, DeltaTimePipe, LapTimePipe, SpeedometerComponent } from 'src/app/shared';
import { TelemetryTraceComponent } from './telemetry-trace.component';

export interface RaceData {
  sessionType: string;
  IsLimitedTime: boolean;
  isLimitedSessionLaps: boolean;
  currentLap: number;
  totalLaps: number;
  sessionTimeRemaining: number;
  position: number;
  speed: number;
  gear: number;
  rpm: number;
  trackTemp: number;
  airTemp: number;
  fuelRemaining: number;
  brakeBias: number;
  strengthOfField: number;
  lastSectorNum: number;
  lastSectorTime: number;
  lastLapTime: number;
  lastLapTimeDelta: number;
  bestLapTime: number;
  bestLapTimeDelta: number;
  driverBehindName: string;
  driverBehindLicense: string;
  driverBehindLicenseColor: string;
  driverBehindIRating: number;
  driverBehindDelta: number;
  driverAheadName: string;
  driverAheadLicense: string;
  driverAheadLicenseColor: string;
  driverAheadIRating: number;
  driverAheadDelta: number;
  pitLimiterOn: boolean;
  incidents: number;
  maxIncidents: number;
  flag: '' | 'yellow' | 'green' | 'blue' | 'white' | 'finish' | 'black' | 'black-orange' | 'red';
  trackPositions: TrackPosition[];
  brakePct: number;
  throttlePct: number;
}

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
    TelemetryTraceComponent,
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
