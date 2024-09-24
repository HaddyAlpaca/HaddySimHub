import { Component, ViewEncapsulation, input } from '@angular/core';

@Component({
  selector: 'app-data-group',
  templateUrl: './data-group.component.html',
  standalone: true,
  encapsulation: ViewEncapsulation.None,
})
export class DataGroupComponent {
  public title = input<string>();
  public borderColor = input<string>('white');
}
