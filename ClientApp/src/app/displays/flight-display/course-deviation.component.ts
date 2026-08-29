import { Component, computed, input } from '@angular/core';

/**
 * Course deviation indicator: a needle showing where the selected course lies
 * relative to the aircraft, with an optional glideslope scale beside it.
 * Geometry is in a 440x90 viewBox -- wide and shallow, matching the shape of the
 * panel it sits in, so the instrument fills the space instead of being scaled down
 * to fit the shorter dimension.
 */
@Component({
  selector: 'app-course-deviation',
  templateUrl: './course-deviation.component.html',
  styleUrl: './course-deviation.component.scss',
})
export class CourseDeviationComponent {
  /** Lateral deviation, -1 to 1, positive when the aircraft is right of course. */
  public readonly lateral = input<number | null>(null);

  /** Glideslope deviation, -1 to 1, positive when the aircraft is above the path. */
  public readonly glideslope = input<number | null>(null);

  private static readonly _centreX = 170;
  private static readonly _centreY = 45;

  /** Horizontal travel of the needle at full scale. */
  private static readonly _lateralTravel = 120;

  /** Vertical travel of the glideslope marker at full scale. */
  private static readonly _glideslopeTravel = 32;

  private static readonly _glideslopeX = 390;

  public readonly hasLateral = computed(() => isPresent(this.lateral()));

  public readonly hasGlideslope = computed(() => isPresent(this.glideslope()));

  /**
   * The needle shows where the course is, not where the aircraft is, so it swings
   * opposite to the deviation: drift right of course and the needle sits left,
   * telling the pilot to fly left to recapture it.
   */
  public readonly needleX = computed(() =>
    CourseDeviationComponent._centreX - clamp(this.lateral()) * CourseDeviationComponent._lateralTravel,
  );

  /** Likewise inverted, and in screen coordinates: fly above the path and the marker drops. */
  public readonly glideslopeY = computed(() =>
    CourseDeviationComponent._centreY + clamp(this.glideslope()) * CourseDeviationComponent._glideslopeTravel,
  );

  /** The two dots either side of centre, at half and full scale. */
  public readonly lateralDots = computed(() =>
    [-1, -0.5, 0.5, 1].map(
      (fraction) => CourseDeviationComponent._centreX + fraction * CourseDeviationComponent._lateralTravel,
    ),
  );

  public readonly glideslopeDots = computed(() =>
    [-1, -0.5, 0.5, 1].map(
      (fraction) => CourseDeviationComponent._centreY + fraction * CourseDeviationComponent._glideslopeTravel,
    ),
  );

  public readonly centreX = CourseDeviationComponent._centreX;
  public readonly centreY = CourseDeviationComponent._centreY;
  public readonly glideslopeX = CourseDeviationComponent._glideslopeX;
}

const isPresent = (value: number | null): value is number =>
  value !== null && value !== undefined && !Number.isNaN(value);

/** Guards against a source that has not clamped, so the needle never leaves the dots. */
const clamp = (value: number | null): number => (isPresent(value) ? Math.max(-1, Math.min(1, value)) : 0);
