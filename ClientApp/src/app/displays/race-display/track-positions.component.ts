import { NgClass } from '@angular/common';
import { Component, input } from '@angular/core';
import { TimingEntry } from './race-data';

@Component({
  selector: 'app-track-positions',
  templateUrl: './track-positions.component.html',
  styleUrl: './track-positions.component.scss',
  imports: [NgClass],
})
export class TrackPositionsComponent {
  public positions = input<TimingEntry[]>([]);
}
