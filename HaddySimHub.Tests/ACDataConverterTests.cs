using HaddySimHub.Displays.AC;
using HaddySimHub.Models;

namespace HaddySimHub.Tests
{
    [TestClass]
    public class ACDataConverterTests
    {
        [TestMethod]
        public void Convert_ReturnsRaceDashboard()
        {
            // Arrange
            var converter = new ACDataConverter();
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
            var converter = new ACDataConverter();
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
            var converter = new ACDataConverter();
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
            var converter = new ACDataConverter();
            var telemetry = CreateMockTelemetry(gear: 3);

            // Act
            var result = converter.Convert(telemetry);
            var raceData = result.Data as RaceData;

            // Assert
            Assert.IsNotNull(raceData);
            Assert.AreEqual("3", raceData.Gear);
        }

        [TestMethod]
        public void Convert_SpeedConversion()
        {
            // Arrange
            var converter = new ACDataConverter();
            var telemetry = CreateMockTelemetry(speedMps: 25.0f); // 90 km/h

            // Act
            var result = converter.Convert(telemetry);
            var raceData = result.Data as RaceData;

            // Assert
            Assert.IsNotNull(raceData);
            Assert.AreEqual(90, raceData.Speed);
        }

        [TestMethod]
        public void Convert_RPM()
        {
            // Arrange
            var converter = new ACDataConverter();
            var telemetry = CreateMockTelemetry(rpm: 5500);

            // Act
            var result = converter.Convert(telemetry);
            var raceData = result.Data as RaceData;

            // Assert
            Assert.IsNotNull(raceData);
            Assert.AreEqual(5500, raceData.Rpm);
        }

        [TestMethod]
        public void Convert_SessionTypePractice()
        {
            // Arrange
            var converter = new ACDataConverter();
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
            var converter = new ACDataConverter();
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
            var converter = new ACDataConverter();
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
            var converter = new ACDataConverter();
            var telemetry = CreateMockTelemetry(currentLap: 5);

            // Act
            var result = converter.Convert(telemetry);
            var raceData = result.Data as RaceData;

            // Assert
            Assert.IsNotNull(raceData);
            Assert.AreEqual(5, raceData.CurrentLap);
        }

        [TestMethod]
        public void Convert_TotalLaps()
        {
            // Arrange
            var converter = new ACDataConverter();
            var telemetry = CreateMockTelemetry(totalLaps: 20);

            // Act
            var result = converter.Convert(telemetry);
            var raceData = result.Data as RaceData;

            // Assert
            Assert.IsNotNull(raceData);
            Assert.AreEqual(20, raceData.TotalLaps);
        }

        [TestMethod]
        public void Convert_FuelEstimatedLaps()
        {
            // Arrange
            var converter = new ACDataConverter();
            var telemetry = CreateMockTelemetry(fuelEstimatedLaps: 5.5f);

            // Act
            var result = converter.Convert(telemetry);
            var raceData = result.Data as RaceData;

            // Assert
            Assert.IsNotNull(raceData);
            Assert.AreEqual(5.5f, raceData.FuelEstLaps);
        }

        [TestMethod]
        public void Convert_MapsPedalAndPitLimiterInputs()
        {
            // Arrange
            var converter = new ACDataConverter();
            var telemetry = CreateMockTelemetry(throttleInput: 0.5f, brakeInput: 0.25f, clutchInput: 1f, pitLimiterOn: 1);

            // Act
            var result = converter.Convert(telemetry);
            var raceData = result.Data as RaceData;

            // Assert
            Assert.IsNotNull(raceData);
            Assert.AreEqual(50, raceData.ThrottlePct);
            Assert.AreEqual(25, raceData.BrakePct);
            Assert.AreEqual(100, raceData.ClutchPct);
            Assert.IsTrue(raceData.PitLimiterOn);
        }

        [TestMethod]
        public void Convert_UnavailableFieldsAreNull()
        {
            // Arrange
            var converter = new ACDataConverter();
            var telemetry = CreateMockTelemetry();

            // Act
            var result = converter.Convert(telemetry);
            var raceData = result.Data as RaceData;

            // Assert: AC shared memory does not expose these, so they must be null (hidden in the UI)
            Assert.IsNotNull(raceData);
            Assert.IsNull(raceData.Position);
            Assert.IsNull(raceData.FuelRemaining);
            Assert.IsNull(raceData.FuelAvgLap);
            Assert.IsNull(raceData.FuelLastLap);
            Assert.IsNull(raceData.BestLapTime);
            Assert.IsNull(raceData.BestLapTimeDelta);
            Assert.IsNull(raceData.LastLapTimeDelta);
        }

        [TestMethod]
        public void Convert_Temperature()
        {
            // Arrange
            var converter = new ACDataConverter();
            var telemetry = CreateMockTelemetry(airTemp: 25.5f, roadTemp: 35.0f);

            // Act
            var result = converter.Convert(telemetry);
            var raceData = result.Data as RaceData;

            // Assert
            Assert.IsNotNull(raceData);
            Assert.AreEqual(25.5f, raceData.AirTemp);
            Assert.AreEqual(35.0f, raceData.TrackTemp);
        }

        #region Helpers

        private ACTelemetry CreateMockTelemetry(
            float speedMps = 0,
            float rpm = 0,
            float maxRpm = 8000,
            float gear = 0,
            int sessionType = 0,
            int currentLap = 1,
            int totalLaps = 0,
            float fuelEstimatedLaps = 0,
            float airTemp = 20,
            float roadTemp = 30,
            int sessionTimeLeft = 0,
            int currentLapTime = 0,
            int lastLapTime = 0,
            float throttleInput = 0,
            float brakeInput = 0,
            float clutchInput = 0,
            float pitLimiterOn = 0)
        {
            return new ACTelemetry
            {
                SpeedMps = speedMps,
                Rpm = rpm,
                MaxRpm = maxRpm,
                Gear = gear,
                SessionType = sessionType,
                CurrentLap = currentLap,
                TotalLaps = totalLaps,
                FuelEstimatedLaps = fuelEstimatedLaps,
                AirTemp = airTemp,
                RoadTemp = roadTemp,
                SessionTimeLeft = sessionTimeLeft,
                CurrentLapTime = currentLapTime,
                LastLapTime = lastLapTime,
                ThrottleInput = throttleInput,
                BrakeInput = brakeInput,
                ClutchInput = clutchInput,
                PitLimiterOn = pitLimiterOn
            };
        }

        #endregion
    }
}
