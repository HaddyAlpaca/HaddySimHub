import { Component, input, ViewEncapsulation } from '@angular/core';
import { SpeedometerComponent, LapTimePipe } from 'src/app/shared';

export interface RallyData {
  speed: number;
  gear: number;
  rpm: number;
  maxRpm: number;
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
})
export class RallyDisplayComponent {
  public data = input.required<RallyData>({});
}
