export interface RaceData {
  sessionType: string;
  IsLimitedTime: boolean;
  isLimitedSessionLaps: boolean;
  currentLap: number;
  totalLaps: number;
  sessionTimeRemaining: number;
  position: number;
  speed: number;
  gear: string;
  rpm: number;
  rpmLights: { rpm: number; color: string }[];
  rpmMax: number;
  trackTemp: number;
  airTemp: number;
  fuelRemaining: number;
  brakeBias: number;
  strengthOfField: number;
  lastSectorNum: number;
  lastSectorTime: number;
  lastLapTime: number;
  lastLapTimeDelta: number;
  bestLapTime: number;
  bestLapTimeDelta: number;
  pitLimiterOn: boolean;
  incidents: number;
  maxIncidents: number;
  flag: '' | 'yellow' | 'green' | 'blue' | 'white' | 'finish' | 'black' | 'black-orange' | 'red';
  timingEntries: TimingEntry[];
  brakePct: number;
  throttlePct: number;
}

export interface TimingEntry {
  position: number;
  driverName: string;
  carNumber: number;
  license: string;
  licenseColor: string;
  iRating: number;
  laps: number;
  lapCompletedPct: number;
  isPlayer: boolean;
  isSafetyCar: boolean;
  isInPits: boolean;
  timeToPlayer: number;
  isLapAhead: boolean;
  isLapBehind: boolean;
  totalPosition: number;
}
