using iRacingSDK.Support;

namespace iRacingSDK;

public class CarArray : IEnumerable<Car>
{
    private readonly Car[] cars;
    
    public CarArray(Telemetry telemetry)
    {
        var drivers = telemetry.SessionData.DriverInfo.CompetingDrivers;

        cars = new Car[drivers.Length];

        for (var i = 0; i < drivers.Length; i++)
            cars[i] = new Car(telemetry, i);
    }

    public Car this[long carIdx]
    {
        get
        {
            if (carIdx < 0)
                throw new Exception("Attempt to load car details for negative car index {0}".F(carIdx));

            if (carIdx >= cars.Length)
                throw new Exception("Attempt to load car details for unknown carIndex.  carIdx: {0}, maxNumber: {1}".F(carIdx, cars.Length - 1));

            return cars[carIdx];
        }
    }

    public IEnumerator<Car> GetEnumerator() => (cars as IEnumerable<Car>).GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => cars.GetEnumerator();
}
