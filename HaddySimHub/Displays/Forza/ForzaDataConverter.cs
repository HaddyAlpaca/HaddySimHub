using HaddySimHub.Interfaces;
using HaddySimHub.Models;

namespace HaddySimHub.Displays.Forza;

public sealed class ForzaDataConverter : IDataConverter<ForzaTelemetry, DisplayUpdate>
{
    public DisplayUpdate Convert(ForzaTelemetry source)
    {
        var raceData = new RaceData
        {
            SessionType = source.IsRaceOn != 0 ? "Race" : "Practice",
            CurrentLap = source.LapNumber,
            Speed = (int)Math.Max(source.Speed * 3.6f, 0f),
            Gear = FormatGear(source.Gear),
            Rpm = (int)Math.Max(source.CurrentEngineRpm, 0f),
            RpmMax = (int)Math.Max(source.EngineMaxRpm, 0f),
            FuelRemaining = source.Fuel,
            CurrentLapTime = Math.Max(source.CurrentLap, 0f),
            LastLapTime = Math.Max(source.LastLap, 0f),
            BestLapTime = source.BestLap > 0f ? source.BestLap : null,
            ThrottlePct = source.Throttle * 100 / byte.MaxValue,
            BrakePct = source.Brake * 100 / byte.MaxValue,
            ClutchPct = source.Clutch * 100 / byte.MaxValue,
            SteeringPct = (int)(Math.Abs(source.Steer) * 100f / sbyte.MaxValue),
            Position = source.RacePosition > 0 ? source.RacePosition : null,
            CarOrdinal = source.CarOrdinal,
            CarClass = source.CarClass,
            CarPerformanceIndex = source.CarPerformanceIndex,
            DrivetrainType = source.DrivetrainType,
            NumCylinders = source.NumCylinders,
            Power = source.Power,
            Torque = source.Torque,
        };

        return new DisplayUpdate
        {
            Type = DisplayType.RaceDashboard,
            Data = raceData,
        };
    }

    private static string FormatGear(byte gear) => gear switch
    {
        0 => "R",
        1 => "N",
        _ => (gear - 1).ToString(),
    };
}
