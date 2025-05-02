import { ChangeDetectionStrategy, Component, ViewEncapsulation, effect, input, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TrackPositionsComponent } from './track-positions.component';
import { OpponentDeltaComponent } from './opponent-delta.component';
import { DeltaTimePipe, LapTimePipe, SpeedometerComponent } from 'src/app/shared';
import { TelemetryTraceComponent } from './telemetry-trace.component';
import { RaceData, TimingEntry } from './race-data';

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

  private readonly _driverBehind = signal<TimingEntry | null>(null);
  public readonly driverBehind = this._driverBehind.asReadonly();

  private readonly _driverAhead = signal<TimingEntry | null>(null);
  public readonly driverAhead = this._driverAhead.asReadonly();

  public constructor() {
    effect(() => {
      const entries = this.data().timingEntries?.sort((a, b) => b.timeToPlayer - a.timeToPlayer);

      if (!entries) {
        this._driverBehind.set(null);
        this._driverAhead.set(null);
        return;
      }

      const playerIndex = entries.findIndex(e => e.isPlayer);
      const driverAhead = playerIndex > 0 ? entries[playerIndex - 1] : null;
      const driverBehind = playerIndex < entries.length - 1 ? entries[playerIndex + 1] : null;

      this._driverBehind.set(driverBehind);
      this._driverAhead.set(driverAhead);
    });
  }
}
