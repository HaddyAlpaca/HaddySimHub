namespace iRacingSDK;

public partial class Telemetry : Dictionary<string, object>
{
    private LapSector[] carSectorIdx;
    public LapSector[] CarSectorIdx //0 -> Start/Finish, 1 -> 33%, 2-> 66%
    {
        get
        {
            if (carSectorIdx != null)
                return carSectorIdx;

            carSectorIdx = new LapSector[64];
            for(int i = 0; i < 64; i++)
					carSectorIdx[i] = new LapSector(this.CarIdxLap[i], ToSectorFromPercentage(CarIdxLapDistPct[i]));

            return carSectorIdx;
        }
    }

	static int ToSectorFromPercentage(float percentage)
	{
		if (percentage > 0.66)
			return 2;

		else if (percentage > 0.33)
			return 1;

		return 0;
	}
}
