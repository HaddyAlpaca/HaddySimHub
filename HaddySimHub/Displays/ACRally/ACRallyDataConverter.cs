using HaddySimHub.Interfaces;
using HaddySimHub.Models;

namespace HaddySimHub.Displays.ACRally;

/// <summary>
/// Converts Assetto Corsa Rally telemetry to the rally dashboard model.
/// </summary>
/// <remarks>
/// A rally stage is a single lap, so stage progress comes from the position along
/// the spline rather than a lap count. The shared memory pages expose the duration
/// of the sector that was completed last, but not the running split times the
/// dashboard shows, so those are accumulated here as the car crosses each sector.
/// </remarks>
public class ACRallyDataConverter : IDataConverter<ACRallyTelemetry, DisplayUpdate>
{
    private readonly object _sync = new();

    private int _lastSectorIndex;
    private int _lastStageTimeMs;
    private float _sector1Time;
    private float _sector2Time;

    public DisplayUpdate Convert(ACRallyTelemetry data)
    {
        var (sector1Time, sector2Time) = TrackSectorSplits(data);

        var stageTimeSeconds = data.CurrentTime / 1000f;
        var rpmMax = data.MaxRpm > 0 ? data.MaxRpm : System.Convert.ToInt32(data.CurrentMaxRpm);
        var progress = Math.Clamp(data.NormalizedCarPosition, 0f, 1f);

        var displayData = new RallyData
        {
            Speed = System.Convert.ToInt32(Math.Max(data.SpeedKmh, 0f)),
            Rpm = data.Rpms,
            RpmMax = rpmMax,
            Gear = FormatGear(data.Gear),
            CompletedPct = System.Convert.ToInt32(progress * 100),
            DistanceTravelled = GetDistanceTravelled(data, progress),
            Position = data.Position > 0 ? data.Position : 1,
            Sector1Time = sector1Time,
            Sector2Time = sector2Time,
            LapTime = stageTimeSeconds,
        };

        return new DisplayUpdate { Type = DisplayType.RallyDashboard, Data = displayData };
    }

    /// <summary>
    /// Assetto Corsa reports gears as 0 = reverse, 1 = neutral, 2 = first gear.
    /// </summary>
    private static string FormatGear(int gear) => gear switch
    {
        <= 0 => "R",
        1 => "N",
        _ => (gear - 1).ToString(),
    };

    private static int GetDistanceTravelled(ACRallyTelemetry data, float progress)
    {
        // The graphics page reports metres covered directly; the spline position
        // scaled by the stage length covers the frames before it starts counting.
        if (data.DistanceTraveled > 0)
        {
            return System.Convert.ToInt32(data.DistanceTraveled);
        }

        return data.TrackSplineLength > 0
            ? System.Convert.ToInt32(progress * data.TrackSplineLength)
            : 0;
    }

    /// <summary>
    /// Records the elapsed stage time each time the car crosses into a new sector,
    /// and starts over when a new stage begins.
    /// </summary>
    private (float Sector1Time, float Sector2Time) TrackSectorSplits(ACRallyTelemetry data)
    {
        lock (_sync)
        {
            var restarted = data.CurrentSectorIndex < _lastSectorIndex || data.CurrentTime < _lastStageTimeMs;

            if (restarted)
            {
                _sector1Time = 0;
                _sector2Time = 0;
            }
            else if (data.CurrentSectorIndex > _lastSectorIndex)
            {
                var elapsedSeconds = data.CurrentTime / 1000f;

                // The sector just left is the one before the index the car moved into.
                switch (data.CurrentSectorIndex - 1)
                {
                    case 0:
                        _sector1Time = elapsedSeconds;
                        break;
                    case 1:
                        _sector2Time = elapsedSeconds;
                        break;
                }
            }

            _lastSectorIndex = data.CurrentSectorIndex;
            _lastStageTimeMs = data.CurrentTime;

            return (_sector1Time, _sector2Time);
        }
    }
}
