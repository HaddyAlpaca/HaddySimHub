import { signal } from '@angular/core';
import { RaceData, RallyData, TruckData } from 'src/app/displays';
import { DisplayType } from 'src/app/signalr.service';

export class MockAppStore {
  public displayType = signal(DisplayType.None);
  public truckData = signal<TruckData>({} as TruckData);
  public raceData = signal<RaceData>({} as RaceData);
  public rallyData = signal<RallyData>({} as RallyData);

  public updateDisplay(displayUpdate: { type: DisplayType; data: unknown }): void {
    this.displayType.set(displayUpdate.type);
    switch (displayUpdate.type) {
      case DisplayType.TruckDashboard:
        this.truckData.set(displayUpdate.data as TruckData);
        break;
      case DisplayType.RaceDashboard:
        this.raceData.set(displayUpdate.data as RaceData);
        break;
      case DisplayType.RallyDashboard:
        this.rallyData.set(displayUpdate.data as RallyData);
        break;
    }
  }
}
