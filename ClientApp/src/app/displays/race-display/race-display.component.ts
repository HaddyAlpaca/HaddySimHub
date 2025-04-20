import { Component, input } from '@angular/core';
import { RaceData } from './race-data';

@Component({
    selector: 'app-race-display',
})
export class RaceDisplayComponent {
  public data = input.required<RaceData>({});
}