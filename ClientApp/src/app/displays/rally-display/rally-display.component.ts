import { Component, input, ViewEncapsulation } from '@angular/core';
import { RallyData } from './rally-data';
import { SpeedometerComponent } from '@components/speedometer/speedometer.component';
import { DataElementComponent, DataType } from '@components/data-element/data-element.component';
import { DataGroupComponent } from '@components/data-group/data-group.component';

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
