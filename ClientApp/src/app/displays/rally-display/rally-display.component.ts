import { Component, input, ViewEncapsulation } from '@angular/core';
import { RallyData } from './rally-data';
import { SpeedometerComponent, DataElementComponent, DataGroupComponent, DataType } from 'src/app/shared';

@Component({
  selector: 'app-rally-display',
  styleUrl: './rally-display.component.scss',
  templateUrl: './rally-display.component.html',
  encapsulation: ViewEncapsulation.None,
  imports: [
    SpeedometerComponent,
    DataElementComponent,
    DataGroupComponent,
  ],
})
export class RallyDisplayComponent {
  public readonly DataType = DataType;
  public data = input.required<RallyData>({});
}
