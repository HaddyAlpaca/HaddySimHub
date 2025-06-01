import { ChangeDetectionStrategy, Component, computed, inject, ViewEncapsulation } from '@angular/core';
import { SpeedometerComponent, LapTimePipe } from 'src/app/shared';
import { SignalRService } from 'src/app/signalr.service';

export interface RallyData {
  speed: number;
  gear: string;
  rpm: number;
  rpmMax: number;
  distanceTravelled: number;
  completedPct: number;
  timeElapsed: number;
  sector1Time: number;
  sector2Time: number;
  lapTime: number;
  sector: number;
  position: number;
  brakeTempFl: number;
  brakeTempFr: number;
  brakeTempRl: number;
  brakeTempRr: number;
  tyrePressFl: number;
  tyrePressFr: number;
  tyrePressRl: number;
  tyrePressRr: number;
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
  private readonly _signalRService = inject(SignalRService);
  protected readonly data = computed(() => (this._signalRService.displayData()?.data ?? {}) as RallyData);
}
