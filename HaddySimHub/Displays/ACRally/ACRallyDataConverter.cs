using HaddySimHub.Interfaces;
using HaddySimHub.Models;

namespace HaddySimHub.Displays.ACRally;

/// <summary>
/// Converts Assetto Corsa Rally telemetry to display format
/// </summary>
public class ACRallyDataConverter : IDataConverter<ACRallyTelemetry, DisplayUpdate>
{
    public DisplayUpdate Convert(ACRallyTelemetry data)
    {
        var rpmMax = System.Convert.ToInt32(data.MaxRpm * 10);

        // Convert stage time (in ms) to sector times for display
        // Using current stage time as lap time, and splitting across sectors
        var stageTimeSeconds = data.CurrentLapTime / 1000f;
        var sector1TimeSeconds = stageTimeSeconds * 0.5f; // Approximate first half
        var sector2TimeSeconds = stageTimeSeconds * 0.5f; // Approximate second half

        var displayData = new RallyData
        {
            Speed = System.Convert.ToInt32(data.SpeedMps * 3.6), // Convert m/s to km/h
            Rpm = System.Convert.ToInt32(data.Rpm * 10),
            RpmMax = rpmMax,
            Gear = data.Gear == 0 ? "N" : data.Gear < 0 ? "R" : System.Convert.ToInt32(data.Gear).ToString(),
            Clutch = System.Convert.ToInt32(data.ClutchInput * 100),
            Brake = System.Convert.ToInt32(data.BrakeInput * 100),
            Throttle = System.Convert.ToInt32(data.ThrottleInput * 100),
            CompletedPct = data.CurrentLap > 0 && data.TotalLaps > 0 
                ? Math.Min(System.Convert.ToInt32((data.CurrentLap / (float)data.TotalLaps) * 100), 100)
                : Math.Min(System.Convert.ToInt32(data.NormalizedSplinePosTrack * 100), 100),
            DistanceTravelled = System.Convert.ToInt32(data.NormalizedSplinePosTrack * 1000),
            Position = 1, // Rally doesn't have position like circuit racing
            Sector1Time = sector1TimeSeconds,
            Sector2Time = sector2TimeSeconds,
            LapTime = stageTimeSeconds,
        };

        return new DisplayUpdate { Type = DisplayType.RallyDashboard, Data = displayData };
    }
}
