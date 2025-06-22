import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

@Component({
  selector: 'app-gauge',
  templateUrl: './gauge.component.html',
  styleUrl: './gauge.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class GaugeComponent {
  public readonly value = input.required<number>();
  public readonly max = input.required<number>();
  public readonly degrees = computed(() => {
    const value = this.value();
    const max = this.max();
    if (value < 0 || max <= 0) {
      return 0;
    }

    // 0 to 100% maps to -125 to 125 degrees
    return Math.round(-125 + (Math.min(value / max, 1) * 250)) + 'deg';
  });
}
