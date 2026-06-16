import { Component, ViewEncapsulation, input } from '@angular/core';

@Component({
  selector: 'app-speedometer',
  templateUrl: './speedometer.component.html',
  encapsulation: ViewEncapsulation.None,
})
export class SpeedometerComponent {
  public readonly speed = input.required<number>();
  public readonly rpm = input.required<number>();
  public readonly gear = input.required<string>();
}
