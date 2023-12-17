using HaddySimHub.GameData;
using HaddySimHub.GameData.Models;
using SCSSdkClient;

namespace HaddySimHub.Ets2;

public sealed class GameDataReader : IGameDataReader 
{
    private readonly SCSSdkTelemetry _telemetry;

    public event EventHandler<object>? RawDataUpdate;

    public GameDataReader()
    {
        this._telemetry = new SCSSdkTelemetry();
        this._telemetry.Data += (SCSSdkClient.Object.SCSTelemetry data, bool newTimestamp) =>
        {
            this.RawDataUpdate?.Invoke(this, data);
        };
    }

    public object Convert(object rawData)
    {
        if (rawData is not SCSSdkClient.Object.SCSTelemetry typedRawData)
        {
            return new TruckData();
        }

        return new TruckData()
        {
            //Navigation info
            SourceCity = typedRawData.JobValues.CitySource,
            SourceCompany = typedRawData.JobValues.CompanySource,
            DestinationCity = typedRawData.JobValues.CityDestination,
            DestinationCompany = typedRawData.JobValues.CompanyDestination,
            DistanceRemaining = (int)typedRawData.NavigationValues.NavigationDistance,
            TimeRemaining = (int)Math.Ceiling(typedRawData.NavigationValues.NavigationTime / 60),
            TimeRemainingIrl = (int)Math.Ceiling((typedRawData.NavigationValues.NavigationTime / 60) * typedRawData.CommonValues.Scale),
            RestTimeRemaining = typedRawData.CommonValues.NextRestStop.Value,
            RestTimeRemainingIrl = (int)Math.Ceiling(typedRawData.CommonValues.NextRestStop.Value * typedRawData.CommonValues.Scale),
            //Job info
            JobTimeRemaining = typedRawData.JobValues.RemainingDeliveryTime.Value,
            JobTimeRemainingIrl = (long)Math.Ceiling(typedRawData.JobValues.RemainingDeliveryTime.Value * typedRawData.CommonValues.Scale),
            JobIncome = typedRawData.JobValues.Income,
            JobCargoName = typedRawData.JobValues.CargoValues.Name,
            JobCargoMass = (int)Math.Ceiling(typedRawData.JobValues.CargoValues.Mass),
            JobCargoDamage = (int)Math.Round(typedRawData.JobValues.CargoValues.CargoDamage * 100),
            //Damage
            DamageCabin = (int)Math.Round(typedRawData.TruckValues.CurrentValues.DamageValues.Cabin * 100),
            DamageWheels = (int)Math.Round(typedRawData.TruckValues.CurrentValues.DamageValues.WheelsAvg * 100),
            DamageTransmission = (int)Math.Round(typedRawData.TruckValues.CurrentValues.DamageValues.Transmission * 100),
            DamageEngine = (int)Math.Round(typedRawData.TruckValues.CurrentValues.DamageValues.Engine * 100),
            DamageChassis = (int)Math.Round(typedRawData.TruckValues.CurrentValues.DamageValues.Chassis * 100),
            DamageTrailer = (int)Math.Ceiling(typedRawData.TrailerValues.Average(t => (t.DamageValues.Chassis + t.DamageValues.Cargo + t.DamageValues.Wheels + t.DamageValues.Body)/4)),
            TrailerAttached = typedRawData.TrailerValues.Length != 0,
            //Dashboard
            Gear = (short)typedRawData.TruckValues.CurrentValues.DashboardValues.GearDashboards,
            Rpm = (int)typedRawData.TruckValues.CurrentValues.DashboardValues.RPM,
            RpmMax = (int)typedRawData.TruckValues.ConstantsValues.MotorValues.EngineRpmMax,
            Speed = (short)typedRawData.TruckValues.CurrentValues.DashboardValues.Speed.Kph,
            SpeedLimit = (short)typedRawData.NavigationValues.SpeedLimit.Kph,
            CruiseControlOn = typedRawData.TruckValues.CurrentValues.DashboardValues.CruiseControl,
            CruiseControlSpeed = (short)typedRawData.TruckValues.CurrentValues.DashboardValues.CruiseControlSpeed.Kph,
            LowBeamOn = typedRawData.TruckValues.CurrentValues.LightsValues.BeamLow,
            HighBeamOn = typedRawData.TruckValues.CurrentValues.LightsValues.BeamHigh,
            ParkingBrakeOn = typedRawData.TruckValues.CurrentValues.MotorValues.BrakeValues.ParkingBrake,
            BatteryWarningOn = typedRawData.TruckValues.CurrentValues.DashboardValues.WarningValues.BatteryVoltage,
            HazardLightsOn = typedRawData.TruckValues.CurrentValues.LightsValues.HazardWarningLights,
            EngineWaterTempWarningOn = typedRawData.TruckValues.CurrentValues.DashboardValues.WarningValues.WaterTemperature,
            OilPressureWarningOn = typedRawData.TruckValues.CurrentValues.DashboardValues.WarningValues.OilPressure,
            FuelDistance = (int)typedRawData.TruckValues.CurrentValues.DashboardValues.FuelValue.Range,
            TruckName = $"{typedRawData.TruckValues.ConstantsValues.Brand} {typedRawData.TruckValues.ConstantsValues.Name}",
            BlinkerLeftOn = typedRawData.TruckValues.CurrentValues.LightsValues.BlinkerLeftOn,
            BlinkerRightOn = typedRawData.TruckValues.CurrentValues.LightsValues .BlinkerRightOn,
            FuelWarningOn = typedRawData.TruckValues.CurrentValues.DashboardValues.WarningValues.FuelW,
        };
    }
}