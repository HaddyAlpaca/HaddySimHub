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
