import { Component, computed, input } from '@angular/core';

@Component({
  selector: 'app-waypoint',
  template: '<div class="data-item">{{description()}}</div>',
  standalone: true,
})
export class WaypointComponent {
  public city = input<string>();
  public company = input<string>();
  public description = computed(() => {
    if (this.city() && this.company()) {
      return `${this.city()} (${this.company()})`;
    } else if(this.city()) {
      return this.city();
    }

    return '-';
  });
}
