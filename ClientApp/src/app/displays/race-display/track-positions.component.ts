import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { TimingEntry } from './race-data';

@Component({
  selector: 'app-track-positions',
  templateUrl: './track-positions.component.html',
  styleUrl: './track-positions.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TrackPositionsComponent {
  public readonly positions = input<TimingEntry[]>([]);
}
