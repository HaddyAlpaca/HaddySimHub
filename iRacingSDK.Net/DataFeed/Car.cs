using iRacingSDK.Support;

namespace iRacingSDK;

public class Car
{
    readonly int carIdx;
    readonly Telemetry telemetry;
    readonly SessionData._DriverInfo._Drivers driver;
    public readonly CarDetails Details;

    public Car(Telemetry telemetry, int carIdx)
    {
        this.telemetry = telemetry;
        this.carIdx = carIdx;
        this.driver = telemetry.SessionData.DriverInfo.CompetingDrivers[carIdx];
        this.Details = new CarDetails(telemetry, carIdx);
    }

    public int Index { get { return carIdx; } }
    public int CarIdx { get { return carIdx; } }

    public int Lap { get { return telemetry.CarIdxLap[carIdx]; } }
    public float DistancePercentage { get { return telemetry.CarIdxLapDistPct[carIdx]; } }
    public float TotalDistance { get { return this.Lap + this.DistancePercentage; } }
    public LapSector LapSector { get { return telemetry.CarSectorIdx[carIdx]; } }
    public int Position { get { return telemetry.Positions[carIdx]; } }
    public int OfficialPostion { get { return telemetry.CarIdxPosition[carIdx]; } }
    public bool HasSeenCheckeredFlag { get { return telemetry.HasSeenCheckeredFlag != null ? telemetry.HasSeenCheckeredFlag[carIdx] : false; } }
    public bool HasData { get { return telemetry.HasData(carIdx); } }
    public bool HasRetired { get { return telemetry.HasRetired != null ? telemetry.HasRetired[carIdx] : true; } }
    public TrackLocation TrackSurface { get { return telemetry.CarIdxTrackSurface[carIdx]; } }
    public int PitStopCount { get { return telemetry.CarIdxPitStopCount[carIdx]; } }
    public bool IsInPits { get { return TrackSurface == TrackLocation.InPitStall; } }

    public SessionData._SessionInfo._Sessions._ResultsPositions ResultPosition
    {
        get
        {
            if (telemetry.Session.ResultsPositions == null)
                return null;

            return telemetry.Session.ResultsPositions.FirstOrDefault(rp => rp.CarIdx == carIdx);
        }
    }

    public TimeSpan LastTimeSpan
    {
        get { return TimeSpan.FromSeconds(LastTime); }
    }

    public double LastTime
    {
        get
        {
            var rp = ResultPosition;
            if (rp == null)
                return 0f;

            if( rp.LapsComplete != (Lap-1))
            {
                TraceInfo.WriteLine("Attempt to get LastTime from session data, with mismatch Lap counters.  Telemerty Lap: {0}.  Session LapComplete: {1}", Lap-1, rp.LapsComplete);
                return 0f;
            }

            return rp.LastTime;
        }
    }
}
