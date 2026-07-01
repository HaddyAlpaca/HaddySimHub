import { Component, ViewEncapsulation, inject } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { DeltaTimePipe, IRatingPipe, LapTimePipe, SpeedometerComponent } from '../../shared';
import { TelemetryTraceComponent } from './telemetry-trace.component';
import { TimePipe } from '../../shared/time/time.pipe';
import { APP_STORE } from '../../state/app.store';

@Component({
  selector: 'app-race-display',
  templateUrl: 'race-display.component.html',
  styleUrl: 'race-display.component.scss',
  encapsulation: ViewEncapsulation.None,
  imports: [
    DecimalPipe,
    DeltaTimePipe,
    IRatingPipe,
    LapTimePipe,
    TimePipe,
    SpeedometerComponent,
    TelemetryTraceComponent,
  ],
})
export class RaceDisplayComponent {
  private readonly _store = inject(APP_STORE);
  protected readonly data = this._store.raceData;
  protected get telemetrySample(): { brakePct: number; throttlePct: number; steeringPct: number } {
    return {
      brakePct: this.data().brakePct,
      throttlePct: this.data().throttlePct,
      steeringPct: this.data().steeringPct,
    };
  }

  protected rainLabel(intensity: number): string {
    if (intensity >= 3) {
      return 'Heavy';
    }
    if (intensity >= 2) {
      return 'Medium';
    }
    return 'Light';
  }

  protected gripLabel(status: number): string {
    switch (status) {
      case 0: return 'Green';
      case 1: return 'Fast';
      case 2: return 'Optimum';
      case 3: return 'Wet';
      default: return 'Unknown';
    }
  }
}
