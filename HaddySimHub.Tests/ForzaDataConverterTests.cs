using HaddySimHub.Displays.Forza;
using HaddySimHub.Models;
using System.Runtime.InteropServices;

namespace HaddySimHub.Tests;

[TestClass]
public class ForzaDataConverterTests
{
    [TestMethod]
    public void Telemetry_MatchesForzaHorizon5CarDashPacketSize()
    {
        Assert.AreEqual(323, Marshal.SizeOf<ForzaTelemetry>());
    }

    [TestMethod]
    public void Convert_MapsTelemetryToRaceDashboard()
    {
        var converter = new ForzaDataConverter();
        var telemetry = new ForzaTelemetry
        {
            IsRaceOn = 1,
            Speed = 50,
            CurrentEngineRpm = 4200,
            EngineMaxRpm = 7000,
            Gear = 3,
            Throttle = 128,
            Brake = 64,
            LapNumber = 4,
            RacePosition = 2,
            CurrentLap = 12.5f,
            CarOrdinal = 123,
            CarClass = 7,
            CarPerformanceIndex = 999,
            DrivetrainType = 2,
            NumCylinders = 8,
            Power = 500000,
            Torque = 700,
        };

        var result = converter.Convert(telemetry);
        var raceData = result.Data as RaceData;

        Assert.AreEqual(DisplayType.RaceDashboard, result.Type);
        Assert.IsNotNull(raceData);
        Assert.AreEqual("Race", raceData.SessionType);
        Assert.AreEqual(180, raceData.Speed);
        Assert.AreEqual("2", raceData.Gear);
        Assert.AreEqual(4200, raceData.Rpm);
        Assert.AreEqual(4, raceData.CurrentLap);
        Assert.AreEqual(2, raceData.Position);
        Assert.AreEqual(50, raceData.ThrottlePct);
        Assert.AreEqual(25, raceData.BrakePct);
        Assert.AreEqual(12.5f, raceData.CurrentLapTime);
        Assert.AreEqual(123, raceData.CarOrdinal);
        Assert.AreEqual(7, raceData.CarClass);
        Assert.AreEqual(999, raceData.CarPerformanceIndex);
        Assert.AreEqual(2, raceData.DrivetrainType);
        Assert.AreEqual(8, raceData.NumCylinders);
        Assert.AreEqual(500000, raceData.Power);
        Assert.AreEqual(700, raceData.Torque);
    }

    [TestMethod]
    public void Convert_MapsForzaGearValues()
    {
        var converter = new ForzaDataConverter();

        Assert.AreEqual("R", ((RaceData)converter.Convert(new ForzaTelemetry { Gear = 0 }).Data!).Gear);
        Assert.AreEqual("N", ((RaceData)converter.Convert(new ForzaTelemetry { Gear = 1 }).Data!).Gear);
        Assert.AreEqual("1", ((RaceData)converter.Convert(new ForzaTelemetry { Gear = 2 }).Data!).Gear);
    }
}
