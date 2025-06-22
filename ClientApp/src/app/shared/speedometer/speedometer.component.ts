import { ChangeDetectionStrategy, Component, ViewEncapsulation, input } from '@angular/core';

@Component({
  selector: 'app-speedometer',
  templateUrl: './speedometer.component.html',
  encapsulation: ViewEncapsulation.None,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SpeedometerComponent {
  public readonly speed = input.required<number>();
  public readonly rpm = input.required<number>();
  public readonly gear = input.required<string>();
}
