import { Component, computed, inject, ViewEncapsulation } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { WaypointComponent } from './waypoint.component';
import { TimespanPipe, NumberNlPipe, NumberFlexDigitPipe } from '../../shared';
import { GaugeComponent } from '../../shared/gauge/gauge.component';
import { APP_STORE } from '../../state/app.store';

export type { TruckData } from './truck-data';

const damageWarningThreshold = 25;
const damageCriticalThreshold = 50;


@Component({
  selector: 'app-truck-display',
  templateUrl: 'truck-display.component.html',
  styleUrl: 'truck-display.component.scss',
  encapsulation: ViewEncapsulation.None,
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

  protected readonly fuelRangeTooShort = computed(() => {
    const data = this.data();
    return data.distanceRemaining > 0 && data.fuelDistance < data.distanceRemaining;
  });

  protected damageClass(value: number): string {
    if (value >= damageCriticalThreshold) {
      return 'damage-critical';
    }

    if (value >= damageWarningThreshold) {
      return 'damage-warning';
    }

    return '';
  }
}
