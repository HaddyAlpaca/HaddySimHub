using HaddySimHub.Models;
using SCSSdkClient;
using SCSSdkClient.Object;
using static SCSSdkClient.Object.SCSTelemetry;

namespace HaddySimHub.Displays;

internal sealed class Ets2DashboardDisplay(Func<DisplayUpdate, Task> updateDisplay) : DisplayBase<SCSTelemetry>(updateDisplay)
{
    private IEnumerable<Notification> _notifications = [];
    private SCSSdkTelemetry? telemetry;
    private SpecialEvents _specialEventsValues = new();

    public override void Start() {
        this.telemetry = new ();
        this.telemetry.Data += (SCSTelemetry data, bool newTimestamp) =>
        {
            if (this._specialEventsValues.Fined != data.SpecialEventsValues.Fined)
            {
                string description = data.GamePlay.FinedEvent.Offence switch
                {
                    Offence.Speeding => "Snelheidsovertreding",
                    Offence.Speeding_camera => "Snelheidscamera",
                    Offence.Avoid_sleeping => "Rusttijdenovertreding",
                    Offence.Crash => "Aanrijding",
                    Offence.Red_signal => "Roodlichtovertreding",
                    Offence.Hard_Shoulder_Violation => "Rijden over de vluchtstrook",
                    _ => data.GamePlay.FinedEvent.Offence.ToString(),
                };
                string message = $"Boete: {description} {data.GamePlay.FinedEvent.Amount}";
                this.AddNotification(message);
            }

            if (this._specialEventsValues.Tollgate != data.SpecialEventsValues.Tollgate)
            {
                string message = data.GamePlay.TollgateEvent.PayAmount == 0
                    ? "Begin van tolweg."
                    : $"Tol betaald: {data.GamePlay.TollgateEvent.PayAmount}";
                this.AddNotification(message);
            }

            this._specialEventsValues = data.SpecialEventsValues;
            this._updateDisplay(this.ConvertToDisplayUpdate(data));
        };
    }

    public override void Stop()
    {
        this.telemetry?.Dispose();
        this.telemetry = null;
    }

    public override string Description => "Euro Truck Simulator 2";

    public override bool IsActive => Functions.IsProcessRunning("eurotrucks2");

    protected override DisplayUpdate ConvertToDisplayUpdate(SCSTelemetry data)
    {
        var openNotifications = this._notifications.Where(n => n.Ttl >= DateTime.Now);
        this._notifications = openNotifications;

        var displayData = new TruckData()
        {
            // Navigation info
            SourceCity = data.JobValues.CitySource,
            SourceCompany = data.JobValues.CompanySource,
            DestinationCity = data.JobValues.CityDestination,
            DestinationCompany = data.JobValues.CompanyDestination,
            DistanceRemaining = (int)Math.Round(Math.Max(data.NavigationValues.NavigationDistance, 0) / 1000),
            TimeRemaining = (int)Math.Round(Math.Max(data.NavigationValues.NavigationTime, 0) / 60),
            TimeRemainingIrl = (int)Math.Round(Math.Max(data.NavigationValues.NavigationTime, 0) / 60 / data.CommonValues.Scale),
            RestTimeRemaining = Math.Max(data.CommonValues.NextRestStop.Value, 0),
            RestTimeRemainingIrl = (int)Math.Round(Math.Max(data.CommonValues.NextRestStop.Value, 0) / data.CommonValues.Scale),

            // Job info
            JobTimeRemaining = Math.Max(data.JobValues.RemainingDeliveryTime.Value, 0),
            JobTimeRemainingIrl = (long)Math.Round(Math.Max(data.JobValues.RemainingDeliveryTime.Value, 0) / data.CommonValues.Scale),
            JobIncome = data.JobValues.Income,
            JobCargoName = data.JobValues.CargoValues.Name,
            JobCargoMass = (int)Math.Ceiling(data.JobValues.CargoValues.Mass),
            JobCargoDamage = (int)Math.Round(data.JobValues.CargoValues.CargoDamage * 100),

            // Damage
            DamageTruckCabin = (int)Math.Round(data.TruckValues.CurrentValues.DamageValues.Cabin * 100),
            DamageTruckWheels = (int)Math.Round(data.TruckValues.CurrentValues.DamageValues.WheelsAvg * 100),
            DamageTruckTransmission = (int)Math.Round(data.TruckValues.CurrentValues.DamageValues.Transmission * 100),
            DamageTruckEngine = (int)Math.Round(data.TruckValues.CurrentValues.DamageValues.Engine * 100),
            DamageTruckChassis = (int)Math.Round(data.TruckValues.CurrentValues.DamageValues.Chassis * 100),
            DamageTrailerChassis = (int)Math.Round(data.TrailerValues.Average(t => t.DamageValues.Chassis) * 100),
            DamageTrailerCargo = (int)Math.Round(data.TrailerValues.Average(t => t.DamageValues.Cargo) * 100),
            DamageTrailerWheels = (int)Math.Round(data.TrailerValues.Average(t => t.DamageValues.Wheels) * 100),
            DamageTrailerBody = (int)Math.Round(data.TrailerValues.Average(t => t.DamageValues.Body) * 100),
            NumberOfTrailersAttached = data.TrailerValues.Length,

            // Dashboard
            Gear = (short)data.TruckValues.CurrentValues.DashboardValues.GearDashboards,
            Rpm = (int)data.TruckValues.CurrentValues.DashboardValues.RPM,
            RpmMax = (int)data.TruckValues.ConstantsValues.MotorValues.EngineRpmMax,
            Speed = (short)Math.Max(data.TruckValues.CurrentValues.DashboardValues.Speed.Kph, 0),
            SpeedLimit = (short)Math.Max(data.NavigationValues.SpeedLimit.Kph, 0),
            CruiseControlOn = data.TruckValues.CurrentValues.DashboardValues.CruiseControl,
            CruiseControlSpeed = (short)data.TruckValues.CurrentValues.DashboardValues.CruiseControlSpeed.Kph,
            LowBeamOn = data.TruckValues.CurrentValues.LightsValues.BeamLow,
            HighBeamOn = data.TruckValues.CurrentValues.LightsValues.BeamHigh,
            ParkingBrakeOn = data.TruckValues.CurrentValues.MotorValues.BrakeValues.ParkingBrake,
            HazardLightsOn = data.TruckValues.CurrentValues.LightsValues.HazardWarningLights,
            FuelDistance = data.TruckValues.CurrentValues.DashboardValues.FuelValue.Range,
            FuelAmount = data.TruckValues.CurrentValues.DashboardValues.FuelValue.Amount,
            FuelWarningOn = data.TruckValues.CurrentValues.DashboardValues.WarningValues.FuelW,
            AdBlueAmount = data.TruckValues.CurrentValues.DashboardValues.AdBlue,
            AdBlueWarningOn = data.TruckValues.CurrentValues.DashboardValues.WarningValues.AdBlue,
            TruckName = $"{data.TruckValues.ConstantsValues.Brand} {data.TruckValues.ConstantsValues.Name}",
            BlinkerLeftOn = data.TruckValues.CurrentValues.LightsValues.BlinkerLeftOn,
            BlinkerRightOn = data.TruckValues.CurrentValues.LightsValues.BlinkerRightOn,
            WipersOn = data.TruckValues.CurrentValues.DashboardValues.Wipers,
            GameTime = data.CommonValues.GameTime.Value,
            FuelAverageConsumption = data.TruckValues.CurrentValues.DashboardValues.FuelValue.AverageConsumption * 100,
            Throttle = Convert.ToInt32(Math.Round(data.ControlValues.GameValues.Throttle * 100)),
            DifferentialLock = data.TruckValues.CurrentValues.DifferentialLock,
            OilPressure = data.TruckValues.CurrentValues.DashboardValues.OilPressure,
            OilPressureWarningOn = data.TruckValues.CurrentValues.DashboardValues.WarningValues.OilPressure,
            OilTemp = data.TruckValues.CurrentValues.DashboardValues.OilTemperature,
            WaterTemp = data.TruckValues.CurrentValues.DashboardValues.WaterTemperature,
            WaterTempWarningOn = data.TruckValues.CurrentValues.DashboardValues.WarningValues.WaterTemperature,
            BatteryVoltageWarningOn = data.TruckValues.CurrentValues.DashboardValues.WarningValues.BatteryVoltage,
            BatteryVoltage = data.TruckValues.CurrentValues.DashboardValues.BatteryVoltage,
            Messages = openNotifications.Select(n => n.Message).ToArray(),
        };

        return new DisplayUpdate { Type = DisplayType.TruckDashboard, Data = displayData };
    }

    private void AddNotification(string message)
    {
        this._notifications = [..this._notifications, new Notification { Message = message }];
    }

    private record Notification
    {
        public string Message { get; init; } = string.Empty;
        public DateTime Ttl { get; init; } = DateTime.Now.AddSeconds(10);
    }
}