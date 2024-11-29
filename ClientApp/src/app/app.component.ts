import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TruckDisplayComponent } from '@displays/truck-display/truck-display.component';
import { RaceDisplayComponent } from '@displays/race-display/race-display.component';
import { GameDataService } from 'src/app/game-data.service';
import { ConnectionStatusComponent } from '@components/connection-status/connection-status.component';
import { ClockComponent } from '@components/clock/clock.component';
import { RallyDisplayComponent } from '@displays/rally-display/rally-display.component';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
  imports: [
    CommonModule,
    TruckDisplayComponent,
    RaceDisplayComponent,
    RallyDisplayComponent,
    ConnectionStatusComponent,
    ClockComponent,
  ],
})
export class AppComponent {
  public gameDataService = inject(GameDataService);
}
