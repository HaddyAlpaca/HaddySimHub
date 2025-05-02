import { ChangeDetectionStrategy, Component, ViewEncapsulation, input } from '@angular/core';
import { GearPipe } from '../gear/gear.pipe';

@Component({
  selector: 'app-speedometer',
  templateUrl: './speedometer.component.html',
  encapsulation: ViewEncapsulation.None,
  imports: [GearPipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SpeedometerComponent {
  public speed = input.required<number>();
  public rpm = input.required<number>();
  public gear = input.required<number>();
  public multiReverse = input(false);
}
