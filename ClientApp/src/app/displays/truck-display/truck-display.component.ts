import { ChangeDetectionStrategy, Component, input, ViewEncapsulation } from '@angular/core';
import { TruckData } from './truck-data';
import { TimespanPipe } from './timespan.pipe';
import { NgClass, DecimalPipe, CommonModule } from '@angular/common';
import { NumberNlPipe } from '@components/number-nl/number-nl.pipe';
import { WaypointComponent } from './waypoint.component';
import { SpeedometerComponent } from '@components/speedometer/speedometer.component';
import { GearPipe } from '@components/speedometer/gear.pipe';
import { NumberFlexDigitPipe } from '@components/number-flex-digit/number-flex-digit.pipe';
import { DataType } from '@components/data-element/data-element.component';

@Component({
  selector: 'app-truck-display',
  templateUrl: 'truck-display.component.html',
  styleUrl: 'truck-display.component.scss',
  encapsulation: ViewEncapsulation.None,
  changeDetection: ChangeDetectionStrategy.OnPush,
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
export class TruckDisplayComponent {
  public readonly DataType = DataType;
  public data = input.required<TruckData>({});
}
