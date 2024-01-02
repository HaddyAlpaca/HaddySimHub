using HaddySimHub.GameData.Models;
using HaddySimHub.Logging;
using iRacingSDK;
using NSubstitute;
using static iRacingSDK.SessionData;

namespace HaddySimHub.iRacing.Test;

[TestClass]
public class GameDataReaderTest
{
    private readonly ILogger logger;
    private readonly GameDataReader sut;
    private DataSample sample;
    private SessionData sessionData;

    public GameDataReaderTest() 
    {
        this.logger = Substitute.For<ILogger>();
        this.sut = new GameDataReader(this.logger);

        //Set the default data sample
        this.sample = new DataSample
        {
            IsConnected = true,
            Telemetry = [],
        };

        this.sample.Telemetry.Add("SessionNum", 1);
        this.sample.Telemetry.Add("Lap", 1);
        this.sample.Telemetry.Add("PlayerCarDriverIncidentCount", 0);
        this.sample.Telemetry.Add("PlayerCarIdx", 4);
        this.sample.Telemetry.Add("SessionTimeRemain", (double)0);
        this.sample.Telemetry.Add("PlayerCarPosition", 0);
        this.sample.Telemetry.Add("LapCurrentLapTime", (float)0);
        this.sample.Telemetry.Add("LapLastLapTime", (float)0);
        this.sample.Telemetry.Add("LapBestLapTime", (float)0);
        this.sample.Telemetry.Add("Gear", 0);
        this.sample.Telemetry.Add("RPM", (float)0);
        this.sample.Telemetry.Add("Speed", (float)0);
        this.sample.Telemetry.Add("dcBrakeBias", (float)0);
        this.sample.Telemetry.Add("FuelLevel", (float)0);
        this.sample.Telemetry.Add("AirTemp", (float)0);
        this.sample.Telemetry.Add("TrackTemp", (float)0);
        this.sample.Telemetry.Add("SessionFlags", 0);
        this.sample.Telemetry.Add("EngineWarnings", 0);
        this.sample.Telemetry.Add("LapDistPct", (float)0);
        this.sample.Telemetry.Add("SessionTime", (double)0);

        this.sessionData = new SessionData() 
        {
            SessionInfo = new()
            {
                Sessions = [
                    new _SessionInfo._Sessions
                    {
                        SessionNum = 1,
                        SessionType = "Practice",
                        SessionLaps = "unlimited",
                        SessionTime = "unlimited",
                    }
                ]
            },
            SplitTimeInfo = new() 
            {
                Sectors = [
                    new _SplitTimeInfo._Sectors { SectorNum = 0, SectorStartPct = 0 },
                    new _SplitTimeInfo._Sectors { SectorNum = 1, SectorStartPct = 0.381897 },
                    new _SplitTimeInfo._Sectors { SectorNum = 2, SectorStartPct = 0.578883},
                ]
            },
            WeekendInfo = new() 
            {
                WeekendOptions = new()
                {
                    IncidentLimit = "17",
                }
            },
            DriverInfo = new()
            {
                Drivers = [
                    new _DriverInfo._Drivers { CarIdx = 1, UserName = "Opponent A", CarNumberRaw = 1 },
                    new _DriverInfo._Drivers { CarIdx = 2, UserName = "Opponent B", CarNumberRaw = 2 },
                    new _DriverInfo._Drivers { CarIdx = 3, UserName = "Opponent C", CarNumberRaw = 3 },
                    new _DriverInfo._Drivers { CarIdx = 4, UserName = "Player", CarNumberRaw = 33 },
                    new _DriverInfo._Drivers { CarIdx = 9, CarIsPaceCar = 1 },
                ]
            }
        };
    
        this.sample.SessionData = this.sessionData;
        this.sample.Telemetry.SessionData = this.sessionData;
    }

    [TestMethod]
    public void Object_of_incorrect_type_throws_an_exception()
    {
        Assert.ThrowsException<InvalidDataException>(() => this.sut.Convert(new object()));
    }

    [TestMethod]
    public void Object_of_correct_type_does_not_throw_an_exception()
    {
        this.sut.Convert(this.sample);
    }

#region Incidents tests
    [TestMethod]
    public void When_maximum_number_of_incidents_is_unlimited_999_is_returned()
    {
        this.sessionData.WeekendInfo.WeekendOptions.IncidentLimit = "unlimited";

        var raceData = (RaceData)this.sut.Convert(this.sample);
        Assert.AreEqual(999, raceData.MaxIncidents);
    }

    [TestMethod]
    public void Maximum_number_of_incidents_cannot_be_negative()
    {
        this.sessionData.WeekendInfo.WeekendOptions.IncidentLimit = "-1";

        var raceData = (RaceData)this.sut.Convert(this.sample);
        Assert.AreEqual(0, raceData.MaxIncidents);
    }

    [TestMethod]
    public void Incidents_cannot_be_negative()
    {
        this.sample.Telemetry["PlayerCarDriverIncidentCount"] = -1;

        var raceData = (RaceData)this.sut.Convert(this.sample);
        Assert.AreEqual(0, raceData.Incidents);
    }
#endregion

#region TrackPositions tests
    [TestMethod]
    public void Pace_car_is_not_included_when_in_pitlane()
    {
        var raceData = (RaceData)this.sut.Convert(this.sample);
        Assert.AreEqual(4, raceData.TrackPositions.Length);
    }
#endregion
}