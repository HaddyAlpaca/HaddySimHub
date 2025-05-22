import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { ClockService } from './clock.service';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-clock',
  templateUrl: './clock.component.html',
  imports: [DatePipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ClockComponent {
  private readonly _clockService = inject(ClockService);

  protected currentTime = this._clockService.currentTime;
}
