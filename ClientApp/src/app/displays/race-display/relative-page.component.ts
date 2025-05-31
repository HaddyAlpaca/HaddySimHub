import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { DeltaTimePipe } from 'src/app/shared';
import { SignalRService } from 'src/app/signalr.service';
import { RaceData } from './race-data';

@Component({
  selector: 'app-relative-page',
  templateUrl: 'relative-page.component.html',
  styleUrl: 'relative-page.component.scss',
  imports: [DeltaTimePipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RelativePageComponent {
  private readonly _signalRService = inject(SignalRService);
  protected readonly timingEntries = computed(() => (this._signalRService.displayData()?.data as RaceData).timingEntries || []);

  protected readonly sortedEntries = computed(() => {
    const entries = this.timingEntries()
      .map(entry => ({
        ...entry,
        timeToPlayer: Math.round(entry.timeToPlayer * 10) / 10,
      }))
      .sort((a, b) => b.timeToPlayer - a.timeToPlayer);

    const playerIndex = entries.findIndex(e => e.isPlayer);
    if (playerIndex < 0) {
      return entries;
    }

    const startIndex = Math.max(0, playerIndex - 5);
    return entries.slice(startIndex, Math.max(entries.length, 10));
  });
}
