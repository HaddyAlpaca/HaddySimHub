using HaddySimHub.Interfaces;
using HaddySimHub.Models;

namespace HaddySimHub.Displays.ACC;

/// <summary>
/// Converts Assetto Corsa Competizione telemetry to RaceData
/// </summary>
public class ACCDataConverter : IDataConverter<ACCTelemetry, DisplayUpdate>
{
    public DisplayUpdate Convert(ACCTelemetry source)
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
        int speedKmh = (int)(source.SpeedMs * 3.6f);

        // Determine session type
        string sessionType = source.SessionType switch
        {
            0 => "Practice",
            1 => "Qualifying",
            2 => "Race",
            _ => "Practice"
        };

        // Calculate lap time
        float lapTimeSeconds = source.LapTimeMs / 1000f;

        var raceData = new RaceData
        {
            SessionType = sessionType,
            IsLimitedTime = false,
            IsLimitedSessionLaps = source.MaxLaps > 0,
            CurrentLap = source.CurrentLapCount,
            TotalLaps = source.MaxLaps,
            SessionTimeRemaining = source.SessionTimeLeftMs / 1000f,
            Position = 1,  // ACC doesn't provide position directly in this struct
            Speed = speedKmh,
            Gear = gearString,
            Rpm = (int)source.Rpm,
            RpmMax = (int)source.MaxRpm,
            TrackTemp = source.RoadTemp,
            AirTemp = source.AirTemp,
            FuelRemaining = 0,  // Not directly available in basic telemetry
            FuelAvgLap = source.FuelAutoConsumption,
            FuelLastLap = source.FuelAutoConsumption,
            FuelEstLaps = source.FuelEstimatedLaps,
            CurrentLapTime = lapTimeSeconds,
            LastLapTime = lapTimeSeconds,
            CarNumber = "1"  // ACC doesn't provide car number in basic telemetry
        };

        return new DisplayUpdate
        {
            Type = DisplayType.RaceDashboard,
            Data = raceData
        };
    }
}
