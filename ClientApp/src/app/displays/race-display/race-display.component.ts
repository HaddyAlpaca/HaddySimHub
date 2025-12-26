import { ChangeDetectionStrategy, Component, ViewEncapsulation, computed, inject } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { DeltaTimePipe, LapTimePipe, SpeedometerComponent } from 'src/app/shared';
import { TelemetrySample, TelemetryTraceComponent } from './telemetry-trace.component';
import { RpmLightsComponent } from 'src/app/shared/rpm-lights/rpm-lights.component';
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
    RpmLightsComponent,
  ],
})
export class RaceDisplayComponent {
  private readonly _store = inject(APP_STORE);
  protected readonly data = this._store.raceData;

  protected readonly telemetrySample = computed(() => {
    const sample = {
      brakePct: this.data().brakePct,
      throttlePct: this.data().throttlePct,
      steeringPct: this.data().steeringPct,
    } as TelemetrySample;
    return sample;
  });
}
