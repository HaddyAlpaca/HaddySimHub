// This file is part of iRacingSDK.
//
// Copyright 2014 Dean Netherton
// https://github.com/vipoo/iRacingSDK.Net
//
// iRacingSDK is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// iRacingSDK is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with iRacingSDK.  If not, see <http://www.gnu.org/licenses/>.

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
