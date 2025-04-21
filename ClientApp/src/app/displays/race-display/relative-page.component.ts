import { Component, input } from '@angular/core';
import { TimingEntry } from './race-data';

@Component({
  selector: 'app-relative-page',
  templateUrl: 'relative-page.component.html',
  styleUrl: 'relative-page.component.scss',
})
export class RelativePageComponent {
    public timingEntries = input.required<TimingEntry[]>();
}
