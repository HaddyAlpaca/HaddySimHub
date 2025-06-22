import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

@Component({
  selector: 'app-waypoint',
  template: '<div class="data-item">{{description()}}</div>',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class WaypointComponent {
  public readonly city = input<string>();
  public readonly company = input<string>();
  protected readonly description = computed(() => {
    if (this.city() && this.company()) {
      return `${this.city()} (${this.company()})`;
    } else if(this.city()) {
      return this.city();
    }

    return '-';
  });
}
