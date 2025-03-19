using iRacingSDK.Support;

namespace iRacingSDK;

public static partial class DataSampleExtensions
{
    /// <summary>
    /// Logs an error is frame numbers goes down - indicating the game is replaying in reverse.
    /// Sometimes stream may glitch and the FrameNum decements
    /// </summary>
    public static IEnumerable<DataSample> ForwardOnly(this IEnumerable<DataSample> samples)
    {
        foreach (var data in samples)
        {
            if (data.LastSample != null && data.LastSample.Telemetry.ReplayFrameNum > data.Telemetry.ReplayFrameNum)
                TraceInfo.WriteLine(
                    "WARNING! Replay data reversed.  Current enumeration only support iRacing in forward mode. Received sample {0} after sample {1}",
                    data.Telemetry.ReplayFrameNum, data.LastSample.Telemetry.ReplayFrameNum);
            else
                yield return data;
        }
    }
}
