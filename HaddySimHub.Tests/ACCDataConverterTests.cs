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
            // Arrange
            var converter = new ACCDataConverter();
            var telemetry = CreateMockTelemetry();

            // Act
            var result = converter.Convert(telemetry);

            // Assert
            Assert.AreEqual(DisplayType.RaceDashboard, result.Type);
            Assert.IsInstanceOfType(result.Data, typeof(RaceData));
        }

        [TestMethod]
        public void Convert_GearNeutral()
        {
            // Arrange
            var converter = new ACCDataConverter();
            var telemetry = CreateMockTelemetry(gear: 0);

            // Act
            var result = converter.Convert(telemetry);
            var raceData = result.Data as RaceData;

            // Assert
            Assert.IsNotNull(raceData);
            Assert.AreEqual("N", raceData.Gear);
        }

        [TestMethod]
        public void Convert_GearReverse()
        {
            // Arrange
            var converter = new ACCDataConverter();
            var telemetry = CreateMockTelemetry(gear: -1);

            // Act
            var result = converter.Convert(telemetry);
            var raceData = result.Data as RaceData;

            // Assert
            Assert.IsNotNull(raceData);
            Assert.AreEqual("R", raceData.Gear);
        }

        [TestMethod]
        public void Convert_GearForward()
        {
            // Arrange
            var converter = new ACCDataConverter();
            var telemetry = CreateMockTelemetry(gear: 2);

            // Act
            var result = converter.Convert(telemetry);
            var raceData = result.Data as RaceData;

            // Assert
            Assert.IsNotNull(raceData);
            Assert.AreEqual("2", raceData.Gear);
        }

        [TestMethod]
        public void Convert_SpeedConversion()
        {
            // Arrange
            var converter = new ACCDataConverter();
            var telemetry = CreateMockTelemetry(speedMs: 30.0f); // 108 km/h

            // Act
            var result = converter.Convert(telemetry);
            var raceData = result.Data as RaceData;

            // Assert
            Assert.IsNotNull(raceData);
            Assert.AreEqual(108, raceData.Speed);
        }

        [TestMethod]
        public void Convert_RPM()
        {
            // Arrange
            var converter = new ACCDataConverter();
            var telemetry = CreateMockTelemetry(rpm: 6000);

            // Act
            var result = converter.Convert(telemetry);
            var raceData = result.Data as RaceData;

            // Assert
            Assert.IsNotNull(raceData);
            Assert.AreEqual(6000, raceData.Rpm);
        }

        [TestMethod]
        public void Convert_SessionTypePractice()
        {
            // Arrange
            var converter = new ACCDataConverter();
            var telemetry = CreateMockTelemetry(sessionType: 0);

            // Act
            var result = converter.Convert(telemetry);
            var raceData = result.Data as RaceData;

            // Assert
            Assert.IsNotNull(raceData);
            Assert.AreEqual("Practice", raceData.SessionType);
        }

        [TestMethod]
        public void Convert_SessionTypeQualifying()
        {
            // Arrange
            var converter = new ACCDataConverter();
            var telemetry = CreateMockTelemetry(sessionType: 1);

            // Act
            var result = converter.Convert(telemetry);
            var raceData = result.Data as RaceData;

            // Assert
            Assert.IsNotNull(raceData);
            Assert.AreEqual("Qualifying", raceData.SessionType);
        }

        [TestMethod]
        public void Convert_SessionTypeRace()
        {
            // Arrange
            var converter = new ACCDataConverter();
            var telemetry = CreateMockTelemetry(sessionType: 2);

            // Act
            var result = converter.Convert(telemetry);
            var raceData = result.Data as RaceData;

            // Assert
            Assert.IsNotNull(raceData);
            Assert.AreEqual("Race", raceData.SessionType);
        }

        [TestMethod]
        public void Convert_CurrentLap()
        {
            // Arrange
            var converter = new ACCDataConverter();
            var telemetry = CreateMockTelemetry(currentLapCount: 7);

            // Act
            var result = converter.Convert(telemetry);
            var raceData = result.Data as RaceData;

            // Assert
            Assert.IsNotNull(raceData);
            Assert.AreEqual(7, raceData.CurrentLap);
        }

        [TestMethod]
        public void Convert_TotalLaps()
        {
            // Arrange
            var converter = new ACCDataConverter();
            var telemetry = CreateMockTelemetry(maxLaps: 30);

            // Act
            var result = converter.Convert(telemetry);
            var raceData = result.Data as RaceData;

            // Assert
            Assert.IsNotNull(raceData);
            Assert.AreEqual(30, raceData.TotalLaps);
        }

        [TestMethod]
        public void Convert_FuelEstimatedLaps()
        {
            // Arrange
            var converter = new ACCDataConverter();
            var telemetry = CreateMockTelemetry(fuelEstimatedLaps: 8.0f);

            // Act
            var result = converter.Convert(telemetry);
            var raceData = result.Data as RaceData;

            // Assert
            Assert.IsNotNull(raceData);
            Assert.AreEqual(8.0f, raceData.FuelEstLaps);
        }

        [TestMethod]
        public void Convert_FuelAutoConsumption()
        {
            // Arrange
            var converter = new ACCDataConverter();
            var telemetry = CreateMockTelemetry(fuelAutoConsumption: 2.5f);

            // Act
            var result = converter.Convert(telemetry);
            var raceData = result.Data as RaceData;

            // Assert
            Assert.IsNotNull(raceData);
            Assert.AreEqual(2.5f, raceData.FuelAvgLap);
        }

        [TestMethod]
        public void Convert_Temperature()
        {
            // Arrange
            var converter = new ACCDataConverter();
            var telemetry = CreateMockTelemetry(airTemp: 22.5f, roadTemp: 38.0f);

            // Act
            var result = converter.Convert(telemetry);
            var raceData = result.Data as RaceData;

            // Assert
            Assert.IsNotNull(raceData);
            Assert.AreEqual(22.5f, raceData.AirTemp);
            Assert.AreEqual(38.0f, raceData.TrackTemp);
        }

        [TestMethod]
        public void Convert_LapTime()
        {
            // Arrange
            var converter = new ACCDataConverter();
            var telemetry = CreateMockTelemetry(lapTimeMs: 125432); // 125.432 seconds

            // Act
            var result = converter.Convert(telemetry);
            var raceData = result.Data as RaceData;

            // Assert
            Assert.IsNotNull(raceData);
            Assert.AreEqual(125.432f, raceData.CurrentLapTime, 0.001f);
        }

        #region Helpers

        private ACCTelemetry CreateMockTelemetry(
            float speedMs = 0,
            float rpm = 0,
            float maxRpm = 9000,
            float gear = 0,
            int sessionType = 0,
            int currentLapCount = 1,
            int maxLaps = 0,
            float fuelEstimatedLaps = 0,
            float fuelAutoConsumption = 0,
            float airTemp = 20,
            float roadTemp = 30,
            int sessionTimeLeftMs = 0,
            int lapTimeMs = 0)
        {
            return new ACCTelemetry
            {
                SpeedMs = speedMs,
                Rpm = rpm,
                MaxRpm = maxRpm,
                Gear = gear,
                SessionType = sessionType,
                CurrentLapCount = currentLapCount,
                MaxLaps = maxLaps,
                FuelEstimatedLaps = fuelEstimatedLaps,
                FuelAutoConsumption = fuelAutoConsumption,
                AirTemp = airTemp,
                RoadTemp = roadTemp,
                SessionTimeLeftMs = sessionTimeLeftMs,
                LapTimeMs = lapTimeMs
            };
        }

        #endregion
    }
}
