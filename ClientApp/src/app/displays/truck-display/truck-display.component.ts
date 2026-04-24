import { ChangeDetectionStrategy, Component, inject, ViewEncapsulation } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { WaypointComponent } from './waypoint.component';
import { TimespanPipe, NumberNlPipe, NumberFlexDigitPipe } from 'src/app/shared';
import { GaugeComponent } from 'src/app/shared/gauge/gauge.component';
import { APP_STORE } from 'src/app/state/app.store';

export type { TruckData } from './truck-data';

@Component({
  selector: 'app-truck-display',
  templateUrl: 'truck-display.component.html',
  styleUrl: 'truck-display.component.scss',
  encapsulation: ViewEncapsulation.None,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    DecimalPipe,
    TimespanPipe,
    NumberNlPipe,
    WaypointComponent,
    NumberFlexDigitPipe,
    GaugeComponent,
  ],
})
export class TruckDisplayComponent {
  private readonly _store = inject(APP_STORE);
  protected readonly data = this._store.truckData;
}
