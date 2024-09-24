import { ChangeDetectionStrategy, Component, ViewEncapsulation } from '@angular/core';
import { TruckData } from './truck-data';
import { TimespanPipe } from './timespan.pipe';
import { NgClass, DecimalPipe, CommonModule } from '@angular/common';
import { NumberNlPipe } from '@components/number-nl/number-nl.pipe';
import { DisplayComponent } from '@displays/display.component';
import { WaypointComponent } from './waypoint.component';
import { SpeedometerComponent } from '@components/speedometer/speedometer.component';
import { GearPipe } from '@components/speedometer/gear.pipe';
import { NumberFlexDigitPipe } from '@components/number-flex-digit/number-flex-digit.pipe';

@Component({
    selector: 'app-truck-display',
    templateUrl: 'truck-display.component.html',
    styleUrl: 'truck-display.component.scss',
    encapsulation: ViewEncapsulation.None,
    changeDetection: ChangeDetectionStrategy.OnPush,
    standalone: true,
    imports: [
      NgClass,
      DecimalPipe,
      TimespanPipe,
      NumberNlPipe,
      CommonModule,
      WaypointComponent,
      SpeedometerComponent,
      GearPipe,
      NumberFlexDigitPipe,
    ],
})
export class TruckDisplayComponent extends DisplayComponent<TruckData> {
  protected override checkDataType(data: unknown): boolean {
    return (data as TruckData).sourceCity !== undefined;
  }

  protected override createDefaultData(): TruckData {
    return new TruckData();
  }
}
