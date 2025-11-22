using HaddySimHub.Displays.IRacing;
using HaddySimHub.Models;
using iRacingSDK;
using static iRacingSDK.SessionData;
using static iRacingSDK.SessionData._SessionInfo;
using static iRacingSDK.SessionData._SessionInfo._Sessions;
using static iRacingSDK.SessionData._WeekendInfo;

namespace HaddySimHub.Tests
{
    [TestClass]
    public class IRacingDisplayTests
    {
        #region GenerateRpmLights Tests

        [TestMethod]
        public void GenerateRpmLights_WithFiaF4_ReturnsCorrectRpmLights()
        {
            // Arrange
            var carName = "FIA F4";

            // Act
            var lights = Display.GenerateRpmLights(carName);

            // Assert
            Assert.IsNotNull(lights);
            Assert.HasCount(6, lights);
            Assert.AreEqual(6300, lights[0].Rpm);
            Assert.AreEqual("Green", lights[0].Color);
            Assert.AreEqual(6500, lights[1].Rpm);
            Assert.AreEqual("Green", lights[1].Color);
            Assert.AreEqual(6600, lights[2].Rpm);
            Assert.AreEqual("Green", lights[2].Color);
            Assert.AreEqual(6700, lights[3].Rpm);
            Assert.AreEqual("Green", lights[3].Color);
            Assert.AreEqual(6800, lights[4].Rpm);
            Assert.AreEqual("Red", lights[4].Color);
            Assert.AreEqual(6900, lights[5].Rpm);
            Assert.AreEqual("Red", lights[5].Color);
        }

        [TestMethod]
        public void GenerateRpmLights_WithFiaF4_AllLightsHaveValidRpm()
        {
            // Arrange
            var carName = "FIA F4";

            // Act
            var lights = Display.GenerateRpmLights(carName);

            // Assert
            foreach (var light in lights)
            {
                Assert.IsGreaterThanOrEqualTo(6300, light.Rpm, $"RPM {light.Rpm} should be >= 6300");
                Assert.IsLessThanOrEqualTo(6900, light.Rpm, $"RPM {light.Rpm} should be <= 6900");
            }
        }

        [TestMethod]
        public void GenerateRpmLights_WithFiaF4_HasCorrectColorPattern()
        {
            // Arrange
            var carName = "FIA F4";

            // Act
            var lights = Display.GenerateRpmLights(carName);

            // Assert
            Assert.AreEqual("Green", lights[0].Color);
            Assert.AreEqual("Green", lights[1].Color);
            Assert.AreEqual("Green", lights[2].Color);
            Assert.AreEqual("Green", lights[3].Color);
            Assert.AreEqual("Red", lights[4].Color);
            Assert.AreEqual("Red", lights[5].Color);
        }

        [TestMethod]
        public void GenerateRpmLights_WithFiaF4_RpmValuesAreAscending()
        {
            // Arrange
            var carName = "FIA F4";

            // Act
            var lights = Display.GenerateRpmLights(carName);

            // Assert
            for (int i = 1; i < lights.Length; i++)
            {
                Assert.IsGreaterThan(lights[i - 1].Rpm, lights[i].Rpm, $"RPM values should be ascending: {lights[i].Rpm} should be > {lights[i - 1].Rpm}");
            }
        }

        [TestMethod]
        public void GenerateRpmLights_WithUnknownCar_ReturnsEmptyArray()
        {
            // Arrange
            var carName = "UnknownCar";

            // Act
            var lights = Display.GenerateRpmLights(carName);

            // Assert
            Assert.IsNotNull(lights);
            Assert.IsEmpty(lights);
        }

        [TestMethod]
        public void GenerateRpmLights_WithEmptyString_ReturnsEmptyArray()
        {
            // Arrange
            var carName = string.Empty;

            // Act
            var lights = Display.GenerateRpmLights(carName);

            // Assert
            Assert.IsNotNull(lights);
            Assert.IsEmpty(lights);
        }

        [TestMethod]
        public void GenerateRpmLights_WithNullCarName_ReturnsEmptyArray()
        {
            // Arrange & Act & Assert
            var lights = Display.GenerateRpmLights(null!);
            Assert.IsNotNull(lights);
            Assert.IsEmpty(lights);
        }

        #endregion

        #region GetRpmMax Tests

        [TestMethod]
        public void GetRpmMax_WithFiaF4_Returns7000()
        {
            // Arrange
            var carName = "FIA F4";

            // Act
            var rpmMax = Display.GetRpmMax(carName);

            // Assert
            Assert.AreEqual(7000, rpmMax);
        }

        [TestMethod]
        public void GetRpmMax_WithUnknownCar_ReturnsZero()
        {
            // Arrange
            var carName = "UnknownCar";

            // Act
            var rpmMax = Display.GetRpmMax(carName);

            // Assert
            Assert.AreEqual(0, rpmMax);
        }

        [TestMethod]
        public void GetRpmMax_WithEmptyString_ReturnsZero()
        {
            // Arrange
            var carName = string.Empty;

            // Act
            var rpmMax = Display.GetRpmMax(carName);

            // Assert
            Assert.AreEqual(0, rpmMax);
        }

        [TestMethod]
        public void GetRpmMax_WithNullCarName_ReturnsZero()
        {
            // Arrange & Act
            var rpmMax = Display.GetRpmMax(null!);

            // Assert
            Assert.AreEqual(0, rpmMax);
        }

        #endregion

        #region Gear Conversion Tests

        [TestMethod]
        public void ConvertToDisplayUpdate_GearReverseConvertsToR()
        {
            // Arrange
            var display = new Display();
            var data = CreateMockDataSample(gear: -1, playerCarIdx: 0, carIdxLap: new int[64]);

            // Act
            var update = display.ConvertToDisplayUpdate(data);
            var raceData = update.Data as RaceData;

            // Assert
            Assert.IsNotNull(raceData);
            Assert.AreEqual("R", raceData.Gear);
        }

        [TestMethod]
        public void ConvertToDisplayUpdate_GearNeutralConvertsToN()
        {
            // Arrange
            var display = new Display();
            var data = CreateMockDataSample(gear: 0, playerCarIdx: 0, carIdxLap: new int[64]);

            // Act
            var update = display.ConvertToDisplayUpdate(data);
            var raceData = update.Data as RaceData;

            // Assert
            Assert.IsNotNull(raceData);
            Assert.AreEqual("N", raceData.Gear);
        }

        [TestMethod]
        public void ConvertToDisplayUpdate_GearFirstConvertsToOneString()
        {
            // Arrange
            var display = new Display();
            var data = CreateMockDataSample(gear: 1, playerCarIdx: 0, carIdxLap: new int[64]);

            // Act
            var update = display.ConvertToDisplayUpdate(data);
            var raceData = update.Data as RaceData;

            // Assert
            Assert.IsNotNull(raceData);
            Assert.AreEqual("1", raceData.Gear);
        }

        [TestMethod]
        public void ConvertToDisplayUpdate_GearHighConvertsToNumericString()
        {
            // Arrange
            var display = new Display();
            var data = CreateMockDataSample(gear: 6, playerCarIdx: 0, carIdxLap: new int[64]);

            // Act
            var update = display.ConvertToDisplayUpdate(data);
            var raceData = update.Data as RaceData;

            // Assert
            Assert.IsNotNull(raceData);
            Assert.AreEqual("6", raceData.Gear);
        }

        #endregion

        #region Speed Conversion Tests

        [TestMethod]
        public void ConvertToDisplayUpdate_SpeedConversionMultipliesByThree_Six()
        {
            // Arrange
            var display = new Display();
            var data = CreateMockDataSample(speed: 10.0f, playerCarIdx: 0, carIdxLap: new int[64]); // 10 m/s = 36 km/h

            // Act
            var update = display.ConvertToDisplayUpdate(data);
            var raceData = update.Data as RaceData;

            // Assert
            Assert.IsNotNull(raceData);
            Assert.AreEqual(36, raceData.Speed);
        }

        [TestMethod]
        public void ConvertToDisplayUpdate_SpeedZeroRemains()
        {
            // Arrange
            var display = new Display();
            var data = CreateMockDataSample(speed: 0.0f, playerCarIdx: 0, carIdxLap: new int[64]);

            // Act
            var update = display.ConvertToDisplayUpdate(data);
            var raceData = update.Data as RaceData;

            // Assert
            Assert.IsNotNull(raceData);
            Assert.AreEqual(0, raceData.Speed);
        }

        [TestMethod]
        public void ConvertToDisplayUpdate_SpeedRoundsCorrectly()
        {
            // Arrange
            var display = new Display();
            var data = CreateMockDataSample(speed: 27.777f, playerCarIdx: 0, carIdxLap: new int[64]); // 27.777 * 3.6 = 99.9972 -> rounds to 100

            // Act
            var update = display.ConvertToDisplayUpdate(data);
            var raceData = update.Data as RaceData;

            // Assert
            Assert.IsNotNull(raceData);
            Assert.AreEqual(100, raceData.Speed);
        }

        #endregion

        #region RPM Tests

        [TestMethod]
        public void ConvertToDisplayUpdate_RpmConvertedToInt()
        {
            // Arrange
            var display = new Display();
            var data = CreateMockDataSample(rpm: 6543.5f, playerCarIdx: 0, carIdxLap: new int[64]);

            // Act
            var update = display.ConvertToDisplayUpdate(data);
            var raceData = update.Data as RaceData;

            // Assert
            Assert.IsNotNull(raceData);
            Assert.AreEqual(6543, raceData.Rpm);
        }

        [TestMethod]
        public void ConvertToDisplayUpdate_RpmZeroRemains()
        {
            // Arrange
            var display = new Display();
            var data = CreateMockDataSample(rpm: 0.0f, playerCarIdx: 0, carIdxLap: new int[64]);

            // Act
            var update = display.ConvertToDisplayUpdate(data);
            var raceData = update.Data as RaceData;

            // Assert
            Assert.IsNotNull(raceData);
            Assert.AreEqual(0, raceData.Rpm);
        }

        #endregion

        #region Throttle and Brake Conversion Tests

        [TestMethod]
        public void ConvertToDisplayUpdate_ThrottleConvertedToPercentage()
        {
            // Arrange
            var display = new Display();
            var data = CreateMockDataSample(throttle: 0.75f, playerCarIdx: 0, carIdxLap: new int[64]); // 0.75 = 75%

            // Act
            var update = display.ConvertToDisplayUpdate(data);
            var raceData = update.Data as RaceData;

            // Assert
            Assert.IsNotNull(raceData);
            Assert.AreEqual(75, raceData.ThrottlePct);
        }

        [TestMethod]
        public void ConvertToDisplayUpdate_BrakeConvertedToPercentage()
        {
            // Arrange
            var display = new Display();
            var data = CreateMockDataSample(brake: 0.50f, playerCarIdx: 0, carIdxLap: new int[64]); // 0.50 = 50%

            // Act
            var update = display.ConvertToDisplayUpdate(data);
            var raceData = update.Data as RaceData;

            // Assert
            Assert.IsNotNull(raceData);
            Assert.AreEqual(50, raceData.BrakePct);
        }

        [TestMethod]
        public void ConvertToDisplayUpdate_FullThrottle()
        {
            // Arrange
            var display = new Display();
            var data = CreateMockDataSample(throttle: 1.0f, playerCarIdx: 0, carIdxLap: new int[64]);

            // Act
            var update = display.ConvertToDisplayUpdate(data);
            var raceData = update.Data as RaceData;

            // Assert
            Assert.IsNotNull(raceData);
            Assert.AreEqual(100, raceData.ThrottlePct);
        }

        [TestMethod]
        public void ConvertToDisplayUpdate_NoThrottle()
        {
            // Arrange
            var display = new Display();
            var data = CreateMockDataSample(throttle: 0.0f, playerCarIdx: 0, carIdxLap: new int[64]);

            // Act
            var update = display.ConvertToDisplayUpdate(data);
            var raceData = update.Data as RaceData;

            // Assert
            Assert.IsNotNull(raceData);
            Assert.AreEqual(0, raceData.ThrottlePct);
        }

        #endregion

        #region Steering Conversion Tests

        [TestMethod]
        public void ConvertToDisplayUpdate_SteeringCenteredIs50Percent()
        {
            // Arrange
            var display = new Display();
            var data = CreateMockDataSample(steeringAngle: 0.0f, steeringWheelAngleMax: 12.0f, playerCarIdx: 0, carIdxLap: new int[64]);

            // Act
            var update = display.ConvertToDisplayUpdate(data);
            var raceData = update.Data as RaceData;

            // Assert
            Assert.IsNotNull(raceData);
            Assert.AreEqual(50, raceData.SteeringPct);
        }

        [TestMethod]
        public void ConvertToDisplayUpdate_SteeringFullLeft()
        {
            // Arrange
            var display = new Display();
            var data = CreateMockDataSample(steeringAngle: -6.0f, steeringWheelAngleMax: 12.0f, playerCarIdx: 0, carIdxLap: new int[64]);

            // Act
            var update = display.ConvertToDisplayUpdate(data);
            var raceData = update.Data as RaceData;

            // Assert
            Assert.IsNotNull(raceData);
            Assert.AreEqual(0, raceData.SteeringPct);
        }

        [TestMethod]
        public void ConvertToDisplayUpdate_SteeringFullRight()
        {
            // Arrange
            var display = new Display();
            var data = CreateMockDataSample(steeringAngle: 6.0f, steeringWheelAngleMax: 12.0f, playerCarIdx: 0, carIdxLap: new int[64]);

            // Act
            var update = display.ConvertToDisplayUpdate(data);
            var raceData = update.Data as RaceData;

            // Assert
            Assert.IsNotNull(raceData);
            Assert.AreEqual(100, raceData.SteeringPct);
        }

        [TestMethod]
        public void ConvertToDisplayUpdate_SteeringQuarterLeft()
        {
            // Arrange
            var display = new Display();
            var data = CreateMockDataSample(steeringAngle: -3.0f, steeringWheelAngleMax: 12.0f, playerCarIdx: 0, carIdxLap: new int[64]);

            // Act
            var update = display.ConvertToDisplayUpdate(data);
            var raceData = update.Data as RaceData;

            // Assert
            Assert.IsNotNull(raceData);
            Assert.AreEqual(25, raceData.SteeringPct);
        }

        [TestMethod]
        public void ConvertToDisplayUpdate_SteeringZeroAngleMaxDefaultsTo50()
        {
            // Arrange
            var display = new Display();
            var data = CreateMockDataSample(steeringAngle: 5.0f, steeringWheelAngleMax: 0.0f, playerCarIdx: 0, carIdxLap: new int[64]);

            // Act
            var update = display.ConvertToDisplayUpdate(data);
            var raceData = update.Data as RaceData;

            // Assert
            Assert.IsNotNull(raceData);
            Assert.AreEqual(50, raceData.SteeringPct);
        }

        [TestMethod]
        public void ConvertToDisplayUpdate_SteeringClampsToZero()
        {
            // Arrange
            var display = new Display();
            var data = CreateMockDataSample(steeringAngle: -7.0f, steeringWheelAngleMax: 12.0f, playerCarIdx: 0, carIdxLap: new int[64]);

            // Act
            var update = display.ConvertToDisplayUpdate(data);
            var raceData = update.Data as RaceData;

            // Assert
            Assert.IsNotNull(raceData);
            Assert.IsGreaterThanOrEqualTo(0, raceData.SteeringPct);
        }

        [TestMethod]
        public void ConvertToDisplayUpdate_SteeringClampsTo100()
        {
            // Arrange
            var display = new Display();
            var data = CreateMockDataSample(steeringAngle: 7.0f, steeringWheelAngleMax: 12.0f, playerCarIdx: 0, carIdxLap: new int[64]);

            // Act
            var update = display.ConvertToDisplayUpdate(data);
            var raceData = update.Data as RaceData;

            // Assert
            Assert.IsNotNull(raceData);
            Assert.IsLessThanOrEqualTo(100, raceData.SteeringPct);
        }

        #endregion

        #region Display Properties Tests

        [TestMethod]
        public void Display_Description_ReturnsIRacing()
        {
            // Arrange
            var display = new Display();

            // Act
            var description = display.Description;

            // Assert
            Assert.AreEqual("IRacing", description);
        }

        [TestMethod]
        public void Display_DisplayType_IsRaceDashboard()
        {
            // Arrange
            var display = new Display();
            var data = CreateMockDataSample(playerCarIdx: 0, carIdxLap: new int[64]);

            // Act
            var update = display.ConvertToDisplayUpdate(data);

            // Assert
            Assert.AreEqual(DisplayType.RaceDashboard, update.Type);
        }

        #endregion

        #region Fuel Tests

        [TestMethod]
        public void ConvertToDisplayUpdate_FuelRemaining()
        {
            // Arrange
            var display = new Display();
            var data = CreateMockDataSample(fuelLevel: 45.5f, playerCarIdx: 0, carIdxLap: new int[64]);

            // Act
            var update = display.ConvertToDisplayUpdate(data);
            var raceData = update.Data as RaceData;

            // Assert
            Assert.IsNotNull(raceData);
            Assert.AreEqual(45.5f, raceData.FuelRemaining);
        }

        [TestMethod]
        public void ConvertToDisplayUpdate_FuelEstLapsZeroWhenNoHistory()
        {
            // Arrange
            var display = new Display();
            var data = CreateMockDataSample(fuelLevel: 50.0f, playerCarIdx: 0, carIdxLap: new int[64]);

            // Act
            var update = display.ConvertToDisplayUpdate(data);
            var raceData = update.Data as RaceData;

            // Assert
            Assert.IsNotNull(raceData);
            Assert.AreEqual(0, raceData.FuelEstLaps);
        }

        #endregion

        #region Brake Bias Tests

        [TestMethod]
        public void ConvertToDisplayUpdate_BrakeBiasPassedThrough()
        {
            // Arrange
            var display = new Display();
            var data = CreateMockDataSample(brakeBias: 52.5f, playerCarIdx: 0, carIdxLap: new int[64]);

            // Act
            var update = display.ConvertToDisplayUpdate(data);
            var raceData = update.Data as RaceData;

            // Assert
            Assert.IsNotNull(raceData);
            Assert.AreEqual(52.5f, raceData.BrakeBias);
        }

        #endregion

        #region Temperature Tests

        [TestMethod]
        public void ConvertToDisplayUpdate_AirTemperaturePassedThrough()
        {
            // Arrange
            var display = new Display();
            var data = CreateMockDataSample(airTemp: 28.5f, playerCarIdx: 0, carIdxLap: new int[64]);

            // Act
            var update = display.ConvertToDisplayUpdate(data);
            var raceData = update.Data as RaceData;

            // Assert
            Assert.IsNotNull(raceData);
            Assert.AreEqual(28.5f, raceData.AirTemp);
        }

        [TestMethod]
        public void ConvertToDisplayUpdate_TrackTemperaturePassedThrough()
        {
            // Arrange
            var display = new Display();
            var data = CreateMockDataSample(trackTemp: 45.2f, playerCarIdx: 0, carIdxLap: new int[64]);

            // Act
            var update = display.ConvertToDisplayUpdate(data);
            var raceData = update.Data as RaceData;

            // Assert
            Assert.IsNotNull(raceData);
            Assert.AreEqual(45.2f, raceData.TrackTemp);
        }

        #endregion

        #region Pit Limiter Tests

        [TestMethod]
        public void ConvertToDisplayUpdate_PitLimiterDetected()
        {
            // Arrange
            var display = new Display();
            var data = CreateMockDataSample(engineWarnings: EngineWarnings.PitSpeedLimiter, playerCarIdx: 0, carIdxLap: new int[64]);

            // Act
            var update = display.ConvertToDisplayUpdate(data);
            var raceData = update.Data as RaceData;

            // Assert
            Assert.IsNotNull(raceData);
            Assert.IsTrue(raceData.PitLimiterOn);
        }

        [TestMethod]
        public void ConvertToDisplayUpdate_PitLimiterNotActive()
        {
            // Arrange
            var display = new Display();
            var data = CreateMockDataSample(engineWarnings: EngineWarnings.None, playerCarIdx: 0, carIdxLap: new int[64]);

            // Act
            var update = display.ConvertToDisplayUpdate(data);
            var raceData = update.Data as RaceData;

            // Assert
            Assert.IsNotNull(raceData);
            Assert.IsFalse(raceData.PitLimiterOn);
        }

        #endregion

        #region Position and Incidents Tests

        [TestMethod]
        public void ConvertToDisplayUpdate_PositionPassedThrough()
        {
            // Arrange
            var display = new Display();
            var data = CreateMockDataSample(position: 5, playerCarIdx: 0, carIdxLap: new int[64]);

            // Act
            var update = display.ConvertToDisplayUpdate(data);
            var raceData = update.Data as RaceData;

            // Assert
            Assert.IsNotNull(raceData);
            Assert.AreEqual(5, raceData.Position);
        }

        [TestMethod]
        public void ConvertToDisplayUpdate_IncidentsCountClamped()
        {
            // Arrange
            var display = new Display();
            var data = CreateMockDataSample(incidents: -5, playerCarIdx: 0, carIdxLap: new int[64]); // Negative should be clamped to 0

            // Act
            var update = display.ConvertToDisplayUpdate(data);
            var raceData = update.Data as RaceData;

            // Assert
            Assert.IsNotNull(raceData);
            Assert.AreEqual(0, raceData.Incidents);
        }

        [TestMethod]
        public void ConvertToDisplayUpdate_IncidentsCountPositive()
        {
            // Arrange
            var display = new Display();
            var data = CreateMockDataSample(incidents: 3, playerCarIdx: 0, carIdxLap: new int[64]);

            // Act
            var update = display.ConvertToDisplayUpdate(data);
            var raceData = update.Data as RaceData;

            // Assert
            Assert.IsNotNull(raceData);
            Assert.AreEqual(3, raceData.Incidents);
        }

        #endregion

        #region Helper Methods

        private class TestDataSample : IDataSample
        {
            public bool IsConnected { get; set; }
            public SessionData SessionData { get; set; } = new();
            public Telemetry Telemetry { get; set; } = [];
            public DataSample LastSample { get; set; } =  new();
        }

        private class DataSampleBuilder
        {
            private readonly IDataSample _sample;
            private readonly Telemetry _telemetry;

            public DataSampleBuilder()
            {
                _sample = new TestDataSample();
                _telemetry = new Telemetry();
                _sample.Telemetry = _telemetry;

                // Set default values for all required telemetry data
                _telemetry["SessionNum"] = 1;
                _telemetry["PlayerCarIdx"] = 0; // Default to car index 0
                _telemetry["CarIdxLap"] = new int[64];
                _telemetry["FuelLevel"] = 0.0f;
                _telemetry["PlayerCarInPitStall"] = false;
                _telemetry["OnPitRoad"] = false;
                _telemetry["Lap"] = 0;
                _telemetry["PlayerCarDriverIncidentCount"] = 0;
                _telemetry["SessionTimeRemain"] = 0.0;
                _telemetry["PlayerCarPosition"] = 0;
                _telemetry["LapCurrentLapTime"] = 0.0f;
                _telemetry["LapLastLapTime"] = 0.0f;
                _telemetry["LapDeltaToSessionLastlLap"] = 0.0f;
                _telemetry["LapBestLapTime"] = 0.0f;
                _telemetry["LapDeltaToBestLap"] = 0.0f;
                _telemetry["Gear"] = 0;
                _telemetry["RPM"] = 0.0f;
                _telemetry["Speed"] = 0.0f;
                _telemetry["Brake"] = 0.0f;
                _telemetry["Throttle"] = 0.0f;
                _telemetry["DcBrakeBias"] = 0.0f;
                _telemetry["AirTemp"] = 0.0f;
                _telemetry["TrackTemp"] = 0.0f;
                _telemetry["EngineWarnings"] = (int)EngineWarnings.None;
                _telemetry["SteeringWheelAngle"] = 0.0f;
                _telemetry["SteeringWheelAngleMax"] = 0.0f;
                
                _sample.SessionData = new SessionData
                {
                    SessionInfo = new _SessionInfo
                    {
                        Sessions =
                        [
                            new _Sessions
                            {
                                SessionNum = 1,
                                SessionLaps = "0",
                                SessionTime = "unlimited",
                                ResultsPositions = new _ResultsPositions[64],
                            }
                        ]
                    },
                    DriverInfo = new _DriverInfo
                    {
                         Drivers = 
                         [
                            new _DriverInfo._Drivers
                            {
                                CarIdx = 0,
                                CarNumberRaw = 1, // Ensure this is > 0 for MaxLength
                                UserName = "Test Driver",
                                IRating = 1500,
                                CarNumber = "42"
                            }
                         ]
                    },
                    WeekendInfo = new _WeekendInfo
                    {
                        WeekendOptions = new _WeekendOptions
                        {
                            IncidentLimit = "0"
                        },
                        TrackName = "Test Track"
                    }
                };
                // Link the session data to the telemetry object
                _telemetry.SessionData = _sample.SessionData;
            }

            public DataSampleBuilder WithGear(int gear) { _telemetry["Gear"] = gear; return this; }
            public DataSampleBuilder WithSpeed(float speed) { _telemetry["Speed"] = speed; return this; }
            public DataSampleBuilder WithRpm(float rpm) { _telemetry["RPM"] = rpm; return this; }
            public DataSampleBuilder WithThrottle(float throttle) { _telemetry["Throttle"] = throttle; return this; }
            public DataSampleBuilder WithBrake(float brake) { _telemetry["Brake"] = brake; return this; }
            public DataSampleBuilder WithSteeringAngle(float angle) { _telemetry["SteeringWheelAngle"] = angle; return this; }
            public DataSampleBuilder WithSteeringWheelAngleMax(float max) { _telemetry["SteeringWheelAngleMax"] = max; return this; }
            public DataSampleBuilder WithFuelLevel(float fuel) { _telemetry["FuelLevel"] = fuel; return this; }
            public DataSampleBuilder WithBrakeBias(float bias) { _telemetry["dcBrakeBias"] = bias; return this; }
            public DataSampleBuilder WithAirTemp(float temp) { _telemetry["AirTemp"] = temp; return this; }
            public DataSampleBuilder WithTrackTemp(float temp) { _telemetry["TrackTemp"] = temp; return this; }
            public DataSampleBuilder WithEngineWarnings(EngineWarnings warnings) { _telemetry["EngineWarnings"] = (Int32)warnings; return this; }
            public DataSampleBuilder WithPosition(int pos) { _telemetry["PlayerCarPosition"] = pos; return this; }
            public DataSampleBuilder WithIncidents(int inc) { _telemetry["PlayerCarDriverIncidentCount"] = inc; return this; }
            public DataSampleBuilder WithPlayerCarIdx(int idx) { 
                _telemetry["PlayerCarIdx"] = idx; 
                _sample.SessionData.DriverInfo.Drivers[0].CarIdx = idx; // Update CarIdx in SessionData to match
                return this; 
            }
            public DataSampleBuilder WithCarIdxLap(int[] carIdxLap) { _telemetry["CarIdxLap"] = carIdxLap; return this; }
            public DataSampleBuilder WithCarNumberRaw(int carNumberRaw) {
                // This method should update the existing driver or add a new one if necessary
                // For simplicity, assuming we're always updating the first driver
                _sample.SessionData.DriverInfo.Drivers[0].CarNumberRaw = carNumberRaw;
                return this;
            }
            public DataSampleBuilder WithSessionNum(int sessionNum)
            {
                _telemetry["SessionNum"] = sessionNum;
                _sample.SessionData.SessionInfo.Sessions[0].SessionNum = sessionNum;
                return this;
            }

            public IDataSample Build() => _sample;
        }

        private IDataSample CreateMockDataSample(
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
            int? incidents = null,
            int? playerCarIdx = null,
            int[]? carIdxLap = null)
        {
            var builder = new DataSampleBuilder();
            if (gear.HasValue) builder.WithGear(gear.Value);
            if (speed.HasValue) builder.WithSpeed(speed.Value);
            if (rpm.HasValue) builder.WithRpm(rpm.Value);
            if (throttle.HasValue) builder.WithThrottle(throttle.Value);
            if (brake.HasValue) builder.WithBrake(brake.Value);
            if (steeringAngle.HasValue) builder.WithSteeringAngle(steeringAngle.Value);
            if (steeringWheelAngleMax.HasValue) builder.WithSteeringWheelAngleMax(steeringWheelAngleMax.Value);
            if (fuelLevel.HasValue) builder.WithFuelLevel(fuelLevel.Value);
            if (brakeBias.HasValue) builder.WithBrakeBias(brakeBias.Value);
            if (airTemp.HasValue) builder.WithAirTemp(airTemp.Value);
            if (trackTemp.HasValue) builder.WithTrackTemp(trackTemp.Value);
            if (engineWarnings.HasValue) builder.WithEngineWarnings(engineWarnings.Value);
            if (position.HasValue) builder.WithPosition(position.Value);
            if (incidents.HasValue) builder.WithIncidents(incidents.Value);
            if (playerCarIdx.HasValue) builder.WithPlayerCarIdx(playerCarIdx.Value);
            if (carIdxLap != null) builder.WithCarIdxLap(carIdxLap);
            return builder.Build();
        }

        #endregion
    }
}
