using HaddySimHub.GameData;
using HaddySimHub.GameData.Models;
using HaddySimHub.Logging;
using SCSSdkClient;

namespace HaddySimHub.Ets2;

public sealed class GameDataReader : IGameDataReader 
{
    private readonly SCSSdkTelemetry _telemetry;

    public event EventHandler<object>? RawDataUpdate;

    public GameDataReader(ILogger logger)
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
            TimeRemainingIrl = (int)Math.Ceiling(this.ConvertToIrlTimeSpan(typedRawData.NavigationValues.NavigationTime / 60)),
            RestTimeRemaining = typedRawData.CommonValues.NextRestStop.Value,
            RestTimeRemainingIrl = (int)Math.Ceiling(this.ConvertToIrlTimeSpan(typedRawData.CommonValues.NextRestStop.Value)),
            //Job info
            JobTimeRemaining = typedRawData.JobValues.RemainingDeliveryTime.Value,
            JobTimeRemainingIrl = (long)Math.Ceiling(this.ConvertToIrlTimeSpan(typedRawData.JobValues.RemainingDeliveryTime.Value)),
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
            //TODO
            DamageTrailer = 0,
            TrailerAttached = typedRawData.TrailerValues.Any(),
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
            FuelDistance = (int)typedRawData.TruckValues.CurrentValues.DashboardValues.FuelValue.Range,
            TruckName = $"{typedRawData.TruckValues.ConstantsValues.Brand} {typedRawData.TruckValues.ConstantsValues.Name}",
        };
    }

    private float ConvertToIrlTimeSpan(float minutes)
    {
        //Assume 1 minute = 15 minutes
        return minutes / 15;
    }
}