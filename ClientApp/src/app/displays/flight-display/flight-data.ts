/**
 * Mirrors HaddySimHub/Models/EngineType.cs, which in turn mirrors the MSFS
 * `ENGINE TYPE` simvar values. Serialized as a number, so order matters.
 */
export enum EngineType {
  Piston,
  Jet,
  None,
  HeloBellTurbine,
  Unsupported,
  Turboprop,
}

/**
 * Mirrors HaddySimHub/Models/FlightData.cs. Kept in sync by hand, like the other
 * display DTOs. Optional fields are the ones the sim only supplies for some
 * aircraft or only during a planned flight -- render nothing rather than a zero.
 */
export interface FlightData {
  // Speed
  indicatedAirspeed: number; // knots
  trueAirspeed: number; // knots
  groundSpeed: number; // knots
  machNumber: number;
  stallWarning: boolean;
  overspeedWarning: boolean;

  // Attitude
  pitchDegrees: number; // positive nose up
  bankDegrees: number; // positive banking right
  slipBall: number; // -1 (full left) to 1 (full right)
  turnRate: number; // degrees per second, positive turning right

  // Altitude
  indicatedAltitude: number; // feet
  altitudeAboveGround: number; // feet
  verticalSpeed: number; // feet per minute
  altimeterSettingHpa: number;
  onGround: boolean;

  // Heading
  headingMagnetic: number; // degrees
  headingTrue: number; // degrees
  groundTrack: number; // degrees
  windDirection: number; // degrees the wind comes from
  windSpeed: number; // knots

  // Autopilot
  autopilotMaster: boolean;
  headingHold: boolean;
  headingBug: number; // degrees
  altitudeHold: boolean;
  altitudeTarget: number; // feet
  speedHold: boolean;
  speedTarget: number; // knots
  verticalSpeedTarget: number; // feet per minute
  navHold: boolean;
  approachHold: boolean;

  // Navigation -- all optional fields below are absent without an active flight plan
  hasActiveFlightPlan: boolean;
  nextWaypointId?: string;
  distanceToWaypointNm?: number;
  waypointEteSeconds?: number;
  destinationId?: string;
  distanceToDestinationNm?: number;
  destinationEteSeconds?: number;
  destinationEtaUtcSeconds?: number; // seconds since midnight UTC
  crossTrackErrorNm?: number; // positive right of course

  // Engine
  engineCount: number;
  engineType: EngineType;
  enginePrimaryPct?: number; // N1 for a turbine, % of max RPM for a piston
  engineRpm?: number; // absent for a jet
  fuelFlowPph?: number;
  oilTemperature?: number; // Celsius
  oilPressure?: number; // psi
  manifoldPressure?: number; // inHg, piston only

  // Fuel
  fuelQuantityLbs: number;
  fuelCapacityLbs: number;
  fuelEnduranceSeconds?: number; // absent when nothing is burning fuel

  // Configuration
  flapsHandleIndex: number; // 0 = clean
  flapsHandlePositions: number; // number of detents including clean
  gearPercentExtended: number; // 0 = up, 100 = down
  gearHandleDown: boolean;
  spoilersPct: number;
  spoilersArmed: boolean;
  parkingBrakeOn: boolean;
  elevatorTrimPct: number; // -100 (nose down) to 100 (nose up)

  // Lights
  landingLightsOn: boolean;
  taxiLightsOn: boolean;
  strobeLightsOn: boolean;
  navLightsOn: boolean;
  beaconOn: boolean;

  // Miscellaneous
  aircraftTitle: string;
  simTimeUtcSeconds: number;
}
