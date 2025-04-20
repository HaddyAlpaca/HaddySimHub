import { ChangeDetectionStrategy, Component, ViewEncapsulation, computed, effect, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TrackPositionsComponent } from './track-positions.component';
import { OpponentDeltaComponent } from './opponent-delta.component';
import { DeltaTimePipe, LapTimePipe, SpeedometerComponent } from 'src/app/shared';
import { TelemetryTraceComponent } from './telemetry-trace.component';
import { RaceData } from './race-data';

@Component({
  selector: 'app-dashboard-page',
  templateUrl: 'dashboard-page.component.html',
  styleUrl: 'dashboard-page.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  imports: [
    CommonModule,
    DeltaTimePipe,
    LapTimePipe,
    TrackPositionsComponent,
    SpeedometerComponent,
    OpponentDeltaComponent,
    TelemetryTraceComponent,
  ],
})
export class DashboardPageComponent {
  public data = input.required<RaceData>({});

  public driverBehindInfo = computed(() => ({
  }));

  public driverAheadInfo = computed(() => ({
  }));
}
