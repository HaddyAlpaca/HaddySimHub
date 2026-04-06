using HaddySimHub;
using HaddySimHub.Interfaces;
using HaddySimHub.Models;

namespace HaddySimHub.Displays.ACC;

public class ACCDataConverter : IDataConverter<ACCTelemetry, DisplayUpdate>
{
    public DisplayUpdate Convert(ACCTelemetry source)
    {
        string gearString = source.Gear switch
        {
            < 0 => "R",
            0 => "N",
            _ => source.Gear.ToString()
        };

        string sessionType = source.SessionType switch
        {
            0 => "Practice",
            1 => "Qualifying",
            2 => "Race",
            3 => "Hotlap",
            4 => "Time Attack",
            5 => "Drift",
            6 => "Drag",
            7 => "Hotstint",
            8 => "Superpole",
            _ => "Practice"
        };

        float currentLapSeconds = source.CurrentTimeMs / 1000f;
        float lastLapSeconds = source.LastTimeMs / 1000f;
        float bestLapSeconds = source.BestTimeMs / 1000f;
        float deltaSeconds = source.DeltaMs / 1000f;

        var raceData = new RaceData
        {
            SessionType = sessionType,
            IsLimitedTime = source.SessionTimeLeftMs > 0,
            IsLimitedSessionLaps = source.NumberOfLaps > 0,
            CurrentLap = source.CurrentLap,
            TotalLaps = source.NumberOfLaps,
            SessionTimeRemaining = source.SessionTimeLeftMs / 1000f,
            Position = source.Position,
            Speed = (int)source.SpeedKmh,
            Gear = gearString,
            Rpm = source.Rpms,
            RpmMax = (int)source.MaxRpm,
            TrackTemp = source.RoadTemp,
            AirTemp = source.AirTemp,
            FuelRemaining = source.Fuel,
            FuelAvgLap = source.FuelPerLap,
            FuelLastLap = source.FuelPerLap,
            FuelEstLaps = source.FuelEstimatedLaps,
            CurrentLapTime = currentLapSeconds,
            LastLapTime = lastLapSeconds,
            LastLapTimeDelta = deltaSeconds,
            BestLapTime = bestLapSeconds,
            BestLapTimeDelta = 0,
            ClutchPct = (int)(source.Clutch * 100f),
            ThrottlePct = (int)(source.Gas * 100f),
            BrakePct = (int)(source.Brake * 100f),
            PitLimiterOn = source.PitLimiterOn == 1,
            CarNumber = "1",
            SteeringPct = (int)(Math.Abs(source.SteerAngle) * 100f),
            BrakeBias = source.BrakeBias,
            PenaltyTime = (int)source.PenaltyTime,
            DrsEnabled = source.DrsEnabled == 1
        };

        return new DisplayUpdate
        {
            Type = DisplayType.RaceDashboard,
            Data = raceData
        };
    }
}
