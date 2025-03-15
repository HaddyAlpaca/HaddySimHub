import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GameDataService } from 'src/app/game-data.service';
import { ClockComponent, ConnectionStatusComponent } from './shared';
import { RaceDisplayComponent, RallyDisplayComponent, TruckDisplayComponent } from './displays';

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
