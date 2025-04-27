import { Component, computed, input } from '@angular/core';
import { TimingEntry } from './race-data';
import { DeltaTimePipe } from 'src/app/shared';

@Component({
  selector: 'app-relative-page',
  templateUrl: 'relative-page.component.html',
  styleUrl: 'relative-page.component.scss',
  imports: [DeltaTimePipe],
})
export class RelativePageComponent {
  public timingEntries = input.required<TimingEntry[]>();
  public sortedEntries = computed(() => { 
    var entries = this.timingEntries().sort((a, b) => b.timeRelativeToPlayer - a.timeRelativeToPlayer);

    var playerIndex = entries.findIndex(e => e.isPlayer);
    if (playerIndex < 0) {
      return entries;
    }

    var startIndex = Math.max(0, playerIndex - 5); 
    return entries.slice(startIndex, entries.length);
  });
}
