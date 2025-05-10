import { ChangeDetectionStrategy, Component, ViewEncapsulation, input } from '@angular/core';

@Component({
  selector: 'app-speedometer',
  templateUrl: './speedometer.component.html',
  encapsulation: ViewEncapsulation.None,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SpeedometerComponent {
  public speed = input.required<number>();
  public rpm = input.required<number>();
  public rpmGreen = input<number>(0);
  public rpmRed = input<number>(0);
  public rpmMax = input<number>(0);
  public gear = input.required<string>();
}
