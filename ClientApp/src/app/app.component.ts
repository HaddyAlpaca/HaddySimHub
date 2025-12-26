import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { ClockComponent, ConnectionStatusComponent } from './shared';
import { RaceDisplayComponent, RallyDisplayComponent, TruckDisplayComponent } from './displays';
import { DisplayType } from './signalr.service';
import { APP_STORE } from './state/app.store';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
  imports: [
    TruckDisplayComponent,
    RaceDisplayComponent,
    RallyDisplayComponent,
    ConnectionStatusComponent,
    ClockComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppComponent {
  private readonly _store = inject(APP_STORE);
  protected readonly type = this._store.displayType;
  public readonly DisplayType = DisplayType;
}
