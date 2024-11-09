import { Component, ViewEncapsulation } from '@angular/core';
import { DisplayComponent } from '@displays/display.component';
import { RallyData } from './rally-data';
import { SpeedometerComponent } from '@components/speedometer/speedometer.component';
import { DataElementComponent } from '@components/data-element/data-element.component';
import { DataGroupComponent } from '@components/data-group/data-group.component';

@Component({
  selector: 'app-rally-display',
  styleUrl: './rally-display.component.scss',
  templateUrl: './rally-display.component.html',
  standalone: true,
  encapsulation: ViewEncapsulation.None,
  imports: [
    SpeedometerComponent,
    DataElementComponent,
    DataGroupComponent,
  ],
})
export class RallyDisplayComponent extends DisplayComponent<RallyData> {
  protected override checkDataType(data: unknown): boolean {
    return (data as RallyData).gear !== undefined;
  }
  protected override createDefaultData(): RallyData {
    return new RallyData();
  }
}
