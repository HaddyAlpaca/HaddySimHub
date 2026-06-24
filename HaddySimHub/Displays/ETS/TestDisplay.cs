using HaddySimHub.Models;
using HaddySimHub.Interfaces;
using HaddySimHub.Services;

namespace HaddySimHub.Displays.ETS
{
    public class TestDisplay : TestDisplayBase
    {
        private const int RpmMax = 3000;
        private const int RpmStep = 100;
        private int _currentRpm = 0;
        private int _cruiseTick = 0;
        private bool _cruiseOn = false;

        public TestDisplay(
            string id,
            IDataConverter<DisplayUpdate, DisplayUpdate> identityDataConverter,
            IDisplayUpdateSender displayUpdateSender)
            : base(id, identityDataConverter, displayUpdateSender)
        {
        }

        protected override DisplayUpdate GenerateDisplayUpdate()
        {
            _currentRpm += RpmStep;
            if (_currentRpm > RpmMax)
            {
                _currentRpm = 0;
            }

            var speed = (short)(82 + Math.Sin(_currentRpm * 2 * Math.PI / RpmMax) * 5);
            var recommendedGear = _currentRpm switch
            {
                > 2000 => "6",
                < 1000 => "4",
                _ => string.Empty,
            };

            var backlight = (float)(0.65 + 0.35 * Math.Sin(_currentRpm * 2 * Math.PI / RpmMax));

            _cruiseTick++;
            if (_cruiseTick >= 30)
            {
                _cruiseTick = 0;
                _cruiseOn = !_cruiseOn;
            }

            return new DisplayUpdate
            {
                Type = DisplayType.TruckDashboard,
                Data = new TruckData
                {
                    // Route card
                    SourceCity = "Berlin",
                    SourceCompany = "Trisan",
                    DestinationCity = "München",
                    DestinationCompany = "LKW Logistics",
                    DistanceRemaining = 587,
                    TimeRemaining = 420,
                    TimeRemainingIrl = 42,
                    RestTimeRemaining = 360,
                    RestTimeRemainingIrl = 36,
                    GameTime = 13 * 60 + 45,

                    // Job card
                    TruckName = "Volvo FH16",
                    JobTimeRemaining = 720,
                    JobTimeRemainingIrl = 72,
                    JobIncome = 18750,
                    JobCargoName = "Helicopter",
                    JobCargoMass = 12500,
                    JobCargoDamage = 3,

                    // Dashboard
                    Speed = speed,
                    Gear = "5",
                    RecommendedGear = recommendedGear,
                    Rpm = (short)_currentRpm,
                    RpmMax = RpmMax,
                    FuelAverageConsumption = 25.5F,
                    FuelAmount = 300,
                    FuelCapacity = 800,
                    FuelDistance = (int)(300 / 25.5 * 100),
                    AdBlueAmount = 60,
                    AdBlueCapacity = 80,
                    SpeedLimit = 80,
                    CruiseControlOn = _cruiseOn,
                    CruiseControlSpeed = 82,
                    EngineOn = true,
                    Throttle = 45,
                    DifferentialLock = false,

                    // Truck damage
                    DamageTruckEngine = 2,
                    DamageTruckTransmission = 8,
                    DamageTruckCabin = 0,
                    DamageTruckChassis = 12,
                    DamageTruckWheels = 5,

                    // Trailer
                    NumberOfTrailersAttached = 1,
                    DamageTrailerChassis = 4,
                    DamageTrailerBody = 1,
                    DamageTrailerWheels = 7,
                    DamageTrailerCargo = 3,

                    // Gauges & controls
                    BrakeTemp = 120,
                    BrakeAirPressure = 120,
                    OilPressure = 45.2f,
                    OilTemp = 92.5f,
                    WaterTemp = 87.1f,
                    BatteryVoltage = 24.6f,
                    RetarderLevel = 0,
                    RetarderStepCount = 4,

                    // Lights
                    ParkingLightsOn = true,
                    LowBeamOn = true,
                    HighBeamOn = false,
                    BlinkerLeftOn = true,
                    BlinkerRightOn = false,
                    HazardLightsOn = false,
                    ParkingBrakeOn = false,
                    WipersOn = false,
                    MotorBrakeOn = false,
                    BeaconOn = false,
                    LiftAxleIndicatorOn = false,
                    FuelWarningOn = false,
                    AdBlueWarningOn = false,
                    AirPressureWarningOn = false,
                    AirPressureEmergencyOn = false,
                    BatteryVoltageWarningOn = false,
                    WaterTempWarningOn = false,
                    OilPressureWarningOn = false,

                    // Extra
                    Odometer = 142537,
                    DashboardBacklight = backlight,
                }
            };
        }
    }
}
