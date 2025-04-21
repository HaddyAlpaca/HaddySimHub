import { Component, input } from '@angular/core';
import { RaceData } from './race-data';
import { DashboardPageComponent } from './dashboard-page.component';
import { RelativePageComponent } from './relative-page.component';

@Component({
  selector: 'app-race-display',
  templateUrl: './race-display.component.html',
  imports: [
    DashboardPageComponent,
    RelativePageComponent,
  ],
})
export class RaceDisplayComponent {
  public data = input.required<RaceData>({});
}
