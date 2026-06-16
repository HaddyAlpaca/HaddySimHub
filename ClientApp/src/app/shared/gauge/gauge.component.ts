import { Component, computed, input } from '@angular/core';

@Component({
  selector: 'app-gauge',
  templateUrl: './gauge.component.html',
  styleUrl: './gauge.component.scss',
})
export class GaugeComponent {
  public readonly value = input.required<number>();
  public readonly max = input.required<number>();
  public readonly greenStart = input(0);
  public readonly greenEnd = input(0);
  public readonly redlineFraction = input(0.95);

  private readonly _cx = 100;
  private readonly _cy = 100;
  private readonly _trackR = 82;
  private readonly _tickOuter = 75;
  private readonly _tickMajorInner = 67;
  private readonly _tickMinorInner = 71;
  private readonly _labelR = 58;
  private readonly _startAngle = 235;
  private readonly _sweep = 250;

  public readonly degreesNum = computed(() => {
    const value = this.value();
    const max = this.max();
    if (value < 0 || max <= 0) {
      return 0;
    }
    return Math.round(-125 + (Math.min(value / max, 1) * 250));
  });

  private toSvgDeg(cssDeg: number): number {
    return (cssDeg + 270) % 360;
  }

  private arc(cssStart: number, cssEnd: number, r: number): string {
    const sDeg = this.toSvgDeg(cssStart);
    const eDeg = this.toSvgDeg(cssEnd);
    const sRad = sDeg * Math.PI / 180;
    const eRad = eDeg * Math.PI / 180;
    const x1 = +(this._cx + r * Math.cos(sRad)).toFixed(1);
    const y1 = +(this._cy + r * Math.sin(sRad)).toFixed(1);
    const x2 = +(this._cx + r * Math.cos(eRad)).toFixed(1);
    const y2 = +(this._cy + r * Math.sin(eRad)).toFixed(1);
    const large = (cssEnd - cssStart) > 180 ? 1 : 0;
    return `M ${x1} ${y1} A ${r} ${r} 0 ${large} 1 ${x2} ${y2}`;
  }

  private point(cssDeg: number, r: number): { x: number; y: number } {
    const deg = this.toSvgDeg(cssDeg);
    const rad = deg * Math.PI / 180;
    return {
      x: +(this._cx + r * Math.cos(rad)).toFixed(1),
      y: +(this._cy + r * Math.sin(rad)).toFixed(1),
    };
  }

  private fractionToCssAngle(fraction: number): number {
    return this._startAngle + fraction * this._sweep;
  }

  public readonly trackPath = computed(() =>
    this.arc(this._startAngle, this._startAngle + this._sweep, this._trackR),
  );

  public readonly zones = computed(() => {
    const max = this.max();
    if (max <= 0) {
      return [] as { path: string; color: string }[];
    }

    const gs = this.greenStart();
    const ge = this.greenEnd();
    const rf = this.redlineFraction();

    const zones: { path: string; color: string }[] = [];
    const endAngle = this._startAngle + this._sweep;

    if (gs > 0 && ge > gs) {
      const greenStart = this.fractionToCssAngle(Math.min(gs / max, 1));
      const greenEnd = this.fractionToCssAngle(Math.min(ge / max, 1));
      if (greenEnd > greenStart) {
        zones.push({ path: this.arc(greenStart, greenEnd, this._trackR), color: '#00cc66' });
      }

      const yellowEnd = this.fractionToCssAngle(Math.min(Math.max(rf, ge / max), 1));
      if (yellowEnd > greenEnd) {
        zones.push({ path: this.arc(greenEnd, yellowEnd, this._trackR), color: '#ffcc00' });
      }

      const redStart = this.fractionToCssAngle(Math.min(Math.max(rf, ge / max), 1));
      if (endAngle > redStart) {
        zones.push({ path: this.arc(redStart, endAngle, this._trackR), color: '#ff3300' });
      }
    } else if (rf < 1) {
      const redStart = this.fractionToCssAngle(rf);
      if (endAngle > redStart) {
        zones.push({ path: this.arc(redStart, endAngle, this._trackR), color: '#ff3300' });
      }
    }

    return zones;
  });

  public readonly ticks = computed(() => {
    const max = this.max();
    if (max <= 0) {
      return [];
    }
    const step = this.niceStep(max);
    const result: { x1: number; y1: number; x2: number; y2: number; major: boolean }[] = [];

    for (let rpm = 0; rpm <= max; rpm += step / 2) {
      const fraction = Math.min(rpm / max, 1);
      const angle = this.fractionToCssAngle(fraction);
      const isMajor = Math.abs(rpm % step) < 0.001;
      const innerR = isMajor ? this._tickMajorInner : this._tickMinorInner;
      const outer = this.point(angle, this._tickOuter);
      const inner = this.point(angle, innerR);
      result.push({ x1: outer.x, y1: outer.y, x2: inner.x, y2: inner.y, major: isMajor });
    }

    return result;
  });

  public readonly labels = computed(() => {
    const max = this.max();
    if (max <= 0) {
      return [];
    }
    const step = this.niceStep(max);
    const result: { x: number; y: number; text: string }[] = [];

    for (let rpm = 0; rpm <= max; rpm += step) {
      const fraction = Math.min(rpm / max, 1);
      const angle = this.fractionToCssAngle(fraction);
      const pt = this.point(angle, this._labelR);
      result.push({ x: pt.x, y: pt.y, text: this.formatRpm(rpm) });
    }

    return result;
  });

  private niceStep(max: number): number {
    const raw = max / 6;
    const magnitude = Math.pow(10, Math.floor(Math.log10(raw)));
    const residual = raw / magnitude;
    if (residual <= 1.5) {
      return magnitude;
    }
    if (residual <= 3.5) {
      return 2 * magnitude;
    }
    if (residual <= 7.5) {
      return 5 * magnitude;
    }
    return 10 * magnitude;
  }

  private formatRpm(rpm: number): string {
    return (rpm / 100).toFixed(0);
  }
}
