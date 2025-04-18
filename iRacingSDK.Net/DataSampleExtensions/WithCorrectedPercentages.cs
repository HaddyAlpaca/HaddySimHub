﻿namespace iRacingSDK;

public static partial class DataSampleExtensions
{
    /// <summary>
    /// Work around bug in iRacing data stream, where cars lap percentage is reported slightly behind 
    /// actual frame - so that as cars cross the line, their percentage still is in the 99% range
    /// a frame later there percentage drops to near 0%
    /// Fix is to watch for lap change - and zero percentage until less than 90%
    /// </summary>
    /// <param name="samples"></param>
    /// <returns></returns>
    public static IEnumerable<DataSample> WithCorrectedPercentages(this IEnumerable<DataSample> samples)
    {
        int[] lastLaps = null;

        foreach (var data in samples.ForwardOnly())
        {
            if (lastLaps == null)
                lastLaps = (int[])data.Telemetry.CarIdxLap.Clone();

            for (int i = 0; i < data.SessionData.DriverInfo.CompetingDrivers.Length; i++)
                if (data.Telemetry.HasData(i))
                    FixPercentagesOnLapChange(
                        ref lastLaps[i],
                        ref data.Telemetry.CarIdxLapDistPct[i],
                        data.Telemetry.CarIdxLap[i]);

            yield return data;
        }
    }

    static void FixPercentagesOnLapChange(ref int lastLap, ref float carIdxLapDistPct, int carIdxLap)
    {
        if (carIdxLap > lastLap && carIdxLapDistPct > 0.80f)
            carIdxLapDistPct = 0;
        else
            lastLap = carIdxLap;
    }
}
