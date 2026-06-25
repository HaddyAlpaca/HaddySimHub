export interface RaceData {
  // Universal mandatory fields
  sessionType: string;
  isLimitedTime: boolean;
  isLimitedSessionLaps: boolean;
  currentLap: number;
  totalLaps: number;
  sessionTimeRemaining: number;
  position?: number; // not exposed by all sims (e.g. Assetto Corsa)
  speed: number;
  gear: string;
  rpm: number;
  rpmMax: number;
  trackTemp: number;
  airTemp: number;
  fuelRemaining?: number; // not exposed by all sims
  fuelAvgLap?: number; // not exposed by all sims
  fuelLastLap?: number; // not exposed by all sims
  fuelEstLaps: number;
  currentLapTime: number;
  lastLapTime: number;
  lastLapTimeDelta?: number; // not exposed by all sims
  bestLapTime?: number; // not exposed by all sims
  bestLapTimeDelta?: number; // not exposed by all sims
  pitLimiterOn: boolean;
  brakePct: number;
  throttlePct: number;
  steeringPct: number;

  // Optional sim-specific fields
  expectedPosition?: string; // iRacing only
  brakeBias?: number; // iRacing, ACC, etc.
  strengthOfField?: number; // iRacing only
  incidents?: number; // iRacing only
  maxIncidents?: number; // iRacing only
  irating?: number; // iRacing only
  safetyRating?: number; // iRacing only
}
