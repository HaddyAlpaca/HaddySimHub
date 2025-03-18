namespace iRacingSDK;

public partial class Telemetry : Dictionary<string, object>
{
    int[] carIdxPitStopCount;
    public int[] CarIdxPitStopCount
    {
        get => carIdxPitStopCount;
        set => carIdxPitStopCount = value;
    }
}