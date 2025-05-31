import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { ClockComponent, ConnectionStatusComponent } from './shared';
import { RaceDisplayComponent, RallyDisplayComponent, TruckDisplayComponent } from './displays';
import { DisplayType, SignalRService } from './signalr.service';

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
  private readonly _signalRService = inject(SignalRService);
  protected readonly type = computed(() => this._signalRService.displayData()?.type ?? DisplayType.None);
  public readonly DisplayType = DisplayType;
}
