using HaddySimHub;
using HaddySimHub.Interfaces;
using HaddySimHub.Models;

namespace HaddySimHub.Displays.ACC;

public class ACCDataConverter : IDataConverter<ACCTelemetry, DisplayUpdate>
{
    public DisplayUpdate Convert(ACCTelemetry source)
    {
        // Assetto Corsa encodes gears as 0 = reverse, 1 = neutral, 2 = first gear.
        string gearString = source.Gear switch
        {
            <= 0 => "R",
            1 => "N",
            _ => (source.Gear - 1).ToString()
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
            // The graphics page counts laps finished, so the lap being driven is one further on.
            CurrentLap = source.CurrentLap + 1,
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
            FuelLastLap = null,
            FuelEstLaps = source.FuelEstimatedLaps,
            CurrentLapTime = currentLapSeconds,
            LastLapTime = lastLapSeconds,
            LastLapTimeDelta = deltaSeconds,
            BestLapTime = bestLapSeconds,
            BestLapTimeDelta = null,
            ClutchPct = (int)(source.Clutch * 100f),
            ThrottlePct = (int)(source.Gas * 100f),
            BrakePct = (int)(source.Brake * 100f),
            PitLimiterOn = source.PitLimiterOn == 1,
            SteeringPct = (int)(Math.Abs(source.SteerAngle) * 100f),
            BrakeBias = source.BrakeBias,
            RainIntensity = source.RainIntensity,
            WindSpeed = source.WindSpeed,
            WindDirection = source.WindDirection,
            TrackGripStatus = source.TrackGripStatus,
        };

        return new DisplayUpdate
        {
            Type = DisplayType.RaceDashboard,
            Data = raceData
        };
    }
}
