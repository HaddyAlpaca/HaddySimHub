using HaddySimHub.Interfaces;
using HaddySimHub.Models;

namespace HaddySimHub.Displays.AC;

/// <summary>
/// Converts Assetto Corsa telemetry to the race dashboard model.
/// </summary>
public class ACDataConverter : IDataConverter<ACTelemetry, DisplayUpdate>
{
    public DisplayUpdate Convert(ACTelemetry source)
    {
        var raceData = new RaceData
        {
            SessionType = FormatSessionType(source.SessionType),
            IsLimitedTime = source.SessionTimeLeft > 0,
            IsLimitedSessionLaps = source.NumberOfLaps > 0,

            // The graphics page counts laps finished, so the lap being driven is one further on.
            CurrentLap = source.CompletedLaps + 1,
            TotalLaps = source.NumberOfLaps,
            SessionTimeRemaining = source.SessionTimeLeft / 1000f,
            Position = source.Position > 0 ? source.Position : null,

            Speed = (int)Math.Max(source.SpeedKmh, 0f),
            Gear = FormatGear(source.Gear),
            Rpm = source.Rpms,
            RpmMax = source.MaxRpm,
            TrackTemp = source.RoadTemp,
            AirTemp = source.AirTemp,

            FuelRemaining = source.Fuel,
            FuelAvgLap = null,  // Assetto Corsa does not report fuel use per lap
            FuelLastLap = null,
            FuelEstLaps = 0,    // Derived from fuel use per lap, which is not reported

            CurrentLapTime = source.CurrentTime / 1000f,
            LastLapTime = source.LastTime / 1000f,
            LastLapTimeDelta = null,  // Not exposed by the Assetto Corsa shared memory
            BestLapTime = ToLapTimeSeconds(source.BestTime),
            BestLapTimeDelta = null,

            ClutchPct = (int)(source.Clutch * 100f),
            ThrottlePct = (int)(source.Gas * 100f),
            BrakePct = (int)(source.Brake * 100f),
            PitLimiterOn = source.PitLimiterOn == 1,
            SteeringPct = (int)(Math.Abs(source.SteerAngle) * 100f),
            BrakeBias = source.BrakeBias,
        };

        return new DisplayUpdate
        {
            Type = DisplayType.RaceDashboard,
            Data = raceData,
        };
    }

    /// <summary>
    /// Assetto Corsa encodes gears as 0 = reverse, 1 = neutral, 2 = first gear.
    /// </summary>
    private static string FormatGear(int gear) => gear switch
    {
        <= 0 => "R",
        1 => "N",
        _ => (gear - 1).ToString(),
    };

    private static string FormatSessionType(ACSessionType sessionType) => sessionType switch
    {
        ACSessionType.Practice => "Practice",
        ACSessionType.Qualifying => "Qualifying",
        ACSessionType.Race => "Race",
        ACSessionType.Hotlap => "Hotlap",
        ACSessionType.TimeAttack => "Time Attack",
        ACSessionType.Drift => "Drift",
        ACSessionType.Drag => "Drag",
        _ => "Practice",
    };

    /// <summary>
    /// The game reports an unset best lap as <see cref="int.MaxValue"/> rather than zero.
    /// </summary>
    private static float? ToLapTimeSeconds(int milliseconds) =>
        milliseconds > 0 && milliseconds != int.MaxValue ? milliseconds / 1000f : null;
}
