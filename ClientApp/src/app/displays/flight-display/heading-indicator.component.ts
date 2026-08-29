import { Component, computed, input } from '@angular/core';

interface HeadingTick {
  x: number;
  major: boolean;
  label: string | null;
}

interface Marker {
  x: number;
  clamped: boolean;
}

/**
 * Horizontal heading tape: the scale slides under a fixed centre pointer, with
 * markers for the autopilot heading bug and the actual ground track. Geometry is
 * in a 600x64 viewBox -- deliberately wide, so that at full panel width the tape
 * stays a band rather than growing into a block.
 */
@Component({
  selector: 'app-heading-indicator',
  templateUrl: './heading-indicator.component.html',
  styleUrl: './heading-indicator.component.scss',
})
export class HeadingIndicatorComponent {
  /** Magnetic heading in degrees. */
  public readonly heading = input.required<number>();

  /** Autopilot selected heading in degrees; hidden when null. */
  public readonly headingBug = input<number | null>(null);

  /** Magnetic ground track in degrees; hidden when null. */
  public readonly groundTrack = input<number | null>(null);

  private static readonly _centre = 300;
  private static readonly _pixelsPerDegree = 6;

  /** Degrees either side of centre that fit on the tape. */
  private static readonly _halfSpan = 48;

  public readonly ticks = computed<HeadingTick[]>(() => {
    const heading = this.heading();
    const span = HeadingIndicatorComponent._halfSpan;
    const result: HeadingTick[] = [];

    // Ticks sit on absolute multiples of five degrees so they slide under the
    // pointer instead of being pinned to the current heading.
    const first = Math.ceil((heading - span) / 5) * 5;
    for (let degrees = first; degrees <= heading + span; degrees += 5) {
      const major = ((degrees % 10) + 10) % 10 === 0;
      const normalized = ((degrees % 360) + 360) % 360;
      result.push({
        x: HeadingIndicatorComponent._centre + (degrees - heading) * HeadingIndicatorComponent._pixelsPerDegree,
        major,
        label: major ? `${normalized / 10}`.padStart(2, '0') : null,
      });
    }

    return result;
  });

  public readonly bugMarker = computed(() => this.marker(this.headingBug()));

  public readonly trackMarker = computed(() => this.marker(this.groundTrack()));

  /**
   * Places a marker for an absolute heading, taking the short way round the compass
   * so a bug at 010 with a heading of 350 sits to the right rather than far left.
   * Off-tape markers are pinned to the edge and flagged, so the template can show
   * them as "that way" instead of hiding them.
   */
  private marker(value: number | null): Marker | null {
    if (value === null || value === undefined || Number.isNaN(value)) {
      return null;
    }

    let delta = value - this.heading();
    delta = ((delta % 360) + 540) % 360 - 180;

    const span = HeadingIndicatorComponent._halfSpan;
    const clamped = Math.abs(delta) > span;
    const shown = Math.max(-span, Math.min(span, delta));

    return {
      x: HeadingIndicatorComponent._centre + shown * HeadingIndicatorComponent._pixelsPerDegree,
      clamped,
    };
  }
}
