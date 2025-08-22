import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { DeltaTimePipe } from 'src/app/shared';
import { TimingEntry } from './race-data';
import { DecimalPipe } from '@angular/common';

@Component({
  selector: 'app-opponent-delta',
  templateUrl: './opponent-delta.component.html',
  styleUrl: './opponent-delta.component.scss',
  imports: [
    DeltaTimePipe,
    DecimalPipe,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OpponentDeltaComponent {
  public readonly caption = input.required<string>();
  public readonly driverInfo = input.required<TimingEntry | null>();
  protected readonly deltaTime = computed(() => {
    const driverInfo = this.driverInfo();
    if (!driverInfo) {
      return 0;
    }
    return Math.round(driverInfo.timeToPlayer * 10) / 10;
  });
}
