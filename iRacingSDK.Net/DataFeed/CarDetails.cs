namespace iRacingSDK;

public class CarDetails
{
    readonly int carIdx;
    readonly Telemetry telemetry;
    readonly SessionData._DriverInfo._Drivers driver;

    public CarDetails(Telemetry telemetry, int carIdx)
    {
        this.telemetry = telemetry;
        this.carIdx = carIdx;
        this.driver = telemetry.SessionData.DriverInfo.CompetingDrivers[carIdx];
    }

    public int Index { get { return carIdx; } }
    public int CarIdx { get { return carIdx; } }
    public SessionData._DriverInfo._Drivers Driver { get { return driver; } }
    public string CarNumberDisplay { get { return driver == null ? "" : driver.CarNumber; } }
    public short CarNumberRaw { get { return driver == null ? (short)-1 : (short)driver.CarNumberRaw; } }
    public string UserName { get { return driver == null ? "Unknown" : driver.UserName; } }
    public bool IsPaceCar { get { return driver == null ? carIdx == 0 : driver.CarIsPaceCar == 1; } }
    public bool IsOnPitRoad { get { return telemetry.OnPitRoad; } }
    

    public Car Car(DataSample data)
    {
        return data.Telemetry.Cars[this.carIdx];
    }
}
