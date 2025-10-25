import { ChangeDetectionStrategy, Component, ViewEncapsulation, computed, effect, inject, viewChild } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { DeltaTimePipe, LapTimePipe, SpeedometerComponent } from 'src/app/shared';
import { TelemetryTraceComponent } from './telemetry-trace.component';
import { RaceData } from './race-data';
import { SignalRService } from 'src/app/signalr.service';
import { RpmLightsComponent } from 'src/app/shared/rpm-lights/rpm-lights.component';
import { TimePipe } from 'src/app/shared/time/time.pipe';

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
  private readonly _signalRService = inject(SignalRService);
  protected readonly data = computed(() => (this._signalRService.displayData()?.data ?? {}) as RaceData);

  private readonly _telemetryTraceComponent = viewChild.required(TelemetryTraceComponent);

  public constructor() {
    effect(() => {
      this._telemetryTraceComponent().update({
        brakePct: this.data().brakePct,
        throttlePct: this.data().throttlePct,
        steeringPct: this.data().steeringPct,
      });
    });
  }
}
