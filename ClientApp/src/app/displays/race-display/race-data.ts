export interface RaceData {
  // Universal mandatory fields
  sessionType: string;
  isLimitedTime: boolean;
  isLimitedSessionLaps: boolean;
  currentLap: number;
  totalLaps: number;
  sessionTimeRemaining: number;
  position: number;
  speed: number;
  gear: string;
  rpm: number;
  rpmMax: number;
  trackTemp: number;
  airTemp: number;
  fuelRemaining: number;
  fuelAvgLap: number;
  fuelLastLap: number;
  fuelEstLaps: number;
  lastLapTime: number;
  lastLapTimeDelta: number;
  bestLapTime: number;
  bestLapTimeDelta: number;
  pitLimiterOn: boolean;
  brakePct: number;
  throttlePct: number;
  steeringPct: number;
  carNumber: string;

  // Optional sim-specific fields
  brakeBias?: number; // iRacing, ACC, etc.
  strengthOfField?: number; // iRacing only
  incidents?: number; // iRacing only
  maxIncidents?: number; // iRacing only
  irating?: number; // iRacing only
  safetyRating?: number; // iRacing only
}
