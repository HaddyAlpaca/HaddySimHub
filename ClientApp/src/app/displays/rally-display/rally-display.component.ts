import { ChangeDetectionStrategy, Component, inject, ViewEncapsulation } from '@angular/core';
import { SpeedometerComponent, LapTimePipe } from '../../shared';
import { APP_STORE } from '../../state/app.store';

export type { RallyData } from './rally-data';

@Component({
  selector: 'app-rally-display',
  styleUrl: './rally-display.component.scss',
  templateUrl: './rally-display.component.html',
  encapsulation: ViewEncapsulation.None,
  imports: [
    SpeedometerComponent,
    LapTimePipe,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RallyDisplayComponent {
  private readonly _store = inject(APP_STORE);
  protected readonly data = this._store.rallyData;
}
