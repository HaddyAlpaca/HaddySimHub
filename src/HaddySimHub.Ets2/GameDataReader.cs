using HaddySimHub.GameData;
using HaddySimHub.GameData.Models;
using SCSSdkClient;
using SCSSdkClient.Object;

namespace HaddySimHub.Ets2;

public sealed class GameDataReader : GameDataReaderBase
{
    private SCSSdkTelemetry? telemetry;
    private SCSTelemetry? lastReceivedData;

    public GameDataReader()
    {
        this.telemetry = new ();
        this.telemetry.Data += (SCSTelemetry data, bool newTimestamp) =>
        {
            this.lastReceivedData = data;
            this.UpdateRawData(data);
        };

        this.telemetry.Tollgate += (s, e) =>
            this.SendNotification($"Tol betaald: {this.lastReceivedData?.GamePlay.TollgateEvent.PayAmount:C0}");

        this.telemetry.RefuelPayed += (s, e) =>
            this.SendNotification($"Brandstof betaald: {this.lastReceivedData?.GamePlay.RefuelEvent.Amount:C0}");

        this.telemetry.Ferry += (s, e) =>
            this.SendNotification(
                $"Bootreis gestart: {this.lastReceivedData?.GamePlay.FerryEvent.SourceName}" +
                $" - {this.lastReceivedData?.GamePlay.FerryEvent.TargetName} " +
                $"({this.lastReceivedData?.GamePlay.FerryEvent.PayAmount:C0})");

        this.telemetry.Train += (s, e) =>
            this.SendNotification($"Treinreis gestart: {this.lastReceivedData?.GamePlay.TrainEvent.SourceName} - {this.lastReceivedData?.GamePlay.TrainEvent.TargetName} ({this.lastReceivedData?.GamePlay.TrainEvent.PayAmount:C0})");

        this.telemetry.JobDelivered += (s, e) =>
            this.SendNotification($"Opdracht afgerond, opbrengst: {this.lastReceivedData?.GamePlay.JobDelivered.Revenue:C0}");

        this.telemetry.JobCancelled += (s, e) =>
            this.SendNotification($"Opdracht geannuleerd, boete: {this.lastReceivedData?.GamePlay.JobCancelled.Penalty:C0}");

        this.telemetry.Fined += (s, e) =>
        {
            string offenceDesription = string.Empty;
            switch (this.lastReceivedData?.GamePlay.FinedEvent?.Offence)
            {
                case Offence.Crash:
                    offenceDesription = "Ongeluk";
                    break;
                case Offence.Avoid_sleeping:
                    offenceDesription = "Overtreding van rusttijden";
                    break;
                case Offence.Wrong_way:
                    offenceDesription = "Spookrijden";
                    break;
                case Offence.Speeding:
                case Offence.Speeding_camera:
                    offenceDesription = "Snelheidsovertreding";
                    break;
                case Offence.No_lights:
                    offenceDesription = "Verlichting";
                    break;
                case Offence.Red_signal:
                    offenceDesription = "Roodlicht overtreding";
                    break;
                case Offence.Avoid_weighting:
                    offenceDesription = "Weging vermeden";
                    break;
                case Offence.Illegal_trailer:
                    offenceDesription = "Illegale trailer";
                    break;
                case Offence.Avoid_Inspection:
                    offenceDesription = "Inspectie vermeden";
                    break;
                case Offence.Illegal_Border_Crossing:
                    break;
                case Offence.Hard_Shoulder_Violation:
                    offenceDesription = "Rijden op de vluchtstrook";
                    break;
                case Offence.Damaged_Vehicle_Usage:
                    offenceDesription = "Rijden met beschadigd voertuig";
                    break;
                default:
                    offenceDesription = "Overtreding";
                    break;
            }

            this.SendNotification($"{offenceDesription}: {this.lastReceivedData?.GamePlay.FinedEvent.Amount:C0}");
        };
    }

    public override DisplayType CurrentDisplayType => DisplayType.TruckDashboard;

    public override object Convert(object rawData)
    {
        if (rawData is not SCSTelemetry typedRawData)
        {
            return new TruckData();
        }

        return new TruckData()
        {
            // Navigation info
            SourceCity = typedRawData.JobValues.CitySource,
            SourceCompany = typedRawData.JobValues.CompanySource,
            DestinationCity = typedRawData.JobValues.CityDestination,
            DestinationCompany = typedRawData.JobValues.CompanyDestination,
            DistanceRemaining = (int)Math.Round(Math.Max(typedRawData.NavigationValues.NavigationDistance, 0) / 1000),
            TimeRemaining = (int)Math.Round(Math.Max(typedRawData.NavigationValues.NavigationTime, 0) / 60),
            TimeRemainingIrl = (int)Math.Round(Math.Max(typedRawData.NavigationValues.NavigationTime, 0) / 60 / typedRawData.CommonValues.Scale),
            RestTimeRemaining = Math.Max(typedRawData.CommonValues.NextRestStop.Value, 0),
            RestTimeRemainingIrl = (int)Math.Round(Math.Max(typedRawData.CommonValues.NextRestStop.Value, 0) / typedRawData.CommonValues.Scale),

            // Job info
            JobTimeRemaining = Math.Max(typedRawData.JobValues.RemainingDeliveryTime.Value, 0),
            JobTimeRemainingIrl = (long)Math.Round(Math.Max(typedRawData.JobValues.RemainingDeliveryTime.Value, 0) / typedRawData.CommonValues.Scale),
            JobIncome = typedRawData.JobValues.Income,
            JobCargoName = typedRawData.JobValues.CargoValues.Name,
            JobCargoMass = (int)Math.Ceiling(typedRawData.JobValues.CargoValues.Mass),
            JobCargoDamage = (int)Math.Round(typedRawData.JobValues.CargoValues.CargoDamage * 100),

            // Damage
            DamageTruckCabin = (int)Math.Round(typedRawData.TruckValues.CurrentValues.DamageValues.Cabin * 100),
            DamageTruckWheels = (int)Math.Round(typedRawData.TruckValues.CurrentValues.DamageValues.WheelsAvg * 100),
            DamageTruckTransmission = (int)Math.Round(typedRawData.TruckValues.CurrentValues.DamageValues.Transmission * 100),
            DamageTruckEngine = (int)Math.Round(typedRawData.TruckValues.CurrentValues.DamageValues.Engine * 100),
            DamageTruckChassis = (int)Math.Round(typedRawData.TruckValues.CurrentValues.DamageValues.Chassis * 100),
            DamageTrailerChassis = (int)Math.Round(typedRawData.TrailerValues.Average(t => t.DamageValues.Chassis) * 100),
            DamageTrailerCargo = (int)Math.Round(typedRawData.TrailerValues.Average(t => t.DamageValues.Cargo) * 100),
            DamageTrailerWheels = (int)Math.Round(typedRawData.TrailerValues.Average(t => t.DamageValues.Wheels) * 100),
            DamageTrailerBody = (int)Math.Round(typedRawData.TrailerValues.Average(t => t.DamageValues.Body) * 100),
            NumberOfTrailersAttached = typedRawData.TrailerValues.Length,

            // Dashboard
            Gear = (short)typedRawData.TruckValues.CurrentValues.DashboardValues.GearDashboards,
            Rpm = (int)typedRawData.TruckValues.CurrentValues.DashboardValues.RPM,
            RpmMax = (int)typedRawData.TruckValues.ConstantsValues.MotorValues.EngineRpmMax,
            Speed = (short)Math.Max(typedRawData.TruckValues.CurrentValues.DashboardValues.Speed.Kph, 0),
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
            BlinkerRightOn = typedRawData.TruckValues.CurrentValues.LightsValues.BlinkerRightOn,
            FuelWarningOn = typedRawData.TruckValues.CurrentValues.DashboardValues.WarningValues.FuelW,
            WipersOn = typedRawData.TruckValues.CurrentValues.DashboardValues.Wipers,
            GameTime = typedRawData.CommonValues.GameTime.Value,
        };
    }
}