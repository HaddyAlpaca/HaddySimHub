import { Component, inject } from '@angular/core';
import { ClockService } from './clock.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-clock',
  templateUrl: './clock.component.html',
  standalone: true,
  imports: [CommonModule],
})
export class ClockComponent {
  private _clockService = inject(ClockService);

  public currentTime = this._clockService.currentTime;
}
