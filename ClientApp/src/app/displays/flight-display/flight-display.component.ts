import { Component, computed, inject, ViewEncapsulation } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { AttitudeIndicatorComponent } from './attitude-indicator.component';
import { HeadingIndicatorComponent } from './heading-indicator.component';
import { CourseDeviationComponent } from './course-deviation.component';
import { NumberNlPipe, TimespanPipe } from '../../shared';
import { APP_STORE } from '../../state/app.store';
import { CourseDeviationSource, EngineType, NavToFrom } from './flight-data';

export type { FlightData } from './flight-data';

/** Below this the fuel readout turns into a warning: the usual VFR reserve. */
const lowFuelSeconds = 45 * 60;

const defaulted = (value: number | undefined, fallback = 0): number => value ?? fallback;

const optional = (value: number | undefined): number | null => value ?? null;

@Component({
  selector: 'app-flight-display',
  styleUrl: './flight-display.component.scss',
  templateUrl: './flight-display.component.html',
  encapsulation: ViewEncapsulation.None,
  imports: [
    AttitudeIndicatorComponent,
    HeadingIndicatorComponent,
    CourseDeviationComponent,
    DecimalPipe,
    NumberNlPipe,
    TimespanPipe,
  ],
})
export class FlightDisplayComponent {
  private readonly _store = inject(APP_STORE);
  protected readonly data = this._store.flightData;

  // The store hands out an empty object until the first frame arrives, so the
  // instrument inputs below are defaulted rather than passed straight through --
  // a bare undefined would render as NaN inside the SVG transforms.
  protected readonly pitch = computed(() => defaulted(this.data().pitchDegrees));
  protected readonly bank = computed(() => defaulted(this.data().bankDegrees));
  protected readonly slip = computed(() => defaulted(this.data().slipBall));
  protected readonly heading = computed(() => defaulted(this.data().headingMagnetic));

  /** Null hides the marker, which is what we want before the first frame. */
  protected readonly groundTrackMarker = computed(() => optional(this.data().groundTrack));

  /** The heading bug only means something while the autopilot is flying. */
  protected readonly headingBugMarker = computed(() =>
    this.data().autopilotMaster ? optional(this.data().headingBug) : null,
  );

  // numbernl formats through Intl, which keeps up to three decimals, and the
  // pipe's separator swap then reads them as another thousands group. Everything
  // shown through it is rounded here first.
  protected readonly indicatedAltitude = computed(() => Math.round(defaulted(this.data().indicatedAltitude)));
  protected readonly altitudeAboveGround = computed(() => Math.round(defaulted(this.data().altitudeAboveGround)));
  protected readonly altitudeTarget = computed(() => Math.round(defaulted(this.data().altitudeTarget)));
  protected readonly fuelQuantity = computed(() => Math.round(defaulted(this.data().fuelQuantityLbs)));

  protected readonly engineRpm = computed(() => {
    const rpm = this.data().engineRpm;
    return rpm === undefined || rpm === null ? null : Math.round(rpm);
  });

  protected readonly hasGuidance = computed(() => this.data().deviationSource !== CourseDeviationSource.None);

  /** How the guidance source is labelled on the panel. */
  protected readonly deviationSourceLabel = computed(() => {
    switch (this.data().deviationSource) {
      case CourseDeviationSource.Gps:
        return 'GPS';
      case CourseDeviationSource.Vor:
        return 'VOR';
      case CourseDeviationSource.Localizer:
        return 'LOC';
      default:
        return null;
    }
  });

  protected readonly toFromLabel = computed(() => {
    switch (this.data().toFrom) {
      case NavToFrom.To:
        return 'TO';
      case NavToFrom.From:
        return 'FROM';
      default:
        return null;
    }
  });

  /**
   * Says what full scale means, so a needle hard over reads as a distance rather
   * than just "a long way". Radio guidance is angular, so it has no such distance.
   */
  protected readonly fullScaleText = computed(() => {
    const fullScale = this.data().lateralFullScaleNm;
    return fullScale === undefined || fullScale === null ? null : `${fullScale} NM`;
  });

  protected readonly lateralDeviation = computed(() => optional(this.data().lateralDeviation));

  protected readonly glideslopeDeviation = computed(() => optional(this.data().glideslopeDeviation));

  /**
   * A pressure setting is read as four bare digits ("1013"), never grouped as
   * "1,013", so it is formatted here rather than through the decimal pipe.
   */
  protected readonly altimeterText = computed(() => `${Math.round(defaulted(this.data().altimeterSettingHpa))}`);

  /** Highest selectable flap detent, i.e. the positions excluding clean. */
  protected readonly flapsMaxIndex = computed(() => Math.max(0, defaulted(this.data().flapsHandlePositions, 1) - 1));

  /** A glider has no engine panel to show. */
  protected readonly hasEngine = computed(() => {
    const data = this.data();
    return data.engineType !== EngineType.None && (data.engineCount ?? 0) > 0;
  });

  /**
   * Turbines are flown on N1, pistons on a percentage of maximum RPM. Same number,
   * different name, so the label has to follow the engine type.
   */
  protected readonly enginePrimaryLabel = computed(() =>
    this.data().engineType === EngineType.Piston ? '% RPM' : 'N1',
  );

  protected readonly isPiston = computed(() => this.data().engineType === EngineType.Piston);

  protected readonly fuelPct = computed(() => {
    const data = this.data();
    if (!data.fuelCapacityLbs) {
      return 0;
    }
    return Math.max(0, Math.min(100, (data.fuelQuantityLbs / data.fuelCapacityLbs) * 100));
  });

  protected readonly lowFuel = computed(() => {
    const endurance = this.data().fuelEnduranceSeconds;
    return endurance !== undefined && endurance !== null && endurance < lowFuelSeconds;
  });

  protected readonly enduranceText = computed(() => {
    const endurance = this.data().fuelEnduranceSeconds;
    if (endurance === undefined || endurance === null) {
      return null;
    }
    const hours = Math.floor(endurance / 3600);
    const minutes = Math.floor((endurance % 3600) / 60);
    return `${hours}:${minutes.toString().padStart(2, '0')}`;
  });

  protected readonly flapsLabel = computed(() => {
    const index = this.data().flapsHandleIndex ?? 0;
    return index === 0 ? 'UP' : `${index}`;
  });

  protected readonly gearState = computed(() => {
    const extended = this.data().gearPercentExtended ?? 0;
    if (extended >= 99) {
      return 'DOWN';
    }
    return extended <= 1 ? 'UP' : 'IN TRANSIT';
  });

  /** Names of the autopilot modes that are engaged, in the order a pilot scans them. */
  protected readonly autopilotModes = computed(() => {
    const data = this.data();
    if (!data.autopilotMaster) {
      return [];
    }

    const modes: string[] = [];
    if (data.headingHold) {
      modes.push('HDG');
    }
    if (data.navHold) {
      modes.push('NAV');
    }
    if (data.approachHold) {
      modes.push('APR');
    }
    if (data.altitudeHold) {
      modes.push('ALT');
    }
    if (data.speedHold) {
      modes.push('SPD');
    }
    return modes;
  });

  /** Armed spoilers read as ARMED until they actually deploy. */
  protected readonly spoilerText = computed(() => {
    const data = this.data();
    const deployed = data.spoilersPct ?? 0;
    if (data.spoilersArmed && deployed <= 0) {
      return 'ARMED';
    }
    return `${Math.round(deployed)}%`;
  });

  protected readonly crossTrackText = computed(() => {
    const error = this.data().crossTrackErrorNm;
    if (error === undefined || error === null) {
      return null;
    }
    const side = error >= 0 ? 'right' : 'left';
    return `${Math.abs(error).toFixed(2)} NM ${side} of course`;
  });

  protected readonly waypointEteMinutes = computed(() => this.toMinutes(this.data().waypointEteSeconds));

  protected readonly destinationEteMinutes = computed(() => this.toMinutes(this.data().destinationEteSeconds));

  /** Formats the ETA as the wall-clock UTC time a pilot would read off the panel. */
  protected readonly destinationEtaText = computed(() => {
    const eta = this.data().destinationEtaUtcSeconds;
    if (eta === undefined || eta === null) {
      return null;
    }
    const hours = Math.floor(eta / 3600) % 24;
    const minutes = Math.floor((eta % 3600) / 60);
    return `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}`;
  });

  private toMinutes(seconds: number | undefined): number | null {
    return seconds === undefined || seconds === null ? null : Math.round(seconds / 60);
  }
}
