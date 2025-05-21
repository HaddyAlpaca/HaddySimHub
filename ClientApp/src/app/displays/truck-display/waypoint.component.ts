import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

@Component({
  selector: 'app-waypoint',
  template: '<div class="data-item">{{description()}}</div>',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class WaypointComponent {
  public city = input<string>();
  public company = input<string>();
  protected description = computed(() => {
    if (this.city() && this.company()) {
      return `${this.city()} (${this.company()})`;
    } else if(this.city()) {
      return this.city();
    }

    return '-';
  });
}
