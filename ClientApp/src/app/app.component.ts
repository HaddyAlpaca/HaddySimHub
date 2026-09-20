import { Component, CUSTOM_ELEMENTS_SCHEMA, inject } from '@angular/core';
import { ConnectionStatusComponent } from './shared';
import { FlightDisplayComponent, RaceDisplayComponent, TruckDisplayComponent } from './displays';
import { DisplayType } from './sse.service';
import { APP_STORE } from './state/app.store';
import './shared/clock/clock.element';
import './displays/rally-display/rally-display.element';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  imports: [
    TruckDisplayComponent,
    RaceDisplayComponent,
    FlightDisplayComponent,
    ConnectionStatusComponent,
  ],
})
export class AppComponent {
  private readonly _store = inject(APP_STORE);
  protected readonly type = this._store.displayType;
  protected readonly rallyData = this._store.rallyData;
  public readonly DisplayType = DisplayType;
}
