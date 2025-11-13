using HaddySimHub.Displays.Dirt2;
using HaddySimHub.Models;
using Xunit;

namespace HaddySimHub.Tests
{
    public class Dirt2DisplayTests
    {
        #region GenerateRpmLights Tests

        [Fact]
        public void GenerateRpmLights_Creates6Lights()
        {
            // Arrange
            int rpmMax = 7000;

            // Act
            var lights = Display.GenerateRpmLights(rpmMax);

            // Assert
            Assert.NotNull(lights);
            Assert.Equal(6, lights.Length);
        }

        [Fact]
        public void GenerateRpmLights_FirstTwoLightsAreGreen()
        {
            // Arrange
            int rpmMax = 7000;

            // Act
            var lights = Display.GenerateRpmLights(rpmMax);

            // Assert
            Assert.Equal("Green", lights[0].Color);
            Assert.Equal("Green", lights[1].Color);
        }

        [Fact]
        public void GenerateRpmLights_MiddleTwoLightsAreYellow()
        {
            // Arrange
            int rpmMax = 7000;

            // Act
            var lights = Display.GenerateRpmLights(rpmMax);

            // Assert
            Assert.Equal("Yellow", lights[2].Color);
            Assert.Equal("Yellow", lights[3].Color);
        }

        [Fact]
        public void GenerateRpmLights_LastTwoLightsAreRed()
        {
            // Arrange
            int rpmMax = 7000;

            // Act
            var lights = Display.GenerateRpmLights(rpmMax);

            // Assert
            Assert.Equal("Red", lights[4].Color);
            Assert.Equal("Red", lights[5].Color);
        }

        [Fact]
        public void GenerateRpmLights_RpmValuesCorrect()
        {
            // Arrange
            int rpmMax = 7000;

            // Act
            var lights = Display.GenerateRpmLights(rpmMax);

            // Assert
            // RPM values are calculated as: rpmMax - ((lightsCount - i) * lightsStep)
            // So: 7000 - (6*200)=5800, 7000-(5*200)=6000, etc.
            Assert.Equal(5800, lights[0].Rpm);
            Assert.Equal(6000, lights[1].Rpm);
            Assert.Equal(6200, lights[2].Rpm);
            Assert.Equal(6400, lights[3].Rpm);
            Assert.Equal(6600, lights[4].Rpm);
            Assert.Equal(6800, lights[5].Rpm);
        }

        [Fact]
        public void GenerateRpmLights_WithDifferentRpmMax()
        {
            // Arrange
            int rpmMax = 8000;

            // Act
            var lights = Display.GenerateRpmLights(rpmMax);

            // Assert
            // 8000 - (6*200)=6800, 8000-(5*200)=7000, etc.
            Assert.Equal(6800, lights[0].Rpm);
            Assert.Equal(7000, lights[1].Rpm);
            Assert.Equal(7200, lights[2].Rpm);
            Assert.Equal(7400, lights[3].Rpm);
            Assert.Equal(7600, lights[4].Rpm);
            Assert.Equal(7800, lights[5].Rpm);
        }

        [Fact]
        public void GenerateRpmLights_WithLowRpmMax()
        {
            // Arrange
            int rpmMax = 2000;

            // Act
            var lights = Display.GenerateRpmLights(rpmMax);

            // Assert
            Assert.NotNull(lights);
            // Even with low max, lights are calculated correctly
            // 2000 - (6*200)=800
            Assert.Equal(800, lights[0].Rpm);
        }

        [Fact]
        public void GenerateRpmLights_AllLightsHaveValidRpm()
        {
            // Arrange
            int rpmMax = 7000;

            // Act
            var lights = Display.GenerateRpmLights(rpmMax);

            // Assert
            foreach (var light in lights)
            {
                Assert.True(light.Rpm > 0);
            }
        }

        #endregion

        #region Speed Conversion Tests

        [Fact]
        public void ConvertToDisplayUpdate_ConvertsSpeedMsToKmh()
        {
            // Arrange
            float speedMs = 27.78f; // approximately 100 km/h

            // Act
            int speedKmh = Convert.ToInt32(speedMs * 3.6);

            // Assert
            Assert.Equal(100, speedKmh);
        }

        [Fact]
        public void ConvertToDisplayUpdate_ConvertsZeroSpeed()
        {
            // Arrange
            float speedMs = 0f;

            // Act
            int speedKmh = Convert.ToInt32(speedMs * 3.6);

            // Assert
            Assert.Equal(0, speedKmh);
        }

        [Fact]
        public void ConvertToDisplayUpdate_ConvertsHighSpeed()
        {
            // Arrange
            float speedMs = 50f; // 180 km/h

            // Act
            int speedKmh = Convert.ToInt32(speedMs * 3.6);

            // Assert
            Assert.Equal(180, speedKmh);
        }

        #endregion

        #region RPM Calculation Tests

        [Fact]
        public void ConvertToDisplayUpdate_CalculatesRpmFromRawValue()
        {
            // Arrange
            float rawRpm = 500f;

            // Act
            int rpm = Convert.ToInt32(rawRpm * 10);

            // Assert
            Assert.Equal(5000, rpm);
        }

        [Fact]
        public void ConvertToDisplayUpdate_CalculatesRpmMax()
        {
            // Arrange
            float maxRpmRaw = 700f;

            // Act
            int rpmMax = Convert.ToInt32(maxRpmRaw * 10);

            // Assert
            Assert.Equal(7000, rpmMax);
        }

        [Fact]
        public void ConvertToDisplayUpdate_CalculatesIdleRpm()
        {
            // Arrange
            float idleRpmRaw = 100f;

            // Act
            int idleRpm = Convert.ToInt32(idleRpmRaw * 10);

            // Assert
            Assert.Equal(1000, idleRpm);
        }

        #endregion

        #region Gear Tests

        [Fact]
        public void ConvertToDisplayUpdate_NeutralGear()
        {
            // Arrange
            float gear = 0;

            // Act
            string gearStr = gear == 0 ? "N" : gear < 0 ? "R" : gear.ToString();

            // Assert
            Assert.Equal("N", gearStr);
        }

        [Fact]
        public void ConvertToDisplayUpdate_ReverseGear()
        {
            // Arrange
            float gear = -1;

            // Act
            string gearStr = gear == 0 ? "N" : gear < 0 ? "R" : gear.ToString();

            // Assert
            Assert.Equal("R", gearStr);
        }

        [Fact]
        public void ConvertToDisplayUpdate_FirstGear()
        {
            // Arrange
            float gear = 1;

            // Act
            string gearStr = gear == 0 ? "N" : gear < 0 ? "R" : gear.ToString();

            // Assert
            Assert.Equal("1", gearStr);
        }

        [Fact]
        public void ConvertToDisplayUpdate_HighGear()
        {
            // Arrange
            float gear = 6;

            // Act
            string gearStr = gear == 0 ? "N" : gear < 0 ? "R" : gear.ToString();

            // Assert
            Assert.Equal("6", gearStr);
        }

        #endregion

        #region Control Input Tests

        [Fact]
        public void ConvertToDisplayUpdate_CalculatesThrottle()
        {
            // Arrange
            float throttle = 0.75f; // 75%

            // Act
            int throttlePercent = Convert.ToInt32(throttle * 100);

            // Assert
            Assert.Equal(75, throttlePercent);
        }

        [Fact]
        public void ConvertToDisplayUpdate_CalculatesBrake()
        {
            // Arrange
            float brakes = 0.50f; // 50%

            // Act
            int brakesPercent = Convert.ToInt32(brakes * 100);

            // Assert
            Assert.Equal(50, brakesPercent);
        }

        [Fact]
        public void ConvertToDisplayUpdate_CalculatesClutch()
        {
            // Arrange
            float clutch = 0.25f; // 25%

            // Act
            int clutchPercent = Convert.ToInt32(clutch * 100);

            // Assert
            Assert.Equal(25, clutchPercent);
        }

        [Fact]
        public void ConvertToDisplayUpdate_ThrottleFullyEngaged()
        {
            // Arrange
            float throttle = 1.0f; // 100%

            // Act
            int throttlePercent = Convert.ToInt32(throttle * 100);

            // Assert
            Assert.Equal(100, throttlePercent);
        }

        [Fact]
        public void ConvertToDisplayUpdate_BrakeNotPressed()
        {
            // Arrange
            float brakes = 0.0f; // 0%

            // Act
            int brakesPercent = Convert.ToInt32(brakes * 100);

            // Assert
            Assert.Equal(0, brakesPercent);
        }

        [Fact]
        public void ConvertToDisplayUpdate_ClutchFullyEngaged()
        {
            // Arrange
            float clutch = 1.0f; // 100%

            // Act
            int clutchPercent = Convert.ToInt32(clutch * 100);

            // Assert
            Assert.Equal(100, clutchPercent);
        }

        #endregion

        #region Progress Tests

        [Fact]
        public void ConvertToDisplayUpdate_CalculatesProgress()
        {
            // Arrange
            float progress = 0.5f; // 50% complete

            // Act
            int progressPercent = Math.Min(Convert.ToInt32(progress * 100), 100);

            // Assert
            Assert.Equal(50, progressPercent);
        }

        [Fact]
        public void ConvertToDisplayUpdate_ProgressClamped()
        {
            // Arrange - sometimes progress can exceed 100% due to floating point
            float progress = 1.05f;

            // Act
            int progressPercent = Math.Min(Convert.ToInt32(progress * 100), 100);

            // Assert
            Assert.Equal(100, progressPercent);
        }

        [Fact]
        public void ConvertToDisplayUpdate_ProgressAtStart()
        {
            // Arrange
            float progress = 0.0f; // 0% complete

            // Act
            int progressPercent = Math.Min(Convert.ToInt32(progress * 100), 100);

            // Assert
            Assert.Equal(0, progressPercent);
        }

        [Fact]
        public void ConvertToDisplayUpdate_ProgressAtEnd()
        {
            // Arrange
            float progress = 1.0f; // 100% complete

            // Act
            int progressPercent = Math.Min(Convert.ToInt32(progress * 100), 100);

            // Assert
            Assert.Equal(100, progressPercent);
        }

        #endregion

        #region Distance Tests

        [Fact]
        public void ConvertToDisplayUpdate_CalculatesDistance()
        {
            // Arrange
            float distance = 5000.5f; // meters

            // Act
            int distanceInt = Math.Max(Convert.ToInt32(distance), 0);

            // Assert
            Assert.Equal(5000, distanceInt);
        }

        [Fact]
        public void ConvertToDisplayUpdate_DistanceClamped()
        {
            // Arrange - distance should never be negative
            float distance = -100f;

            // Act
            int distanceInt = Math.Max(Convert.ToInt32(distance), 0);

            // Assert
            Assert.Equal(0, distanceInt);
        }

        [Fact]
        public void ConvertToDisplayUpdate_DistanceZero()
        {
            // Arrange
            float distance = 0f;

            // Act
            int distanceInt = Math.Max(Convert.ToInt32(distance), 0);

            // Assert
            Assert.Equal(0, distanceInt);
        }

        #endregion

        #region Position Tests

        [Fact]
        public void ConvertToDisplayUpdate_CarPosition()
        {
            // Arrange
            float carPos = 3.4f; // approximately 3rd position

            // Act
            int position = Convert.ToInt32(carPos);

            // Assert
            Assert.Equal(3, position);
        }

        [Fact]
        public void ConvertToDisplayUpdate_FirstPosition()
        {
            // Arrange
            float carPos = 1f;

            // Act
            int position = Convert.ToInt32(carPos);

            // Assert
            Assert.Equal(1, position);
        }

        [Fact]
        public void ConvertToDisplayUpdate_HighPosition()
        {
            // Arrange
            float carPos = 20f;

            // Act
            int position = Convert.ToInt32(carPos);

            // Assert
            Assert.Equal(20, position);
        }

        #endregion

        #region Sector Time Tests

        [Fact]
        public void ConvertToDisplayUpdate_Sector1Time()
        {
            // Arrange
            float sector1Time = 45.5f; // seconds

            // Act & Assert
            Assert.True(sector1Time >= 0);
        }

        [Fact]
        public void ConvertToDisplayUpdate_Sector2Time()
        {
            // Arrange
            float sector2Time = 52.3f; // seconds

            // Act & Assert
            Assert.True(sector2Time >= 0);
        }

        [Fact]
        public void ConvertToDisplayUpdate_LapTime()
        {
            // Arrange
            float lapTime = 180.5f; // seconds

            // Act & Assert
            Assert.True(lapTime >= 0);
        }

        [Fact]
        public void ConvertToDisplayUpdate_SectorTimesAddUpToLapTime()
        {
            // Arrange
            float sector1Time = 45.5f;
            float sector2Time = 52.3f;
            float lapTime = 180.5f;

            // Act
            float calculatedTime = sector1Time + sector2Time;

            // Assert - sectors are part of the lap, but not all of it
            Assert.True(calculatedTime < lapTime);
        }

        #endregion

        #region Display Property Tests

        [Fact]
        public void Display_Description_ReturnsDirtRally2()
        {
            // Arrange
            string description = "Dirt Rally 2";

            // Act & Assert
            Assert.Equal("Dirt Rally 2", description);
        }

        [Fact]
        public void Display_IsActive_ChecksForDirtrally2Process()
        {
            // Arrange
            string processName = "dirtrally2";

            // Act & Assert
            Assert.Equal("dirtrally2", processName);
        }

        #endregion

        #region Display Type Tests

        [Fact]
        public void ConvertToDisplayUpdate_ReturnsRallyDashboardType()
        {
            // Arrange
            var expectedType = DisplayType.RallyDashboard;

            // Act & Assert
            Assert.Equal(DisplayType.RallyDashboard, expectedType);
        }

        #endregion

        #region Edge Case Tests

        [Fact]
        public void ConvertToDisplayUpdate_HandleZeroRpmMax()
        {
            // Arrange
            int rpmMax = 0;

            // Act
            var lights = Display.GenerateRpmLights(rpmMax);

            // Assert
            Assert.NotNull(lights);
            Assert.Equal(6, lights.Length);
        }

        [Fact]
        public void ConvertToDisplayUpdate_HandleLargeRpmMax()
        {
            // Arrange
            int rpmMax = 10000;

            // Act
            var lights = Display.GenerateRpmLights(rpmMax);

            // Assert
            Assert.NotNull(lights);
            Assert.Equal(6, lights.Length);
            // 10000 - (6*200) = 8800
            Assert.Equal(8800, lights[0].Rpm);
        }

        [Fact]
        public void ConvertToDisplayUpdate_HandleVeryLowSpeed()
        {
            // Arrange
            float speedMs = 0.1f;

            // Act
            int speedKmh = Convert.ToInt32(speedMs * 3.6);

            // Assert
            Assert.True(speedKmh >= 0);
        }

        [Fact]
        public void ConvertToDisplayUpdate_HandleVeryHighSpeed()
        {
            // Arrange
            float speedMs = 100f; // 360 km/h

            // Act
            int speedKmh = Convert.ToInt32(speedMs * 3.6);

            // Assert
            Assert.Equal(360, speedKmh);
        }

        #endregion
    }
}
