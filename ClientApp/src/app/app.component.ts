import { Component, CUSTOM_ELEMENTS_SCHEMA, inject } from '@angular/core';
import { ConnectionStatusComponent } from './shared';
import { DisplayType } from './sse.service';
import { APP_STORE } from './state/app.store';
import './shared/clock/clock.element';
import './displays/rally-display/rally-display.element';
import './displays/race-display/race-display.element';
import './displays/truck-display/truck-display.element';
import './displays/flight-display/flight-display.element';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  imports: [
    ConnectionStatusComponent,
  ],
})
export class AppComponent {
  private readonly _store = inject(APP_STORE);
  protected readonly type = this._store.displayType;
  protected readonly rallyData = this._store.rallyData;
  protected readonly raceData = this._store.raceData;
  protected readonly truckData = this._store.truckData;
  protected readonly flightData = this._store.flightData;
  public readonly DisplayType = DisplayType;
}
