using System.Diagnostics;

namespace iRacingSDK;

public partial class Telemetry : Dictionary<string, object>
{
    float? raceDistance;

    public float RaceDistance
    {
        get
        {
            if (raceDistance != null)
                return raceDistance.Value;

            raceDistance = this.CarIdxLap
                .Select((lap, idx) => new { Lap = lap, Distance = lap + this.CarIdxLapDistPct[idx] })
                .Max(l => l.Distance);

            if (raceDistance.Value < this.RaceLaps)
            {
                Trace.WriteLine("WARNING! No cars on current RaceLaps", "DEBUG");
                return this.RaceLaps;
            }

            return raceDistance.Value;
        }
    }
}
