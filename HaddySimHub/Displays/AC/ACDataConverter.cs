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

        // Calculate fuel estimates if possible
        float fuelAvgLap = source.FuelPressure > 0 ? source.FuelPressure * 0.1f : 0;
        float fuelRemaining = source.CarDamage;  // Using damage as proxy for fuel percentage

        var raceData = new RaceData
        {
            SessionType = sessionType,
            IsLimitedTime = false,
            IsLimitedSessionLaps = source.TotalLaps > 0,
            CurrentLap = source.CurrentLap,
            TotalLaps = source.TotalLaps,
            SessionTimeRemaining = source.SessionTimeLeft / 1000f,
            Position = 1,  // AC doesn't provide position directly
            Speed = speedKmh,
            Gear = gearString,
            Rpm = (int)source.Rpm,
            RpmMax = (int)source.MaxRpm,
            TrackTemp = source.RoadTemp,
            AirTemp = source.AirTemp,
            FuelRemaining = fuelRemaining,
            FuelAvgLap = fuelAvgLap,
            FuelLastLap = fuelAvgLap,
            FuelEstLaps = source.FuelEstimatedLaps,
            CurrentLapTime = source.CurrentLapTime / 1000f,
            LastLapTime = source.LastLapTime / 1000f,
            CarNumber = "1"  // AC doesn't provide car number
        };

        return new DisplayUpdate
        {
            Type = DisplayType.RaceDashboard,
            Data = raceData
        };
    }
}
