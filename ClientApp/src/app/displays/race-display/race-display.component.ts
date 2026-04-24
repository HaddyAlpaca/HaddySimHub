import { ChangeDetectionStrategy, Component, ViewEncapsulation, inject } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { DeltaTimePipe, LapTimePipe, SpeedometerComponent } from 'src/app/shared';
import { TelemetryTraceComponent } from './telemetry-trace.component';
import { TimePipe } from 'src/app/shared/time/time.pipe';
import { APP_STORE } from 'src/app/state/app.store';

@Component({
  selector: 'app-race-display',
  templateUrl: 'race-display.component.html',
  styleUrl: 'race-display.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  imports: [
    DecimalPipe,
    DeltaTimePipe,
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
}
