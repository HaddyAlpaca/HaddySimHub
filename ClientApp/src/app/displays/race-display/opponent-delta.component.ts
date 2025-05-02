import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { DeltaTimePipe } from 'src/app/shared';
import { TimingEntry } from './race-data';

@Component({
  selector: 'app-opponent-delta',
  templateUrl: './opponent-delta.component.html',
  styleUrl: './opponent-delta.component.scss',
  imports: [DeltaTimePipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OpponentDeltaComponent {
  public caption = input.required<string>();
  public driverInfo = input.required<TimingEntry | null>();
  public deltaTime = computed(() => {
    const driverInfo = this.driverInfo();
    if (!driverInfo) {
      return 0;
    }
    return Math.round(driverInfo.timeToPlayer * 10) / 10;
  });
}
