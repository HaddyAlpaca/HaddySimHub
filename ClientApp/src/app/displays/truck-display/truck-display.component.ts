import { ChangeDetectionStrategy, Component, computed, inject, ViewEncapsulation } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { WaypointComponent } from './waypoint.component';
import { TimespanPipe, NumberNlPipe, NumberFlexDigitPipe } from 'src/app/shared';
import { SignalRService } from 'src/app/signalr.service';
import { GaugeComponent } from 'src/app/shared/gauge/gauge.component';

export interface TruckData {
  sourceCity: string;
  sourceCompany: string;
  destinationCity: string;
  destinationCompany: string;
  timeRemaining: number;
  timeRemainingIrl: number;
  distanceRemaining: number;
  restTimeRemaining: number;
  restTimeRemainingIrl: number;
  fuelDistance: number;
  fuelAmount: number;
  adBlueAmount: number;
  adBlueWarningOn: boolean;
  jobTimeRemaining: number;
  jobTimeRemainingIrl: number;
  jobIncome: number;
  jobCargoName: string;
  jobCargoMass: number;
  jobCargoDamage: number;
  damageTruckCabin: number;
  damageTruckWheels: number;
  damageTruckTransmission: number;
  damageTruckEngine: number;
  damageTruckChassis: number;
  numberOfTrailersAttached: number;
  damageTrailerChassis: number;
  damageTrailerCargo: number;
  damageTrailerBody: number;
  damageTrailerWheels: number;
  damageTrailer: number;
  speed: number;
  speedLimit: number;
  rpm: number;
  rpmMax: number;
  cruiseControlOn: boolean;
  cruiseControlSpeed: number;
  gear: number;
  parkingLightsOn: boolean;
  lowBeamOn: boolean;
  highBeamOn: boolean;
  parkingBrakeOn: boolean;
  batteryVoltageWarningOn: boolean;
  batteryVoltage: number;
  truckName: string;
  hazardLightsOn: boolean;
  fuelWarningOn: boolean;
  blinkerLeftOn: boolean;
  blinkerRightOn: boolean;
  gameTime: Date;
  wipersOn: boolean;
  fuelAverageConsumption: number;
  throttle: number;
  differentialLock: boolean;
  oilPressure: number;
  oilPressureWarningOn: boolean;
  oilTemp: number;
  waterTemp: number;
  waterTempWarningOn: boolean;
  retarderLevel: number;
  retarderStepCount: number;
}

@Component({
  selector: 'app-truck-display',
  templateUrl: 'truck-display.component.html',
  styleUrl: 'truck-display.component.scss',
  encapsulation: ViewEncapsulation.None,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    DecimalPipe,
    TimespanPipe,
    NumberNlPipe,
    WaypointComponent,
    NumberFlexDigitPipe,
    GaugeComponent,
  ],
})
export class TruckDisplayComponent {
  private readonly _signalRService = inject(SignalRService);
  protected readonly data = computed(() => (this._signalRService.displayData()?.data ?? {}) as TruckData);
}
