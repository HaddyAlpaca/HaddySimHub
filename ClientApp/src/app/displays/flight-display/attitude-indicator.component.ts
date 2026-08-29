import { Component, computed, input } from '@angular/core';

interface LadderRung {
  y: number;
  halfWidth: number;
  label: string;
}

/**
 * Artificial horizon. The sky/ground group rotates against the bank and slides
 * against the pitch, so the fixed aircraft symbol on top reads like the real
 * instrument. All geometry is in a 200x200 viewBox centred on (100, 100).
 */
@Component({
  selector: 'app-attitude-indicator',
  templateUrl: './attitude-indicator.component.html',
  styleUrl: './attitude-indicator.component.scss',
})
export class AttitudeIndicatorComponent {
  /** Pitch in degrees, positive nose up. */
  public readonly pitch = input.required<number>();

  /** Bank in degrees, positive banking right. */
  public readonly bank = input.required<number>();

  /** Slip/skid ball deflection, -1 (full left) to 1 (full right). */
  public readonly slip = input(0);

  /** Vertical pixels the horizon travels per degree of pitch. */
  private static readonly _pixelsPerDegree = 2.4;

  /** Ladder rungs to draw, in degrees from the horizon. */
  private static readonly _ladderDegrees = [-30, -20, -10, 10, 20, 30];

  /** Ball travel in viewBox units for full deflection. */
  private static readonly _slipTravel = 22;

  /**
   * Banking right rolls the aircraft clockwise, so the world rolls anticlockwise
   * relative to it -- hence the negated angle. Pitching up pushes the horizon down.
   */
  public readonly horizonTransform = computed(() => {
    const offset = (this.pitch() * AttitudeIndicatorComponent._pixelsPerDegree).toFixed(1);
    return `rotate(${-this.bank()} 100 100) translate(0 ${offset})`;
  });

  /** The sky pointer rides the horizon, so it uses the same rotation without the pitch slide. */
  public readonly bankPointerTransform = computed(() => `rotate(${-this.bank()} 100 100)`);

  public readonly ladder = computed<LadderRung[]>(() =>
    AttitudeIndicatorComponent._ladderDegrees.map((degrees) => ({
      y: 100 - degrees * AttitudeIndicatorComponent._pixelsPerDegree,
      halfWidth: Math.abs(degrees) === 10 ? 14 : Math.abs(degrees) === 20 ? 22 : 30,
      label: `${Math.abs(degrees)}`,
    })),
  );

  public readonly slipBallX = computed(() => {
    const clamped = Math.max(-1, Math.min(1, this.slip()));
    return 100 + clamped * AttitudeIndicatorComponent._slipTravel;
  });
}
