namespace iRacingSDK;

public static partial class DataSampleExtensions
{
    public static IEnumerable<DataSample> WithFastestLaps(this IEnumerable<DataSample> samples)
    {
        FastLap lastFastLap = null;
        var lastDriverLaps = new int[64];
        var driverLapStartTime = new double[64];
        var fastestLapTime = double.MaxValue;

			foreach (var data in samples.ForwardOnly())
        {
            var carsAndLaps = data.Telemetry
                .CarIdxLap
                .Select((l, i) => new { CarIdx = i, Lap = l })
                .Skip(1)
                .Take(data.SessionData.DriverInfo.CompetingDrivers.Length - 1);

            foreach (var lap in carsAndLaps)
            {
                if (lap.Lap == -1)
                    continue;

                if (lap.Lap == lastDriverLaps[lap.CarIdx] + 1)
                {
                    var lapTime = data.Telemetry.SessionTime - driverLapStartTime[lap.CarIdx];

                    driverLapStartTime[lap.CarIdx] = data.Telemetry.SessionTime;
                    lastDriverLaps[lap.CarIdx] = lap.Lap;

                    if (lap.Lap > 1 && lapTime < fastestLapTime)
                    {
                        fastestLapTime = lapTime;

                        lastFastLap = new FastLap
                        {
                            Time = TimeSpan.FromSeconds(lapTime),
                            Driver = data.SessionData.DriverInfo.CompetingDrivers[lap.CarIdx]
                        };
                    }
                }
            }

            data.Telemetry.FastestLap = lastFastLap;

            yield return data;
        }
    }
}
