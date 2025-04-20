export interface RaceData {
    sessionType: string;
    IsLimitedTime: boolean;
    isLimitedSessionLaps: boolean;
    currentLap: number;
    totalLaps: number;
    sessionTimeRemaining: number;
    position: number;
    speed: number;
    gear: number;
    rpm: number;
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
    pageNumber: number;
  }

  export interface TimingEntry {
    position: number;
    driverName: string;
    carNumber: number;
    license: string;
    licenseColor: string;
    iRating: number;
    lapsCompleted: number;
    distancePct: number;
    isPlayer: boolean;
    isSafetyCar: boolean;
    isInPits: boolean;
  }
  