using HaddySimHub.Interfaces;
using HaddySimHub.Models;

namespace HaddySimHub.Displays.AC;

/// <summary>
/// Converts Assetto Corsa telemetry to RaceData
/// </summary>
public class ACDataConverter : IDataConverter<ACTelemetry, DisplayUpdate>
{
    public DisplayUpdate Convert(ACTelemetry source)
    {
        // Determine gear string
        string gearString = source.Gear switch
        {
            -1 => "R",
            0 => "N",
            > 0 => ((int)source.Gear).ToString(),
            _ => "N"
        };

        // Convert speed from m/s to km/h
        int speedKmh = (int)(source.SpeedMps * 3.6f);

        // Determine session type
        string sessionType = source.SessionType switch
        {
            0 => "Practice",
            1 => "Qualifying",
            2 => "Race",
            _ => "Practice"
        };

        // Calculate fuel estimate from the game's own value (AC exposes no fuel level here)
        var raceData = new RaceData
        {
            // Universal fields
            SessionType = sessionType,
            IsLimitedTime = false,
            IsLimitedSessionLaps = source.TotalLaps > 0,
            CurrentLap = source.CurrentLap,
            TotalLaps = source.TotalLaps,
            SessionTimeRemaining = source.SessionTimeLeft / 1000f,
            Position = null,  // AC shared memory (physics page) does not expose position
            Speed = speedKmh,
            Gear = gearString,
            Rpm = (int)source.Rpm,
            RpmMax = (int)source.MaxRpm,
            TrackTemp = source.RoadTemp,
            AirTemp = source.AirTemp,
            FuelRemaining = null,  // Not exposed by AC shared memory
            FuelAvgLap = null,  // Not exposed by AC shared memory
            FuelLastLap = null,  // Not exposed by AC shared memory
            FuelEstLaps = source.FuelEstimatedLaps,
            CurrentLapTime = source.CurrentLapTime / 1000f,
            LastLapTime = source.LastLapTime / 1000f,
            LastLapTimeDelta = null,  // Not exposed by AC shared memory
            BestLapTime = null,  // Not exposed by AC shared memory
            BestLapTimeDelta = null,  // Not exposed by AC shared memory
            ClutchPct = (int)(source.ClutchInput * 100f),
            ThrottlePct = (int)(source.ThrottleInput * 100f),
            BrakePct = (int)(source.BrakeInput * 100f),
            PitLimiterOn = source.PitLimiterOn == 1,
            SteeringPct = 0,  // Not available
        };

        return new DisplayUpdate
        {
            Type = DisplayType.RaceDashboard,
            Data = raceData
        };
    }
}
