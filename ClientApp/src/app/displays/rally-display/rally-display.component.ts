import { ChangeDetectionStrategy, Component, inject, ViewEncapsulation } from '@angular/core';
import { SpeedometerComponent, LapTimePipe } from 'src/app/shared';
import { APP_STORE } from 'src/app/state/app.store';

export interface RallyData {
  speed: number;
  gear: string;
  rpm: number;
  rpmMax: number;
  distanceTravelled: number;
  completedPct: number;
  sector1Time: number;
  sector2Time: number;
  lapTime: number;
  position: number;
}

@Component({
  selector: 'app-rally-display',
  styleUrl: './rally-display.component.scss',
  templateUrl: './rally-display.component.html',
  encapsulation: ViewEncapsulation.None,
  imports: [
    SpeedometerComponent,
    LapTimePipe,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RallyDisplayComponent {
  private readonly _store = inject(APP_STORE);
  protected readonly data = this._store.rallyData;
}
