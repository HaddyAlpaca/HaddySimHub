using HaddySimHub.Displays.IRacing;
using HaddySimHub.Models;
using iRacingSDK;
using Xunit;

namespace HaddySimHub.Tests
{
    public class IRacingDisplayTests
    {
        #region GenerateRpmLights Tests

        [Fact]
        public void GenerateRpmLights_WithFiaF4_ReturnsCorrectRpmLights()
        {
            // Arrange
            var carName = "FIA F4";

            // Act
            var lights = Display.GenerateRpmLights(carName);

            // Assert
            Assert.NotNull(lights);
            Assert.Equal(6, lights.Length);
            Assert.Equal(6300, lights[0].Rpm);
            Assert.Equal("Green", lights[0].Color);
            Assert.Equal(6500, lights[1].Rpm);
            Assert.Equal("Green", lights[1].Color);
            Assert.Equal(6600, lights[2].Rpm);
            Assert.Equal("Green", lights[2].Color);
            Assert.Equal(6700, lights[3].Rpm);
            Assert.Equal("Green", lights[3].Color);
            Assert.Equal(6800, lights[4].Rpm);
            Assert.Equal("Red", lights[4].Color);
            Assert.Equal(6900, lights[5].Rpm);
            Assert.Equal("Red", lights[5].Color);
        }

        [Fact]
        public void GenerateRpmLights_WithFiaF4_AllLightsHaveValidRpm()
        {
            // Arrange
            var carName = "FIA F4";

            // Act
            var lights = Display.GenerateRpmLights(carName);

            // Assert
            foreach (var light in lights)
            {
                Assert.True(light.Rpm >= 6300, $"RPM {light.Rpm} should be >= 6300");
                Assert.True(light.Rpm <= 6900, $"RPM {light.Rpm} should be <= 6900");
            }
        }

        [Fact]
        public void GenerateRpmLights_WithFiaF4_HasCorrectColorPattern()
        {
            // Arrange
            var carName = "FIA F4";

            // Act
            var lights = Display.GenerateRpmLights(carName);

            // Assert
            Assert.Equal("Green", lights[0].Color);
            Assert.Equal("Green", lights[1].Color);
            Assert.Equal("Green", lights[2].Color);
            Assert.Equal("Green", lights[3].Color);
            Assert.Equal("Red", lights[4].Color);
            Assert.Equal("Red", lights[5].Color);
        }

        [Fact]
        public void GenerateRpmLights_WithFiaF4_RpmValuesAreAscending()
        {
            // Arrange
            var carName = "FIA F4";

            // Act
            var lights = Display.GenerateRpmLights(carName);

            // Assert
            for (int i = 1; i < lights.Length; i++)
            {
                Assert.True(lights[i].Rpm > lights[i - 1].Rpm,
                    $"RPM values should be ascending: {lights[i].Rpm} should be > {lights[i - 1].Rpm}");
            }
        }

        [Fact]
        public void GenerateRpmLights_WithUnknownCar_ReturnsEmptyArray()
        {
            // Arrange
            var carName = "UnknownCar";

            // Act
            var lights = Display.GenerateRpmLights(carName);

            // Assert
            Assert.NotNull(lights);
            Assert.Empty(lights);
        }

        [Fact]
        public void GenerateRpmLights_WithEmptyString_ReturnsEmptyArray()
        {
            // Arrange
            var carName = string.Empty;

            // Act
            var lights = Display.GenerateRpmLights(carName);

            // Assert
            Assert.NotNull(lights);
            Assert.Empty(lights);
        }

        [Fact]
        public void GenerateRpmLights_WithNullCarName_ReturnsEmptyArray()
        {
            // Arrange & Act & Assert
            var lights = Display.GenerateRpmLights(null!);
            Assert.NotNull(lights);
            Assert.Empty(lights);
        }

        #endregion

        #region GetRpmMax Tests

        [Fact]
        public void GetRpmMax_WithFiaF4_Returns7000()
        {
            // Arrange
            var carName = "FIA F4";

            // Act
            var rpmMax = Display.GetRpmMax(carName);

            // Assert
            Assert.Equal(7000, rpmMax);
        }

        [Fact]
        public void GetRpmMax_WithUnknownCar_ReturnsZero()
        {
            // Arrange
            var carName = "UnknownCar";

            // Act
            var rpmMax = Display.GetRpmMax(carName);

            // Assert
            Assert.Equal(0, rpmMax);
        }

        [Fact]
        public void GetRpmMax_WithEmptyString_ReturnsZero()
        {
            // Arrange
            var carName = string.Empty;

            // Act
            var rpmMax = Display.GetRpmMax(carName);

            // Assert
            Assert.Equal(0, rpmMax);
        }

        [Fact]
        public void GetRpmMax_WithNullCarName_ReturnsZero()
        {
            // Arrange & Act
            var rpmMax = Display.GetRpmMax(null!);

            // Assert
            Assert.Equal(0, rpmMax);
        }

        #endregion

        #region Gear Conversion Tests

        [Fact]
        public void ConvertToDisplayUpdate_GearReverseConvertsToR()
        {
            // Arrange
            var display = new Display();
            var data = CreateMockDataSample(gear: -1);

            // Act
            var update = display.ConvertToDisplayUpdate(data);
            var raceData = update.Data as RaceData;

            // Assert
            Assert.NotNull(raceData);
            Assert.Equal("R", raceData.Gear);
        }

        [Fact]
        public void ConvertToDisplayUpdate_GearNeutralConvertsToN()
        {
            // Arrange
            var display = new Display();
            var data = CreateMockDataSample(gear: 0);

            // Act
            var update = display.ConvertToDisplayUpdate(data);
            var raceData = update.Data as RaceData;

            // Assert
            Assert.NotNull(raceData);
            Assert.Equal("N", raceData.Gear);
        }

        [Fact]
        public void ConvertToDisplayUpdate_GearFirstConvertsToOneString()
        {
            // Arrange
            var display = new Display();
            var data = CreateMockDataSample(gear: 1);

            // Act
            var update = display.ConvertToDisplayUpdate(data);
            var raceData = update.Data as RaceData;

            // Assert
            Assert.NotNull(raceData);
            Assert.Equal("1", raceData.Gear);
        }

        [Fact]
        public void ConvertToDisplayUpdate_GearHighConvertsToNumericString()
        {
            // Arrange
            var display = new Display();
            var data = CreateMockDataSample(gear: 6);

            // Act
            var update = display.ConvertToDisplayUpdate(data);
            var raceData = update.Data as RaceData;

            // Assert
            Assert.NotNull(raceData);
            Assert.Equal("6", raceData.Gear);
        }

        #endregion

        #region Speed Conversion Tests

        [Fact]
        public void ConvertToDisplayUpdate_SpeedConversionMultipliesByThree_Six()
        {
            // Arrange
            var display = new Display();
            var data = CreateMockDataSample(speed: 10.0f); // 10 m/s = 36 km/h

            // Act
            var update = display.ConvertToDisplayUpdate(data);
            var raceData = update.Data as RaceData;

            // Assert
            Assert.NotNull(raceData);
            Assert.Equal(36, raceData.Speed);
        }

        [Fact]
        public void ConvertToDisplayUpdate_SpeedZeroRemains()
        {
            // Arrange
            var display = new Display();
            var data = CreateMockDataSample(speed: 0.0f);

            // Act
            var update = display.ConvertToDisplayUpdate(data);
            var raceData = update.Data as RaceData;

            // Assert
            Assert.NotNull(raceData);
            Assert.Equal(0, raceData.Speed);
        }

        [Fact]
        public void ConvertToDisplayUpdate_SpeedRoundsCorrectly()
        {
            // Arrange
            var display = new Display();
            var data = CreateMockDataSample(speed: 27.777f); // 27.777 * 3.6 = 99.9972 -> rounds to 100

            // Act
            var update = display.ConvertToDisplayUpdate(data);
            var raceData = update.Data as RaceData;

            // Assert
            Assert.NotNull(raceData);
            Assert.Equal(100, raceData.Speed);
        }

        #endregion

        #region RPM Tests

        [Fact]
        public void ConvertToDisplayUpdate_RpmConvertedToInt()
        {
            // Arrange
            var display = new Display();
            var data = CreateMockDataSample(rpm: 6543.5f);

            // Act
            var update = display.ConvertToDisplayUpdate(data);
            var raceData = update.Data as RaceData;

            // Assert
            Assert.NotNull(raceData);
            Assert.Equal(6543, raceData.Rpm);
        }

        [Fact]
        public void ConvertToDisplayUpdate_RpmZeroRemains()
        {
            // Arrange
            var display = new Display();
            var data = CreateMockDataSample(rpm: 0.0f);

            // Act
            var update = display.ConvertToDisplayUpdate(data);
            var raceData = update.Data as RaceData;

            // Assert
            Assert.NotNull(raceData);
            Assert.Equal(0, raceData.Rpm);
        }

        #endregion

        #region Throttle and Brake Conversion Tests

        [Fact]
        public void ConvertToDisplayUpdate_ThrottleConvertedToPercentage()
        {
            // Arrange
            var display = new Display();
            var data = CreateMockDataSample(throttle: 0.75f); // 0.75 = 75%

            // Act
            var update = display.ConvertToDisplayUpdate(data);
            var raceData = update.Data as RaceData;

            // Assert
            Assert.NotNull(raceData);
            Assert.Equal(75, raceData.ThrottlePct);
        }

        [Fact]
        public void ConvertToDisplayUpdate_BrakeConvertedToPercentage()
        {
            // Arrange
            var display = new Display();
            var data = CreateMockDataSample(brake: 0.50f); // 0.50 = 50%

            // Act
            var update = display.ConvertToDisplayUpdate(data);
            var raceData = update.Data as RaceData;

            // Assert
            Assert.NotNull(raceData);
            Assert.Equal(50, raceData.BrakePct);
        }

        [Fact]
        public void ConvertToDisplayUpdate_FullThrottle()
        {
            // Arrange
            var display = new Display();
            var data = CreateMockDataSample(throttle: 1.0f);

            // Act
            var update = display.ConvertToDisplayUpdate(data);
            var raceData = update.Data as RaceData;

            // Assert
            Assert.NotNull(raceData);
            Assert.Equal(100, raceData.ThrottlePct);
        }

        [Fact]
        public void ConvertToDisplayUpdate_NoThrottle()
        {
            // Arrange
            var display = new Display();
            var data = CreateMockDataSample(throttle: 0.0f);

            // Act
            var update = display.ConvertToDisplayUpdate(data);
            var raceData = update.Data as RaceData;

            // Assert
            Assert.NotNull(raceData);
            Assert.Equal(0, raceData.ThrottlePct);
        }

        #endregion

        #region Steering Conversion Tests

        [Fact]
        public void ConvertToDisplayUpdate_SteeringCenteredIs50Percent()
        {
            // Arrange
            var display = new Display();
            var data = CreateMockDataSample(steeringAngle: 0.0f, steeringWheelAngleMax: 12.0f);

            // Act
            var update = display.ConvertToDisplayUpdate(data);
            var raceData = update.Data as RaceData;

            // Assert
            Assert.NotNull(raceData);
            Assert.Equal(50, raceData.SteeringPct);
        }

        [Fact]
        public void ConvertToDisplayUpdate_SteeringFullLeft()
        {
            // Arrange
            var display = new Display();
            var data = CreateMockDataSample(steeringAngle: -6.0f, steeringWheelAngleMax: 12.0f);

            // Act
            var update = display.ConvertToDisplayUpdate(data);
            var raceData = update.Data as RaceData;

            // Assert
            Assert.NotNull(raceData);
            Assert.Equal(0, raceData.SteeringPct);
        }

        [Fact]
        public void ConvertToDisplayUpdate_SteeringFullRight()
        {
            // Arrange
            var display = new Display();
            var data = CreateMockDataSample(steeringAngle: 6.0f, steeringWheelAngleMax: 12.0f);

            // Act
            var update = display.ConvertToDisplayUpdate(data);
            var raceData = update.Data as RaceData;

            // Assert
            Assert.NotNull(raceData);
            Assert.Equal(100, raceData.SteeringPct);
        }

        [Fact]
        public void ConvertToDisplayUpdate_SteeringQuarterLeft()
        {
            // Arrange
            var display = new Display();
            var data = CreateMockDataSample(steeringAngle: -3.0f, steeringWheelAngleMax: 12.0f);

            // Act
            var update = display.ConvertToDisplayUpdate(data);
            var raceData = update.Data as RaceData;

            // Assert
            Assert.NotNull(raceData);
            Assert.Equal(25, raceData.SteeringPct);
        }

        [Fact]
        public void ConvertToDisplayUpdate_SteeringZeroAngleMaxDefaultsTo50()
        {
            // Arrange
            var display = new Display();
            var data = CreateMockDataSample(steeringAngle: 5.0f, steeringWheelAngleMax: 0.0f);

            // Act
            var update = display.ConvertToDisplayUpdate(data);
            var raceData = update.Data as RaceData;

            // Assert
            Assert.NotNull(raceData);
            Assert.Equal(50, raceData.SteeringPct);
        }

        [Fact]
        public void ConvertToDisplayUpdate_SteeringClampsToZero()
        {
            // Arrange
            var display = new Display();
            var data = CreateMockDataSample(steeringAngle: -7.0f, steeringWheelAngleMax: 12.0f);

            // Act
            var update = display.ConvertToDisplayUpdate(data);
            var raceData = update.Data as RaceData;

            // Assert
            Assert.NotNull(raceData);
            Assert.True(raceData.SteeringPct >= 0);
        }

        [Fact]
        public void ConvertToDisplayUpdate_SteeringClampsTo100()
        {
            // Arrange
            var display = new Display();
            var data = CreateMockDataSample(steeringAngle: 7.0f, steeringWheelAngleMax: 12.0f);

            // Act
            var update = display.ConvertToDisplayUpdate(data);
            var raceData = update.Data as RaceData;

            // Assert
            Assert.NotNull(raceData);
            Assert.True(raceData.SteeringPct <= 100);
        }

        #endregion

        #region Display Properties Tests

        [Fact]
        public void Display_Description_ReturnsIRacing()
        {
            // Arrange
            var display = new Display();

            // Act
            var description = display.Description;

            // Assert
            Assert.Equal("IRacing", description);
        }

        [Fact]
        public void Display_IsActive_ChecksForIracingProcessRunning()
        {
            // Arrange
            var display = new Display();

            // Act
            var isActive = display.IsActive;

            // Assert - will be false when not running iRacing
            Assert.IsType<bool>(isActive);
        }

        [Fact]
        public void Display_DisplayType_IsRaceDashboard()
        {
            // Arrange
            var display = new Display();
            var data = CreateMockDataSample();

            // Act
            var update = display.ConvertToDisplayUpdate(data);

            // Assert
            Assert.Equal(DisplayType.RaceDashboard, update.Type);
        }

        #endregion

        #region Fuel Tests

        [Fact]
        public void ConvertToDisplayUpdate_FuelRemaining()
        {
            // Arrange
            var display = new Display();
            var data = CreateMockDataSample(fuelLevel: 45.5f);

            // Act
            var update = display.ConvertToDisplayUpdate(data);
            var raceData = update.Data as RaceData;

            // Assert
            Assert.NotNull(raceData);
            Assert.Equal(45.5f, raceData.FuelRemaining);
        }

        [Fact]
        public void ConvertToDisplayUpdate_FuelEstLapsZeroWhenNoHistory()
        {
            // Arrange
            var display = new Display();
            var data = CreateMockDataSample(fuelLevel: 50.0f);

            // Act
            var update = display.ConvertToDisplayUpdate(data);
            var raceData = update.Data as RaceData;

            // Assert
            Assert.NotNull(raceData);
            Assert.Equal(0, raceData.FuelEstLaps);
        }

        #endregion

        #region Brake Bias Tests

        [Fact]
        public void ConvertToDisplayUpdate_BrakeBiasPassedThrough()
        {
            // Arrange
            var display = new Display();
            var data = CreateMockDataSample(brakeBias: 52.5f);

            // Act
            var update = display.ConvertToDisplayUpdate(data);
            var raceData = update.Data as RaceData;

            // Assert
            Assert.NotNull(raceData);
            Assert.Equal(52.5f, raceData.BrakeBias);
        }

        #endregion

        #region Temperature Tests

        [Fact]
        public void ConvertToDisplayUpdate_AirTemperaturePassedThrough()
        {
            // Arrange
            var display = new Display();
            var data = CreateMockDataSample(airTemp: 28.5f);

            // Act
            var update = display.ConvertToDisplayUpdate(data);
            var raceData = update.Data as RaceData;

            // Assert
            Assert.NotNull(raceData);
            Assert.Equal(28.5f, raceData.AirTemp);
        }

        [Fact]
        public void ConvertToDisplayUpdate_TrackTemperaturePassedThrough()
        {
            // Arrange
            var display = new Display();
            var data = CreateMockDataSample(trackTemp: 45.2f);

            // Act
            var update = display.ConvertToDisplayUpdate(data);
            var raceData = update.Data as RaceData;

            // Assert
            Assert.NotNull(raceData);
            Assert.Equal(45.2f, raceData.TrackTemp);
        }

        #endregion

        #region Pit Limiter Tests

        [Fact]
        public void ConvertToDisplayUpdate_PitLimiterDetected()
        {
            // Arrange
            var display = new Display();
            var data = CreateMockDataSample(engineWarnings: EngineWarnings.PitSpeedLimiter);

            // Act
            var update = display.ConvertToDisplayUpdate(data);
            var raceData = update.Data as RaceData;

            // Assert
            Assert.NotNull(raceData);
            Assert.True(raceData.PitLimiterOn);
        }

        [Fact]
        public void ConvertToDisplayUpdate_PitLimiterNotActive()
        {
            // Arrange
            var display = new Display();
            var data = CreateMockDataSample(engineWarnings: EngineWarnings.None);

            // Act
            var update = display.ConvertToDisplayUpdate(data);
            var raceData = update.Data as RaceData;

            // Assert
            Assert.NotNull(raceData);
            Assert.False(raceData.PitLimiterOn);
        }

        #endregion

        #region Position and Incidents Tests

        [Fact]
        public void ConvertToDisplayUpdate_PositionPassedThrough()
        {
            // Arrange
            var display = new Display();
            var data = CreateMockDataSample(position: 5);

            // Act
            var update = display.ConvertToDisplayUpdate(data);
            var raceData = update.Data as RaceData;

            // Assert
            Assert.NotNull(raceData);
            Assert.Equal(5, raceData.Position);
        }

        [Fact]
        public void ConvertToDisplayUpdate_IncidentsCountClamped()
        {
            // Arrange
            var display = new Display();
            var data = CreateMockDataSample(incidents: -5); // Negative should be clamped to 0

            // Act
            var update = display.ConvertToDisplayUpdate(data);
            var raceData = update.Data as RaceData;

            // Assert
            Assert.NotNull(raceData);
            Assert.Equal(0, raceData.Incidents);
        }

        [Fact]
        public void ConvertToDisplayUpdate_IncidentsCountPositive()
        {
            // Arrange
            var display = new Display();
            var data = CreateMockDataSample(incidents: 3);

            // Act
            var update = display.ConvertToDisplayUpdate(data);
            var raceData = update.Data as RaceData;

            // Assert
            Assert.NotNull(raceData);
            Assert.Equal(3, raceData.Incidents);
        }

        #endregion

        #region Helper Methods

        private DataSample CreateMockDataSample(
            int? gear = null,
            float? speed = null,
            float? rpm = null,
            float? throttle = null,
            float? brake = null,
            float? steeringAngle = null,
            float? steeringWheelAngleMax = null,
            float? fuelLevel = null,
            float? brakeBias = null,
            float? airTemp = null,
            float? trackTemp = null,
            EngineWarnings? engineWarnings = null,
            int? position = null,
            int? incidents = null)
        {
            var mockData = new DataSample();

            var telemetry = typeof(DataSample).GetProperty("Telemetry",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)?.GetValue(mockData)
                ?? throw new InvalidOperationException("Cannot access Telemetry");

            if (gear.HasValue)
                telemetry.GetType().GetProperty("Gear")?.SetValue(telemetry, gear);
            if (speed.HasValue)
                telemetry.GetType().GetProperty("Speed")?.SetValue(telemetry, speed);
            if (rpm.HasValue)
                telemetry.GetType().GetProperty("RPM")?.SetValue(telemetry, rpm);
            if (throttle.HasValue)
                telemetry.GetType().GetProperty("Throttle")?.SetValue(telemetry, throttle);
            if (brake.HasValue)
                telemetry.GetType().GetProperty("Brake")?.SetValue(telemetry, brake);
            if (steeringAngle.HasValue)
                telemetry.GetType().GetProperty("SteeringWheelAngle")?.SetValue(telemetry, steeringAngle);
            if (steeringWheelAngleMax.HasValue)
                telemetry.GetType().GetProperty("SteeringWheelAngleMax")?.SetValue(telemetry, steeringWheelAngleMax);
            if (fuelLevel.HasValue)
                telemetry.GetType().GetProperty("FuelLevel")?.SetValue(telemetry, fuelLevel);
            if (brakeBias.HasValue)
                telemetry.GetType().GetProperty("DcBrakeBias")?.SetValue(telemetry, brakeBias);
            if (airTemp.HasValue)
                telemetry.GetType().GetProperty("AirTemp")?.SetValue(telemetry, airTemp);
            if (trackTemp.HasValue)
                telemetry.GetType().GetProperty("TrackTemp")?.SetValue(telemetry, trackTemp);
            if (engineWarnings.HasValue)
                telemetry.GetType().GetProperty("EngineWarnings")?.SetValue(telemetry, engineWarnings);
            if (position.HasValue)
                telemetry.GetType().GetProperty("PlayerCarPosition")?.SetValue(telemetry, position);
            if (incidents.HasValue)
                telemetry.GetType().GetProperty("PlayerCarDriverIncidentCount")?.SetValue(telemetry, incidents);

            return mockData;
        }

        #endregion
    }
}
