import { Component, computed, effect, ElementRef, inject, ViewEncapsulation } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { WaypointComponent } from './waypoint.component';
import { TimespanPipe, NumberNlPipe, NumberFlexDigitPipe } from '../../shared';
import { GaugeComponent } from '../../shared/gauge/gauge.component';
import { APP_STORE } from '../../state/app.store';

export type { TruckData } from './truck-data';

const damageWarningThreshold = 25;
const damageCriticalThreshold = 50;

const rpmGreenZoneStart = 1250;
const rpmGreenZoneEnd = 2000;

const minutesPerDay = 24 * 60;


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
  private readonly _elementRef = inject(ElementRef);
  protected readonly data = this._store.truckData;

  private static readonly _minBrightness = 0.3;

  protected readonly dashboardBrightness = computed(() => {
    const backlight = this.data().dashboardBacklight;
    if (!backlight || backlight <= 0) {
      return TruckDisplayComponent._minBrightness;
    }
    return Math.max(TruckDisplayComponent._minBrightness, Math.min(backlight, 1));
  });

  public constructor() {
    effect(() => {
      const el = this._elementRef.nativeElement as HTMLElement;
      el.style.setProperty('--dashboard-brightness', `${this.dashboardBrightness()}`);
    });
  }

  protected readonly rpmGreenZoneStart = rpmGreenZoneStart;
  protected readonly rpmGreenZoneEnd = rpmGreenZoneEnd;

  protected readonly fuelRangeTooShort = computed(() => {
    const data = this.data();
    return data.distanceRemaining > 0 && data.fuelDistance < data.distanceRemaining;
  });

  protected readonly fuelPct = computed(() => {
    const data = this.data();
    if (data.fuelCapacity <= 0) {
      return 0;
    }
    return Math.round((data.fuelAmount / data.fuelCapacity) * 100);
  });

  protected readonly adBluePct = computed(() => {
    const data = this.data();
    if (data.adBlueCapacity <= 0) {
      return 0;
    }
    return Math.round((data.adBlueAmount / data.adBlueCapacity) * 100);
  });

  protected readonly gameTimeFormatted = computed(() => {
    const data = this.data();
    const minutes = data.gameTime % minutesPerDay;
    const hours = Math.floor(minutes / 60);
    const mins = minutes % 60;
    return `${hours.toString().padStart(2, '0')}:${mins.toString().padStart(2, '0')}`;
  });

  protected readonly restRatio = computed(() => {
    const data = this.data();
    if (data.timeRemaining <= 0) {
      return 1;
    }
    return Math.min(data.restTimeRemaining / data.timeRemaining, 1);
  });

  protected readonly arrivalTime = computed(() => {
    const data = this.data();
    if (data.timeRemaining <= 0) {
      return '';
    }

    const minutesOfDay = (data.gameTime + data.timeRemaining) % minutesPerDay;
    const hours = Math.floor(minutesOfDay / 60);
    const minutes = minutesOfDay % 60;
    return `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}`;
  });

  protected readonly restBeforeArrival = computed(() => {
    const data = this.data();
    return data.restTimeRemaining > 0 && data.timeRemaining > data.restTimeRemaining;
  });

  protected readonly gearAdvice = computed(() => {
    const data = this.data();
    const recommended = data.recommendedGear;
    return recommended && recommended !== `${data.gear}` ? recommended : '';
  });

  protected readonly shiftDirection = computed(() => {
    const data = this.data();
    const recommended = this.gearAdvice();
    if (!recommended) {
      return '';
    }

    const recommendedGear = Number.parseInt(recommended, 10);
    const currentGear = Number.parseInt(data.gear, 10);
    if (Number.isNaN(recommendedGear) || Number.isNaN(currentGear)) {
      return '';
    }

    return recommendedGear > currentGear ? 'up' : 'down';
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
