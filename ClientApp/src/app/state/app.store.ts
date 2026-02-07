import { computed } from '@angular/core';
import { patchState, signalStore, withComputed, withMethods, withState } from '@ngrx/signals';
import { DisplayUpdate, DisplayType } from '../signalr.service';
import { RaceData, RallyData, TruckData } from '../displays';

export const APP_STORE = signalStore(
  { providedIn: 'root' },
  withState({
    displayUpdate: {} as DisplayUpdate,
  }),
  withMethods((store) => ({
    updateDisplay: (displayUpdate: DisplayUpdate): void => {
      patchState(store, { displayUpdate });
    },
  })),
  withComputed(({ displayUpdate }) => ({
    displayData: computed(() => displayUpdate().data),
    displayType: computed(() => displayUpdate().type ?? DisplayType.None),
    truckData: computed(() => (displayUpdate().data ?? { }) as TruckData),
    raceData: computed(() => (displayUpdate().data ?? { }) as RaceData),
    rallyData: computed(() => (displayUpdate().data ?? { }) as RallyData),
  })),
);
