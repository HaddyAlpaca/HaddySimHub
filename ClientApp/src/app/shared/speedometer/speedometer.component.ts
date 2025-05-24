import { ChangeDetectionStrategy, Component, ViewEncapsulation, computed, input } from '@angular/core';

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

  protected readonly showRpmGreen = computed(() => {
    const rpm = this.rpm();
    const rpmGreen = this.rpmGreen();
    const rpmRed = this.rpmRed();
    const rpmMax = this.rpmMax();

    if (rpmGreen <= 0) {
      return false;
    }

    if (rpmRed > rpmGreen && rpmRed > 0) {
      return rpm > rpmGreen && rpm < rpmRed;
    }

    if (rpmMax > rpmGreen && rpmMax > 0) {
      return rpm > rpmGreen && rpm < rpmMax;
    }

    return rpm > rpmGreen;
  });

  protected readonly showRpmRed = computed(() => {
    const rpmRed = this.rpmRed();
    
    if (rpmRed <= 0) {
      return false;
    }
    
    return this.rpm() >= rpmRed;
  }); 

  protected readonly showRpmMax = computed(() => {
    const rpmMax = this.rpmMax();

    if (rpmMax <= 0) {
      return false;
    }

    return this.rpm() >= rpmMax;
  });
}
