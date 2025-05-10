using System.Data.Common;
using HaddySimHub.Models;
using HaddySimHub.Shared;
using SCSSdkClient;
using SCSSdkClient.Object;

namespace HaddySimHub.Displays.ETS;

internal sealed class Display() : DisplayBase<SCSTelemetry>()
{
    private SCSSdkTelemetry? telemetry;
    private float _fuelAverageConsumption = 0f;

    public override void Start()
    {
        telemetry = new();
        telemetry.Data += async (data, newTimestamp) =>
        {
            await this.SendUpdate(data);
        };
    }

    public override void Stop()
    {
        telemetry?.Dispose();
        telemetry = null;
    }

    public override string Description => "Euro Truck Simulator 2";

    public override bool IsActive => ProcessHelper.IsProcessRunning("eurotrucks2");

    protected override DisplayUpdate ConvertToDisplayUpdate(SCSTelemetry data)
    {
        float fuelAverageConsumption = (float)Math.Round(data.TruckValues.CurrentValues.DashboardValues.FuelValue.AverageConsumption * 100, 1);
        if (fuelAverageConsumption > 0)
        {
            _fuelAverageConsumption = fuelAverageConsumption;
        }

        Console.Clear();
        Console.WriteLine($"HShifterSlot: {data.TruckValues.CurrentValues.MotorValues.GearValues.HShifterSlot}");
        Console.WriteLine($"Selected: {data.TruckValues.CurrentValues.MotorValues.GearValues.Selected}");
        Console.WriteLine($"SlotGear: {data.TruckValues.ConstantsValues.MotorValues.SlotGear}");
        Console.WriteLine($"ShifterTypeValue: {data.TruckValues.ConstantsValues.MotorValues.ShifterTypeValue}");
        Console.WriteLine($"EngineRpmMax: {data.TruckValues.ConstantsValues.MotorValues.EngineRpmMax}");
        Console.WriteLine($"ForwardGearCount: {data.TruckValues.ConstantsValues.MotorValues.ForwardGearCount}");
        Console.WriteLine($"ReverseGearCount: {data.TruckValues.ConstantsValues.MotorValues.ReverseGearCount}");
        Console.WriteLine($"SelectorCount: {data.TruckValues.ConstantsValues.MotorValues.SelectorCount}");
        Console.WriteLine($"SlotHandlePosition: {data.TruckValues.ConstantsValues.MotorValues.SlotHandlePosition}");

        string gear = string.Empty;
        int selectedGear = data.TruckValues.CurrentValues.MotorValues.GearValues.Selected;
        if (selectedGear == 0)
        {
            gear = "N";
        }
        else if (selectedGear < 0)
        {
            gear = "R" + Math.Abs(selectedGear).ToString();
        }
        else if (selectedGear > 0)
        {
            if (data.TruckValues.ConstantsValues.MotorValues.ForwardGearCount == 14)
            {
                gear = selectedGear == 1 ? "C1" : (selectedGear - 2).ToString();
            }
            else
            {
                gear = selectedGear.ToString();
            }
        }

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
            Gear = gear,
            Rpm = (int)data.TruckValues.CurrentValues.DashboardValues.RPM,
            RpmMax = (int)data.TruckValues.ConstantsValues.MotorValues.EngineRpmMax,
            Speed = (short)Math.Max(data.TruckValues.CurrentValues.DashboardValues.Speed.Kph, 0),
            SpeedLimit = (short)Math.Max(data.NavigationValues.SpeedLimit.Kph, 0),
            CruiseControlOn = data.TruckValues.CurrentValues.DashboardValues.CruiseControl,
            CruiseControlSpeed = (short)data.TruckValues.CurrentValues.DashboardValues.CruiseControlSpeed.Kph,
            ParkingLightsOn = data.TruckValues.CurrentValues.LightsValues.Parking,
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
            FuelAverageConsumption = _fuelAverageConsumption,
            Throttle = Convert.ToInt32(Math.Round(data.ControlValues.GameValues.Throttle * 100)),
            DifferentialLock = data.TruckValues.CurrentValues.DifferentialLock,
            OilPressure = data.TruckValues.CurrentValues.DashboardValues.OilPressure,
            OilPressureWarningOn = data.TruckValues.CurrentValues.DashboardValues.WarningValues.OilPressure,
            OilTemp = data.TruckValues.CurrentValues.DashboardValues.OilTemperature,
            WaterTemp = data.TruckValues.CurrentValues.DashboardValues.WaterTemperature,
            WaterTempWarningOn = data.TruckValues.CurrentValues.DashboardValues.WarningValues.WaterTemperature,
            BatteryVoltageWarningOn = data.TruckValues.CurrentValues.DashboardValues.WarningValues.BatteryVoltage,
            BatteryVoltage = data.TruckValues.CurrentValues.DashboardValues.BatteryVoltage,
            RetarderLevel = data.TruckValues.CurrentValues.MotorValues.BrakeValues.RetarderLevel,
            RetarderStepCount = data.TruckValues.ConstantsValues.MotorValues.RetarderStepCount,
        };

        return new DisplayUpdate { Type = DisplayType.TruckDashboard, Data = displayData };
    }
}
