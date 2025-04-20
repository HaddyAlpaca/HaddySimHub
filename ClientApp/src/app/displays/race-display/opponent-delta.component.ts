import { Component, input } from '@angular/core';
import { DeltaTimePipe, IRatingPipe } from 'src/app/shared';
import { TimingEntry } from './race-data';

@Component({
  selector: 'app-opponent-delta',
  templateUrl: './opponent-delta.component.html',
  styleUrl: './opponent-delta.component.scss',
  imports: [
    DeltaTimePipe,
    IRatingPipe,
  ],
})
export class OpponentDeltaComponent {
  public caption = input.required<string>();
  public driverInfo = input.required<TimingEntry | null>();
}
