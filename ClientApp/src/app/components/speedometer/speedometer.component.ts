import { Component, ViewEncapsulation, input } from '@angular/core';
import { GearPipe } from './gear.pipe';

@Component({
  selector: 'app-speedometer',
  templateUrl: './speedometer.component.html',
  encapsulation: ViewEncapsulation.None,
  standalone: true,
  imports: [GearPipe],
})
export class SpeedometerComponent {
  public speed = input.required<number>();
  public rpm = input.required<number>();
  public gear = input.required<number>();
  public multiReverse = input(false);
}
