using HaddySimHub.Displays.ACC;
using HaddySimHub.Models;

namespace HaddySimHub.Tests
{
    [TestClass]
    public class ACCDataConverterTests
    {
        [TestMethod]
        public void Convert_ReturnsRaceDashboard()
        {
            var converter = new ACCDataConverter();
            var telemetry = CreateMockTelemetry();

            var result = converter.Convert(telemetry);

            Assert.AreEqual(DisplayType.RaceDashboard, result.Type);
            Assert.IsInstanceOfType(result.Data, typeof(RaceData));
        }

        [TestMethod]
        public void Convert_GearNeutral()
        {
            var converter = new ACCDataConverter();
            var telemetry = CreateMockTelemetry(gear: 0);

            var result = converter.Convert(telemetry);
            var raceData = result.Data as RaceData;

            Assert.IsNotNull(raceData);
            Assert.AreEqual("N", raceData.Gear);
        }

        [TestMethod]
        public void Convert_GearReverse()
        {
            var converter = new ACCDataConverter();
            var telemetry = CreateMockTelemetry(gear: -1);

            var result = converter.Convert(telemetry);
            var raceData = result.Data as RaceData;

            Assert.IsNotNull(raceData);
            Assert.AreEqual("R", raceData.Gear);
        }

        [TestMethod]
        public void Convert_GearForward()
        {
            var converter = new ACCDataConverter();
            var telemetry = CreateMockTelemetry(gear: 2);

            var result = converter.Convert(telemetry);
            var raceData = result.Data as RaceData;

            Assert.IsNotNull(raceData);
            Assert.AreEqual("2", raceData.Gear);
        }

        [TestMethod]
        public void Convert_SpeedConversion()
        {
            var converter = new ACCDataConverter();
            var telemetry = CreateMockTelemetry(speedKmh: 108.0f);

            var result = converter.Convert(telemetry);
            var raceData = result.Data as RaceData;

            Assert.IsNotNull(raceData);
            Assert.AreEqual(108, raceData.Speed);
        }

        [TestMethod]
        public void Convert_RPM()
        {
            var converter = new ACCDataConverter();
            var telemetry = CreateMockTelemetry(rpms: 6000);

            var result = converter.Convert(telemetry);
            var raceData = result.Data as RaceData;

            Assert.IsNotNull(raceData);
            Assert.AreEqual(6000, raceData.Rpm);
        }

        [TestMethod]
        public void Convert_SessionTypePractice()
        {
            var converter = new ACCDataConverter();
            var telemetry = CreateMockTelemetry(sessionType: 0);

            var result = converter.Convert(telemetry);
            var raceData = result.Data as RaceData;

            Assert.IsNotNull(raceData);
            Assert.AreEqual("Practice", raceData.SessionType);
        }

        [TestMethod]
        public void Convert_SessionTypeQualifying()
        {
            var converter = new ACCDataConverter();
            var telemetry = CreateMockTelemetry(sessionType: 1);

            var result = converter.Convert(telemetry);
            var raceData = result.Data as RaceData;

            Assert.IsNotNull(raceData);
            Assert.AreEqual("Qualifying", raceData.SessionType);
        }

        [TestMethod]
        public void Convert_SessionTypeRace()
        {
            var converter = new ACCDataConverter();
            var telemetry = CreateMockTelemetry(sessionType: 2);

            var result = converter.Convert(telemetry);
            var raceData = result.Data as RaceData;

            Assert.IsNotNull(raceData);
            Assert.AreEqual("Race", raceData.SessionType);
        }

        [TestMethod]
        public void Convert_CurrentLap()
        {
            var converter = new ACCDataConverter();
            var telemetry = CreateMockTelemetry(currentLap: 7);

            var result = converter.Convert(telemetry);
            var raceData = result.Data as RaceData;

            Assert.IsNotNull(raceData);
            Assert.AreEqual(7, raceData.CurrentLap);
        }

        [TestMethod]
        public void Convert_TotalLaps()
        {
            var converter = new ACCDataConverter();
            var telemetry = CreateMockTelemetry(numberOfLaps: 30);

            var result = converter.Convert(telemetry);
            var raceData = result.Data as RaceData;

            Assert.IsNotNull(raceData);
            Assert.AreEqual(30, raceData.TotalLaps);
        }

        [TestMethod]
        public void Convert_FuelEstimatedLaps()
        {
            var converter = new ACCDataConverter();
            var telemetry = CreateMockTelemetry(fuelEstimatedLaps: 8.0f);

            var result = converter.Convert(telemetry);
            var raceData = result.Data as RaceData;

            Assert.IsNotNull(raceData);
            Assert.AreEqual(8.0f, raceData.FuelEstLaps);
        }

        [TestMethod]
        public void Convert_FuelPerLap()
        {
            var converter = new ACCDataConverter();
            var telemetry = CreateMockTelemetry(fuelPerLap: 2.5f);

            var result = converter.Convert(telemetry);
            var raceData = result.Data as RaceData;

            Assert.IsNotNull(raceData);
            Assert.AreEqual(2.5f, raceData.FuelAvgLap);
        }

        [TestMethod]
        public void Convert_Temperature()
        {
            var converter = new ACCDataConverter();
            var telemetry = CreateMockTelemetry(airTemp: 22.5f, roadTemp: 38.0f);

            var result = converter.Convert(telemetry);
            var raceData = result.Data as RaceData;

            Assert.IsNotNull(raceData);
            Assert.AreEqual(22.5f, raceData.AirTemp);
            Assert.AreEqual(38.0f, raceData.TrackTemp);
        }

        [TestMethod]
        public void Convert_CurrentLapTime()
        {
            var converter = new ACCDataConverter();
            var telemetry = CreateMockTelemetry(currentTimeMs: 125432);

            var result = converter.Convert(telemetry);
            var raceData = result.Data as RaceData;

            Assert.IsNotNull(raceData);
            Assert.AreEqual(125.432f, raceData.CurrentLapTime, 0.001f);
        }

        #region Helpers

        private ACCTelemetry CreateMockTelemetry(
            float speedKmh = 0,
            int rpms = 0,
            float maxRpm = 9000,
            int gear = 0,
            int sessionType = 0,
            int currentLap = 1,
            int numberOfLaps = 0,
            float fuelEstimatedLaps = 0,
            float fuelPerLap = 0,
            float airTemp = 20,
            float roadTemp = 30,
            int sessionTimeLeftMs = 0,
            int currentTimeMs = 0)
        {
            return new ACCTelemetry
            {
                SpeedKmh = speedKmh,
                Rpms = rpms,
                MaxRpm = maxRpm,
                Gear = gear,
                SessionType = sessionType,
                CurrentLap = currentLap,
                NumberOfLaps = numberOfLaps,
                FuelEstimatedLaps = fuelEstimatedLaps,
                FuelPerLap = fuelPerLap,
                AirTemp = airTemp,
                RoadTemp = roadTemp,
                SessionTimeLeftMs = sessionTimeLeftMs,
                CurrentTimeMs = currentTimeMs
            };
        }

        #endregion
    }
}
