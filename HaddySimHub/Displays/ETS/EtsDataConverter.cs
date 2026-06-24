using HaddySimHub.Interfaces;
using HaddySimHub.Models;
using SCSSdkClient.Object;

namespace HaddySimHub.Displays.ETS;

public class EtsDataConverter : IDataConverter<SCSTelemetry, DisplayUpdate>
{
    private const float GearAdviceTargetRpm = 1300f;
    private const float GearAdviceMinSpeedKph = 15f;

    private float _fuelAverageConsumption = 0f;

    public DisplayUpdate Convert(SCSTelemetry data)
    {
        float fuelAverageConsumption = (float)Math.Round(data.TruckValues.CurrentValues.DashboardValues.FuelValue.AverageConsumption * 100, 1);
        if (fuelAverageConsumption > 0)
        {
            _fuelAverageConsumption = fuelAverageConsumption;
        }

        uint forwardGearCount = data.TruckValues.ConstantsValues.MotorValues.ForwardGearCount;
        string gear = FormatGear(data.TruckValues.CurrentValues.MotorValues.GearValues.Selected, forwardGearCount);

        int? recommendedGear = RecommendForwardGear(data);
        string recommendedGearText = recommendedGear.HasValue ? FormatGear(recommendedGear.Value, forwardGearCount) : string.Empty;

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
            DamageTrailerChassis = data.TrailerValues.Length > 0 ? (int)Math.Round(data.TrailerValues.Average(t => t.DamageValues.Chassis) * 100) : 0,
            DamageTrailerCargo = data.TrailerValues.Length > 0 ? (int)Math.Round(data.TrailerValues.Average(t => t.DamageValues.Cargo) * 100) : 0,
            DamageTrailerWheels = data.TrailerValues.Length > 0 ? (int)Math.Round(data.TrailerValues.Average(t => t.DamageValues.Wheels) * 100) : 0,
            DamageTrailerBody = data.TrailerValues.Length > 0 ? (int)Math.Round(data.TrailerValues.Average(t => t.DamageValues.Body) * 100) : 0,
            NumberOfTrailersAttached = data.TrailerValues.Length,

            // Dashboard
            Gear = gear,
            RecommendedGear = recommendedGearText,
            Rpm = (int)data.TruckValues.CurrentValues.DashboardValues.RPM,
            RpmMax = (int)data.TruckValues.ConstantsValues.MotorValues.EngineRpmMax,
            Speed = (short)Math.Round(Math.Max(data.TruckValues.CurrentValues.DashboardValues.Speed.Kph, 0)),
            SpeedLimit = (short)Math.Round(Math.Max(data.NavigationValues.SpeedLimit.Kph, 0)),
            CruiseControlOn = data.TruckValues.CurrentValues.DashboardValues.CruiseControl,
            CruiseControlSpeed = (short)Math.Round(data.TruckValues.CurrentValues.DashboardValues.CruiseControlSpeed.Kph),
            ParkingLightsOn = data.TruckValues.CurrentValues.LightsValues.Parking,
            LowBeamOn = data.TruckValues.CurrentValues.LightsValues.BeamLow,
            HighBeamOn = data.TruckValues.CurrentValues.LightsValues.BeamHigh,
            ParkingBrakeOn = data.TruckValues.CurrentValues.MotorValues.BrakeValues.ParkingBrake,
            HazardLightsOn = data.TruckValues.CurrentValues.LightsValues.HazardWarningLights,
            FuelDistance = data.TruckValues.CurrentValues.DashboardValues.FuelValue.Range,
            FuelAmount = data.TruckValues.CurrentValues.DashboardValues.FuelValue.Amount,
            FuelCapacity = data.TruckValues.ConstantsValues.CapacityValues.Fuel,
            FuelWarningOn = data.TruckValues.CurrentValues.DashboardValues.WarningValues.FuelW,
            AdBlueAmount = data.TruckValues.CurrentValues.DashboardValues.AdBlue,
            AdBlueCapacity = data.TruckValues.ConstantsValues.CapacityValues.AdBlue,
            AdBlueWarningOn = data.TruckValues.CurrentValues.DashboardValues.WarningValues.AdBlue,
            TruckName = $"{data.TruckValues.ConstantsValues.Brand} {data.TruckValues.ConstantsValues.Name}",
            BlinkerLeftOn = data.TruckValues.CurrentValues.LightsValues.BlinkerLeftOn,
            BlinkerRightOn = data.TruckValues.CurrentValues.LightsValues.BlinkerRightOn,
            WipersOn = data.TruckValues.CurrentValues.DashboardValues.Wipers,
            GameTime = data.CommonValues.GameTime.Value,
            FuelAverageConsumption = _fuelAverageConsumption,
            Throttle = System.Convert.ToInt32(Math.Round(data.ControlValues.GameValues.Throttle * 100)),
            DifferentialLock = data.TruckValues.CurrentValues.DifferentialLock,
            OilPressure = data.TruckValues.CurrentValues.DashboardValues.OilPressure,
            OilPressureWarningOn = data.TruckValues.CurrentValues.DashboardValues.WarningValues.OilPressure,
            OilTemp = data.TruckValues.CurrentValues.DashboardValues.OilTemperature,
            WaterTemp = data.TruckValues.CurrentValues.DashboardValues.WaterTemperature,
            WaterTempWarningOn = data.TruckValues.CurrentValues.DashboardValues.WarningValues.WaterTemperature,
            BrakeTemp = data.TruckValues.CurrentValues.MotorValues.BrakeValues.Temperature,
            BrakeAirPressure = data.TruckValues.CurrentValues.MotorValues.BrakeValues.AirPressure,
            BatteryVoltageWarningOn = data.TruckValues.CurrentValues.DashboardValues.WarningValues.BatteryVoltage,
            BatteryVoltage = data.TruckValues.CurrentValues.DashboardValues.BatteryVoltage,
            AirPressureWarningOn = data.TruckValues.CurrentValues.DashboardValues.WarningValues.AirPressure,
            AirPressureEmergencyOn = data.TruckValues.CurrentValues.DashboardValues.WarningValues.AirPressureEmergency,
            EngineOn = data.TruckValues.CurrentValues.EngineEnabled,
            MotorBrakeOn = data.TruckValues.CurrentValues.MotorValues.BrakeValues.MotorBrake,
            BeaconOn = data.TruckValues.CurrentValues.LightsValues.Beacon,
            LiftAxleIndicatorOn = data.TruckValues.CurrentValues.LiftAxleIndicator,
            RetarderLevel = data.TruckValues.CurrentValues.MotorValues.BrakeValues.RetarderLevel,
            RetarderStepCount = data.TruckValues.ConstantsValues.MotorValues.RetarderStepCount,
        };

        return new DisplayUpdate { Type = DisplayType.TruckDashboard, Data = displayData };
    }

    private static string FormatGear(int gear, uint forwardGearCount)
    {
        if (gear == 0)
        {
            return "N";
        }

        if (gear < 0)
        {
            return "R" + Math.Abs(gear).ToString();
        }

        if (forwardGearCount == 14)
        {
            return gear == 1 ? "C1" : (gear - 2).ToString();
        }

        return gear.ToString();
    }

    private static int? RecommendForwardGear(SCSTelemetry data)
    {
        var motor = data.TruckValues.ConstantsValues.MotorValues;
        float[] ratios = motor.GearRatiosForward;
        float differential = motor.DifferentialRation;
        float rpmMax = motor.EngineRpmMax;
        float[] radii = data.TruckValues.ConstantsValues.WheelsValues.Radius;

        if (ratios.Length == 0 || differential <= 0 || rpmMax <= 0 || radii.Length == 0)
        {
            return null;
        }

        float wheelRadius = radii.FirstOrDefault(r => r > 0);
        if (wheelRadius <= 0)
        {
            return null;
        }

        float speedMps = Math.Abs(data.TruckValues.CurrentValues.DashboardValues.Speed.Value);
        if (speedMps * 3.6f < GearAdviceMinSpeedKph)
        {
            return null;
        }

        float wheelRevsPerSecond = speedMps / (2f * (float)Math.PI * wheelRadius);

        int bestGear = -1;
        float bestScore = float.MaxValue;
        for (int gear = 1; gear <= ratios.Length; gear++)
        {
            float ratio = ratios[gear - 1];
            if (ratio <= 0)
            {
                continue;
            }

            float predictedRpm = wheelRevsPerSecond * 60f * differential * ratio;
            if (predictedRpm > rpmMax * 0.98f)
            {
                continue;
            }

            float score = Math.Abs(predictedRpm - GearAdviceTargetRpm);
            if (score < bestScore)
            {
                bestScore = score;
                bestGear = gear;
            }
        }

        return bestGear > 0 ? bestGear : null;
    }
}
