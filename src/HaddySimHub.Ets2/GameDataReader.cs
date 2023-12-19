using HaddySimHub.GameData;
using HaddySimHub.GameData.Models;
using HaddySimHub.Logging;
using SCSSdkClient;
using System.Text.Json;

namespace HaddySimHub.Ets2;

public sealed class GameDataReader : GameDataReaderBase 
{
    private readonly SCSSdkTelemetry _telemetry;

    public override event EventHandler<object>? RawDataUpdate;

    public GameDataReader(ILogger logger): base(logger)
    {
        this._telemetry = new SCSSdkTelemetry();
        this._telemetry.Data += (SCSSdkClient.Object.SCSTelemetry data, bool newTimestamp) =>
        {
            this._logger.Debug($"ETS2 data:\n{JsonSerializer.Serialize(data)}\n");

            this.RawDataUpdate?.Invoke(this, data);
        };
    }

    public override object Convert(object rawData)
    {
        if (rawData is not SCSSdkClient.Object.SCSTelemetry typedRawData)
        {
            throw new InvalidDataException("Received data is not of type SCSTelemetry");
        }

        return new TruckData()
        {
            //Navigation info
            SourceCity = typedRawData.JobValues.CitySource,
            SourceCompany = typedRawData.JobValues.CompanySource,
            DestinationCity = typedRawData.JobValues.CityDestination,
            DestinationCompany = typedRawData.JobValues.CompanyDestination,
            DistanceRemaining = (int)Math.Round(typedRawData.NavigationValues.NavigationDistance / 1000),
            TimeRemaining = (int)Math.Round(typedRawData.NavigationValues.NavigationTime / 60),
            TimeRemainingIrl = (int)Math.Round((typedRawData.NavigationValues.NavigationTime / 60) / typedRawData.CommonValues.Scale),
            RestTimeRemaining = typedRawData.CommonValues.NextRestStop.Value,
            RestTimeRemainingIrl = (int)Math.Round(typedRawData.CommonValues.NextRestStop.Value / typedRawData.CommonValues.Scale),
            //Job info
            JobTimeRemaining = typedRawData.JobValues.RemainingDeliveryTime.Value,
            JobTimeRemainingIrl = (long)Math.Round(typedRawData.JobValues.RemainingDeliveryTime.Value / typedRawData.CommonValues.Scale),
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
            SpeedLimit = (short)Math.Max(typedRawData.NavigationValues.SpeedLimit.Kph, 0),
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