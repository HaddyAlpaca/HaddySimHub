import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { DashboardPageComponent } from './dashboard-page.component';
import { RelativePageComponent } from './relative-page.component';
import { SignalRService } from 'src/app/signalr.service';

@Component({
  selector: 'app-race-display',
  templateUrl: './race-display.component.html',
  imports: [
    DashboardPageComponent,
    RelativePageComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RaceDisplayComponent {
  private readonly _signalRService = inject(SignalRService);
  protected readonly page = computed(() => this._signalRService.displayData()?.page ?? 1);
}
