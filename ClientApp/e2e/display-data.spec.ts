import { expect, test, type Page } from '@playwright/test';
import { DisplayType } from '../src/app/sse.service';

const publishUpdate = async (page: Page, type: DisplayType, data: object): Promise<void> => {
  const response = await page.request.post('http://127.0.0.1:3333/__e2e/display-update', {
    data: { type, data },
  });
  expect(response.ok()).toBe(true);
};

test.beforeEach(async ({ page }) => {
  await page.goto('/');
});

test('loads global dashboard styles', async ({ page }) => {
  await expect(page.locator('body')).toHaveCSS('background-color', 'rgb(30, 30, 47)');
});

test('rejects invalid display updates', async ({ page }) => {
  const invalidType = await page.request.post('http://127.0.0.1:3333/__e2e/display-update', {
    data: { type: 99, data: {} },
  });
  const invalidData = await page.request.post('http://127.0.0.1:3333/__e2e/display-update', {
    data: { type: DisplayType.RaceDashboard, data: 'not-an-object' },
  });

  expect(invalidType.status()).toBe(400);
  expect(invalidData.status()).toBe(400);
});

test('renders backend-provided race data', async ({ page }) => {
  await publishUpdate(page, DisplayType.RaceDashboard, {
    sessionType: 'E2E Grand Prix',
    isLimitedTime: false,
    isLimitedSessionLaps: false,
    currentLap: 4,
    totalLaps: 12,
    sessionTimeRemaining: 300,
    position: 7,
    speed: 155,
    gear: '5',
    rpm: 6200,
    rpmMax: 8000,
    trackTemp: 38,
    airTemp: 27.4,
    fuelEstLaps: 5,
    currentLapTime: 0,
    lastLapTime: 94.421,
    pitLimiterOn: false,
    brakePct: 10,
    throttlePct: 80,
    steeringPct: 50,
  });

  await expect(page.locator('haddy-race-display #position')).toHaveText('P7');
  await expect(page.locator('haddy-race-display #air-temp')).toHaveText('27.4°');
});

test('renders backend-provided rally data', async ({ page }) => {
  await publishUpdate(page, DisplayType.RallyDashboard, {
    speed: 120,
    gear: '4',
    rpm: 4500,
    rpmMax: 6000,
    distanceTravelled: 5432,
    completedPct: 63,
    sector1Time: 45,
    sector2Time: 48,
    lapTime: 93,
    position: 2,
  });

  await expect(page.locator('haddy-rally-display .completion-percentage')).toHaveText('63%');
  await expect(page.locator('haddy-rally-display .distance-info .value')).toHaveText('5432 m');
});

test('renders backend-provided truck data', async ({ page }) => {
  await publishUpdate(page, DisplayType.TruckDashboard, {
    sourceCity: 'E2E Depot',
    sourceCompany: 'Dispatch',
    destinationCity: 'Test City',
    destinationCompany: 'Warehouse',
    timeRemaining: 90,
    distanceRemaining: 120,
    fuelDistance: 800,
    fuelAmount: 500,
    fuelCapacity: 1000,
    jobTimeRemaining: 600,
    jobIncome: 32145,
    jobCargoName: 'Test cargo',
    jobCargoMass: 2500,
    damageTruckCabin: 0,
    damageTruckWheels: 0,
    damageTruckTransmission: 0,
    damageTruckEngine: 0,
    damageTruckChassis: 0,
    numberOfTrailersAttached: 0,
    speed: 80,
    speedLimit: 90,
    rpm: 1500,
    rpmMax: 2500,
    gear: '7',
    recommendedGear: '9',
    batteryVoltage: 24,
    truckName: 'E2E Truck',
    gameTime: 780,
    oilPressure: 30,
    waterTemp: 90,
    brakeAirPressure: 8,
    engineOn: true,
    dashboardBacklight: 1,
  });

  await expect(page.locator('haddy-truck-display #truckName')).toHaveText('E2E Truck');
  await expect(page.locator('haddy-truck-display #jobIncome')).toContainText('32.145');
});

test('renders backend-provided flight data', async ({ page }) => {
  await publishUpdate(page, DisplayType.FlightDashboard, {
    indicatedAirspeed: 268,
    trueAirspeed: 318,
    groundSpeed: 312,
    machNumber: 0.62,
    stallWarning: false,
    overspeedWarning: false,
    pitchDegrees: 3,
    bankDegrees: 12,
    slipBall: 0.1,
    turnRate: 1.2,
    indicatedAltitude: 24000,
    altitudeAboveGround: 23860,
    verticalSpeed: 720,
    altimeterSettingHpa: 1013,
    onGround: false,
    headingMagnetic: 94,
    headingTrue: 96,
    groundTrack: 97,
    windDirection: 270,
    windSpeed: 34,
    autopilotMaster: false,
    headingHold: false,
    headingBug: 106,
    altitudeHold: false,
    altitudeTarget: 24000,
    speedHold: false,
    speedTarget: 270,
    verticalSpeedTarget: 0,
    navHold: false,
    approachHold: false,
    hasActiveFlightPlan: false,
    deviationSource: 0,
    toFrom: 0,
    engineCount: 2,
    engineType: 1,
    fuelQuantityLbs: 4320,
    fuelCapacityLbs: 6000,
    flapsHandleIndex: 0,
    flapsHandlePositions: 5,
    gearPercentExtended: 0,
    gearHandleDown: false,
    spoilersPct: 0,
    spoilersArmed: false,
    parkingBrakeOn: false,
    elevatorTrimPct: 4,
    landingLightsOn: false,
    taxiLightsOn: false,
    strobeLightsOn: true,
    navLightsOn: true,
    beaconOn: true,
    aircraftTitle: 'E2E Citation',
    simTimeUtcSeconds: 43200,
  });

  await expect(page.locator('haddy-flight-display .speed-panel .primary')).toContainText('268');
  await expect(page.locator('haddy-flight-display .config-panel .aircraft')).toHaveText('E2E Citation');
});
