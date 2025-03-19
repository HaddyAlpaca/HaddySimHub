namespace iRacingSDK;

public partial class Telemetry : Dictionary<string, object>
{
    private int[] carIdxPitStopCount;
    public int[] CarIdxPitStopCount
    {
        get => carIdxPitStopCount;
        set => carIdxPitStopCount = value;
    }
}