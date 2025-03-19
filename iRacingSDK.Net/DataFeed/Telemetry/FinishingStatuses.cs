namespace iRacingSDK;

public partial class Telemetry : Dictionary<string, object>
{
    public bool[] HasSeenCheckeredFlag;
    public bool IsFinalLap;
    public bool LeaderHasFinished;
    public bool[] HasRetired;

    public bool HasData(int carIdx) => this.CarIdxTrackSurface[carIdx] != TrackLocation.NotInWorld;
}
