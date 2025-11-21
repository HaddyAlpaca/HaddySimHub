using SCSSdkClient.Object;
using SCSSdkClient;

namespace HaddySimHub.Tests
{
    public class SCSTelemetryBuilder
    {
        private readonly SCSTelemetry _t = new SCSTelemetry();

        public SCSTelemetryBuilder WithScale(float scale)
        {
            _t.CommonValues.Scale = scale;
            return this;
        }

        public SCSTelemetryBuilder WithGameTime(ulong gameTime)
        {
            _t.CommonValues.GameTime.Value = (uint)gameTime;
            return this;
        }

        public SCSTelemetryBuilder WithNextRestStop(int minutes)
        {
            _t.CommonValues.NextRestStop.Value = minutes;
            return this;
        }

        public SCSTelemetryBuilder WithNavigation(double distanceMeters, double timeSeconds, double speedLimitMs = 0)
        {
            _t.NavigationValues.NavigationDistance = (float)distanceMeters;
            _t.NavigationValues.NavigationTime = (float)timeSeconds;
            _t.NavigationValues.SpeedLimit.Value = (float)speedLimitMs;
            return this;
        }

        public SCSTelemetryBuilder WithJob(string sourceCity, string sourceCompany, string destCity, string destCompany, ulong income, double cargoMass, float cargoDamage)
        {
            _t.JobValues.CitySource = sourceCity;
            _t.JobValues.CompanySource = sourceCompany;
            _t.JobValues.CityDestination = destCity;
            _t.JobValues.CompanyDestination = destCompany;
            _t.JobValues.Income = income;
            _t.JobValues.CargoValues.Mass = (float)cargoMass;
            _t.JobValues.CargoValues.CargoDamage = cargoDamage;
            return this;
        }

        public SCSTelemetryBuilder WithTruckConstants(string brand = "Volvo", string name = "FH16", int forwardGearCount = 12, int engineRpmMax = 2800, uint retarderStepCount = 0)
        {
            _t.TruckValues.ConstantsValues.Brand = brand;
            _t.TruckValues.ConstantsValues.Name = name;
            _t.TruckValues.ConstantsValues.MotorValues.ForwardGearCount = (uint)forwardGearCount;
            _t.TruckValues.ConstantsValues.MotorValues.EngineRpmMax = (uint)engineRpmMax;
            _t.TruckValues.ConstantsValues.MotorValues.RetarderStepCount = retarderStepCount;
            return this;
        }

        public SCSTelemetryBuilder WithDashboard(double speed = 0, bool cruiseControl = false, double cruiseSpeed = 0, float fuelAvg = 0, float fuelAmount = 0, float fuelRange = 0, float adBlue = 0, float oilPressure = 0, float oilTemp = 0, float waterTemp = 0, float batteryVoltage = 0, int rpm = 0)
        {
            _t.TruckValues.CurrentValues.DashboardValues.Speed.Value = (float)speed;
            _t.TruckValues.CurrentValues.DashboardValues.CruiseControl = cruiseControl;
            _t.TruckValues.CurrentValues.DashboardValues.CruiseControlSpeed.Value = (float)cruiseSpeed;
            _t.TruckValues.CurrentValues.DashboardValues.FuelValue.AverageConsumption = fuelAvg;
            _t.TruckValues.CurrentValues.DashboardValues.FuelValue.Amount = fuelAmount;
            _t.TruckValues.CurrentValues.DashboardValues.FuelValue.Range = fuelRange;
            _t.TruckValues.CurrentValues.DashboardValues.AdBlue = adBlue;
            _t.TruckValues.CurrentValues.DashboardValues.OilPressure = oilPressure;
            _t.TruckValues.CurrentValues.DashboardValues.OilTemperature = oilTemp;
            _t.TruckValues.CurrentValues.DashboardValues.WaterTemperature = waterTemp;
            _t.TruckValues.CurrentValues.DashboardValues.BatteryVoltage = batteryVoltage;
            _t.TruckValues.CurrentValues.DashboardValues.RPM = rpm;
            return this;
        }

        public SCSTelemetryBuilder WithWarnings(bool fuel = false, bool adBlue = false, bool oilPressure = false, bool waterTemp = false, bool batteryVoltage = false)
        {
            _t.TruckValues.CurrentValues.DashboardValues.WarningValues.FuelW = fuel;
            _t.TruckValues.CurrentValues.DashboardValues.WarningValues.AdBlue = adBlue;
            _t.TruckValues.CurrentValues.DashboardValues.WarningValues.OilPressure = oilPressure;
            _t.TruckValues.CurrentValues.DashboardValues.WarningValues.WaterTemperature = waterTemp;
            _t.TruckValues.CurrentValues.DashboardValues.WarningValues.BatteryVoltage = batteryVoltage;
            return this;
        }

        public SCSTelemetryBuilder WithLights(bool parking = false, bool lowBeam = false, bool highBeam = false, bool hazard = false, bool blinkerLeft = false, bool blinkerRight = false)
        {
            _t.TruckValues.CurrentValues.LightsValues.Parking = parking;
            _t.TruckValues.CurrentValues.LightsValues.BeamLow = lowBeam;
            _t.TruckValues.CurrentValues.LightsValues.BeamHigh = highBeam;
            _t.TruckValues.CurrentValues.LightsValues.HazardWarningLights = hazard;
            _t.TruckValues.CurrentValues.LightsValues.BlinkerLeftOn = blinkerLeft;
            _t.TruckValues.CurrentValues.LightsValues.BlinkerRightOn = blinkerRight;
            return this;
        }

        public SCSTelemetryBuilder WithMotor(int selectedGear = 0, bool parkingBrake = false, uint retarderLevel = 0)
        {
            _t.TruckValues.CurrentValues.MotorValues.GearValues.Selected = selectedGear;
            _t.TruckValues.CurrentValues.MotorValues.BrakeValues.ParkingBrake = parkingBrake;
            _t.TruckValues.CurrentValues.MotorValues.BrakeValues.RetarderLevel = retarderLevel;
            return this;
        }

        public SCSTelemetryBuilder WithDamage(float cabin = 0, float engine = 0)
        {
            _t.TruckValues.CurrentValues.DamageValues.Cabin = cabin;
            _t.TruckValues.CurrentValues.DamageValues.Engine = engine;
            return this;
        }

        public SCSTelemetryBuilder WithControl(double throttle = 0)
        {
            _t.ControlValues.GameValues.Throttle = (float)throttle;
            return this;
        }

        public SCSTelemetryBuilder WithTrailerEmpty()
        {
            // TrailerValues setup - leave empty or initialize if available
            _t.TrailerValues = [];
            return this;
        }

        public SCSTelemetry Build() => _t;
    }
}
