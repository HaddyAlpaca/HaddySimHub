import { Component, CUSTOM_ELEMENTS_SCHEMA, inject } from '@angular/core';
import { ConnectionStatusComponent } from './shared';
import { FlightDisplayComponent, RaceDisplayComponent, RallyDisplayComponent, TruckDisplayComponent } from './displays';
import { DisplayType } from './sse.service';
import { APP_STORE } from './state/app.store';
import './shared/clock/clock.element';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  imports: [
    TruckDisplayComponent,
    RaceDisplayComponent,
    RallyDisplayComponent,
    FlightDisplayComponent,
    ConnectionStatusComponent,
  ],
})
export class AppComponent {
  private readonly _store = inject(APP_STORE);
  protected readonly type = this._store.displayType;
  public readonly DisplayType = DisplayType;
}
