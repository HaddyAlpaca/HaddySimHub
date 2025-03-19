namespace iRacingSDK;

public static partial class DataSampleExtensions
{
    /// <summary>
    /// Set the CarIdxPitStopCount field for each enumerted datasample's telemetry
    /// </summary>
    public static IEnumerable<DataSample> WithPitStopCounts(this IEnumerable<DataSample> samples)
    {
        var lastTrackLocation = Enumerable.Repeat(TrackLocation.NotInWorld, 64).ToArray();
        var carIdxPitStopCount = new int[64];

        foreach (var data in samples.ForwardOnly())
        {
            CapturePitStopCounts(lastTrackLocation, carIdxPitStopCount, data);

            data.Telemetry.CarIdxPitStopCount = (int[])carIdxPitStopCount.Clone();

            yield return data;
        }
    }

    static void CapturePitStopCounts(TrackLocation[] lastTrackLocation, int[] carIdxPitStopCount, DataSample data)
    {
        if (data.LastSample == null)
            return;

        CaptureLastTrackLocations(lastTrackLocation, data);
        IncrementPitStopCounts(lastTrackLocation, carIdxPitStopCount, data);
    }

    static void CaptureLastTrackLocations(TrackLocation[] lastTrackLocation, DataSample data)
    {
        var last = data.LastSample.Telemetry.CarIdxTrackSurface;
        for (var i = 0; i < last.Length; i++)
            if (last[i] != TrackLocation.NotInWorld)
                lastTrackLocation[i] = last[i];
    }

    static void IncrementPitStopCounts(TrackLocation[] lastTrackLocation, int[] carIdxPitStopCount, DataSample data)
    {
        var current = data.Telemetry.CarIdxTrackSurface;
        for (var i = 0; i < current.Length; i++)
            if (lastTrackLocation[i] != TrackLocation.InPitStall && current[i] == TrackLocation.InPitStall)
                carIdxPitStopCount[i] += 1;
    }
}
