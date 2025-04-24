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
  public sortedEntries = computed(() => this.timingEntries().sort((a, b) => a.position - b.position));
}
