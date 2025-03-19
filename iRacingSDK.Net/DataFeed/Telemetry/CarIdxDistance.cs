namespace iRacingSDK;

public partial class Telemetry : Dictionary<string, object>
	{
    float[] carIdxDistance;
    public float[] CarIdxDistance
    {
        get
        {
            carIdxDistance ??= [.. Enumerable.Range(0, 64).Select(CarIdx => this.CarIdxLap[CarIdx] + this.CarIdxLapDistPct[CarIdx] )];

            return carIdxDistance;
        }
        internal set
        {
            carIdxDistance = value;
        }
    }

}
