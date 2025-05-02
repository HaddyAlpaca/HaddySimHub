import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { ClockService } from './clock.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-clock',
  templateUrl: './clock.component.html',
  imports: [CommonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ClockComponent {
  private _clockService = inject(ClockService);

  public currentTime = this._clockService.currentTime;
}
