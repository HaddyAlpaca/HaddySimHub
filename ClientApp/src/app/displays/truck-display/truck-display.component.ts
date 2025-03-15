import { ChangeDetectionStrategy, Component, input, ViewEncapsulation } from '@angular/core';
import { TruckData } from './truck-data';
import { NgClass, DecimalPipe, CommonModule } from '@angular/common';
import { WaypointComponent } from './waypoint.component';
import { TimespanPipe, NumberNlPipe, NumberFlexDigitPipe, DataType, GearPipe } from 'src/app/shared';

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
    GearPipe,
    NumberFlexDigitPipe,
  ],
})
export class TruckDisplayComponent {
  public readonly DataType = DataType;
  public data = input.required<TruckData>({});
}
