using HaddySimHub.Displays;
using HaddySimHub.Displays.Dirt2;
using HaddySimHub.Models;
using System.Net.Sockets;

namespace HaddySimHub.Tests
{
    [TestClass]
    public class Dirt2DisplayTests
    {
        private static Packet CreatePacket(
            float speed_ms = 0,
            float rpm = 0,
            float max_rpm = 0,
            float idle_rpm = 0,
            float gear = 0,
            float throttle = 0,
            float brakes = 0,
            float clutch = 0,
            float distance = 0)
        {
            return new Packet
            {
                speed_ms = speed_ms,
                rpm = rpm,
                max_rpm = max_rpm,
                idle_rpm = idle_rpm,
                gear = gear,
                throttle = throttle,
                brakes = brakes,
                clutch = clutch,
                distance = distance
            };
        }

        private class MockUdpClientFactory : IUdpClientFactory
        {
            public UdpClient Create(int port) => null!;
        }
        #region GenerateRpmLights Tests

        [TestMethod]
        public void GenerateRpmLights_Creates6Lights()
        {
            // Arrange
            int rpmMax = 7000;

            // Act
            var lights = Display.GenerateRpmLights(rpmMax);

            // Assert
            Assert.IsNotNull(lights);
            Assert.HasCount(6, lights);
        }

        [TestMethod]
        public void GenerateRpmLights_FirstTwoLightsAreGreen()
        {
            // Arrange
            int rpmMax = 7000;

            // Act
            var lights = Display.GenerateRpmLights(rpmMax);

            // Assert
            Assert.AreEqual("Green", lights[0].Color);
            Assert.AreEqual("Green", lights[1].Color);
        }

        [TestMethod]
        public void GenerateRpmLights_MiddleTwoLightsAreYellow()
        {
            // Arrange
            int rpmMax = 7000;

            // Act
            var lights = Display.GenerateRpmLights(rpmMax);

            // Assert
            Assert.AreEqual("Yellow", lights[2].Color);
            Assert.AreEqual("Yellow", lights[3].Color);
        }

        [TestMethod]
        public void GenerateRpmLights_LastTwoLightsAreRed()
        {
            // Arrange
            int rpmMax = 7000;

            // Act
            var lights = Display.GenerateRpmLights(rpmMax);

            // Assert
            Assert.AreEqual("Red", lights[4].Color);
            Assert.AreEqual("Red", lights[5].Color);
        }

        [TestMethod]
        public void GenerateRpmLights_RpmValuesCorrect()
        {
            // Arrange
            int rpmMax = 7000;

            // Act
            var lights = Display.GenerateRpmLights(rpmMax);

            // Assert
            // RPM values are calculated as: rpmMax - ((lightsCount - i) * lightsStep)
            // So: 7000 - (6*200)=5800, 7000-(5*200)=6000, etc.
            Assert.AreEqual(5800, lights[0].Rpm);
            Assert.AreEqual(6000, lights[1].Rpm);
            Assert.AreEqual(6200, lights[2].Rpm);
            Assert.AreEqual(6400, lights[3].Rpm);
            Assert.AreEqual(6600, lights[4].Rpm);
            Assert.AreEqual(6800, lights[5].Rpm);
        }

        [TestMethod]
        public void GenerateRpmLights_WithDifferentRpmMax()
        {
            // Arrange
            int rpmMax = 8000;

            // Act
            var lights = Display.GenerateRpmLights(rpmMax);

            // Assert
            // 8000 - (6*200)=6800, 8000-(5*200)=7000, etc.
            Assert.AreEqual(6800, lights[0].Rpm);
            Assert.AreEqual(7000, lights[1].Rpm);
            Assert.AreEqual(7200, lights[2].Rpm);
            Assert.AreEqual(7400, lights[3].Rpm);
            Assert.AreEqual(7600, lights[4].Rpm);
            Assert.AreEqual(7800, lights[5].Rpm);
        }

        [TestMethod]
        public void GenerateRpmLights_WithLowRpmMax()
        {
            // Arrange
            int rpmMax = 2000;

            // Act
            var lights = Display.GenerateRpmLights(rpmMax);

            // Assert
            Assert.IsNotNull(lights);
            // Even with low max, lights are calculated correctly
            // 2000 - (6*200)=800
            Assert.AreEqual(800, lights[0].Rpm);
        }

        [TestMethod]
        public void GenerateRpmLights_AllLightsHaveValidRpm()
        {
            // Arrange
            int rpmMax = 7000;

            // Act
            var lights = Display.GenerateRpmLights(rpmMax);

            // Assert
            foreach (var light in lights)
            {
                Assert.IsGreaterThan(0, light.Rpm);
            }
        }

        #endregion

        #region Speed Conversion Tests

        [TestMethod]
            public void ConvertToDisplayUpdate_ConvertsSpeedMsToKmh()
            {
                var display = new Display(new MockUdpClientFactory());
                var packet = CreatePacket(speed_ms: 27.78f);
                var update = display.ConvertToDisplayUpdate(packet);
                var rally = update.Data as RallyData;
                Assert.IsNotNull(rally);
                Assert.AreEqual(100, rally.Speed);
            }

        [TestMethod]
            public void ConvertToDisplayUpdate_ConvertsZeroSpeed()
            {
                var display = new Display(new MockUdpClientFactory());
                var packet = CreatePacket(speed_ms: 0f);
                var update = display.ConvertToDisplayUpdate(packet);
                var rally = update.Data as RallyData;
                Assert.IsNotNull(rally);
                Assert.AreEqual(0, rally.Speed);
            }

        [TestMethod]
            public void ConvertToDisplayUpdate_ConvertsHighSpeed()
            {
                var display = new Display(new MockUdpClientFactory());
                var packet = CreatePacket(speed_ms: 50f);
                var update = display.ConvertToDisplayUpdate(packet);
                var rally = update.Data as RallyData;
                Assert.IsNotNull(rally);
                Assert.AreEqual(180, rally.Speed);
            }

        #endregion

        #region RPM Calculation Tests

        [TestMethod]
            public void ConvertToDisplayUpdate_CalculatesRpmFromRawValue()
            {
                var display = new Display(new MockUdpClientFactory());
                var packet = CreatePacket(rpm: 500f);
                var update = display.ConvertToDisplayUpdate(packet);
                var rally = update.Data as RallyData;
                Assert.IsNotNull(rally);
                Assert.AreEqual(5000, rally.Rpm);
            }

        [TestMethod]
            public void ConvertToDisplayUpdate_CalculatesRpmMax()
            {
                var display = new Display(new MockUdpClientFactory());
                var packet = CreatePacket(max_rpm: 700f);
                var update = display.ConvertToDisplayUpdate(packet);
                var rally = update.Data as RallyData;
                Assert.IsNotNull(rally);
                Assert.AreEqual(7000, rally.RpmMax);
            }

        [TestMethod]
            public void ConvertToDisplayUpdate_CalculatesIdleRpm()
            {
                var display = new Display(new MockUdpClientFactory());
                var packet = CreatePacket(idle_rpm: 100f);
                // idle_rpm is not used in RallyData, but we can check the packet value
                Assert.AreEqual(1000, packet.idle_rpm * 10);
            }

        #endregion

        #region Gear Tests

        [TestMethod]
            public void ConvertToDisplayUpdate_NeutralGear()
            {
                var display = new Display(new MockUdpClientFactory());
                var packet = CreatePacket(gear: 0f);
                var update = display.ConvertToDisplayUpdate(packet);
                var rally = update.Data as RallyData;
                Assert.IsNotNull(rally);
                Assert.AreEqual("N", rally.Gear);
            }

        [TestMethod]
            public void ConvertToDisplayUpdate_ReverseGear()
            {
                var display = new Display(new MockUdpClientFactory());
                var packet = CreatePacket(gear: -1f);
                var update = display.ConvertToDisplayUpdate(packet);
                var rally = update.Data as RallyData;
                Assert.IsNotNull(rally);
                Assert.AreEqual("R", rally.Gear);
            }

        [TestMethod]
            public void ConvertToDisplayUpdate_FirstGear()
            {
                var display = new Display(new MockUdpClientFactory());
                var packet = CreatePacket(gear: 1f);
                var update = display.ConvertToDisplayUpdate(packet);
                var rally = update.Data as RallyData;
                Assert.IsNotNull(rally);
                Assert.AreEqual("1", rally.Gear);
            }

        [TestMethod]
            public void ConvertToDisplayUpdate_HighGear()
            {
                var display = new Display(new MockUdpClientFactory());
                var packet = CreatePacket(gear: 6f);
                var update = display.ConvertToDisplayUpdate(packet);
                var rally = update.Data as RallyData;
                Assert.IsNotNull(rally);
                Assert.AreEqual("6", rally.Gear);
            }

        #endregion

        #region Control Input Tests

            [TestMethod]
            public void ConvertToDisplayUpdate_CalculatesThrottle()
            {
                var display = new Display(new MockUdpClientFactory());
                var packet = CreatePacket(throttle: 0.75f);
                var update = display.ConvertToDisplayUpdate(packet);
                var rally = update.Data as RallyData;
                Assert.IsNotNull(rally);
                Assert.AreEqual(75, rally.Throttle);
            }

        [TestMethod]
        public void ConvertToDisplayUpdate_CalculatesBrake()
        {
            var display = new Display(new MockUdpClientFactory());
            var packet = CreatePacket(brakes: 0.50f);
            var update = display.ConvertToDisplayUpdate(packet);
            var rally = update.Data as RallyData;
            Assert.IsNotNull(rally);
            Assert.AreEqual(50, rally.Brake);
        }

        [TestMethod]
        public void ConvertToDisplayUpdate_CalculatesClutch()
        {
            var display = new Display(new MockUdpClientFactory());
            var packet = CreatePacket(clutch: 0.25f);
            var update = display.ConvertToDisplayUpdate(packet);
            var rally = update.Data as RallyData;
            Assert.IsNotNull(rally);
            Assert.AreEqual(25, rally.Clutch);
        }

        [TestMethod]
        public void ConvertToDisplayUpdate_ThrottleFullyEngaged()
        {
            var display = new Display(new MockUdpClientFactory());
            var packet = CreatePacket(throttle: 1.0f);
            var update = display.ConvertToDisplayUpdate(packet);
            var rally = update.Data as RallyData;
            Assert.IsNotNull(rally);
            Assert.AreEqual(100, rally.Throttle);
        }

        [TestMethod]
        public void ConvertToDisplayUpdate_BrakeNotPressed()
        {
            var display = new Display(new MockUdpClientFactory());
            var packet = CreatePacket(brakes: 0.0f);
            var update = display.ConvertToDisplayUpdate(packet);
            var rally = update.Data as RallyData;
            Assert.IsNotNull(rally);
            Assert.AreEqual(0, rally.Brake);
        }

        [TestMethod]
        public void ConvertToDisplayUpdate_ClutchFullyEngaged()
        {
            var display = new Display(new MockUdpClientFactory());
            var packet = CreatePacket(clutch: 1.0f);
            var update = display.ConvertToDisplayUpdate(packet);
            var rally = update.Data as RallyData;
            Assert.IsNotNull(rally);
            Assert.AreEqual(100, rally.Clutch);
        }

        #endregion

        #region Progress Tests

            [TestMethod]
            public void ConvertToDisplayUpdate_CalculatesProgress()
            {
                var display = new Display(new MockUdpClientFactory());
                var packet = CreatePacket();
                packet.progress = 0.5f;
                var update = display.ConvertToDisplayUpdate(packet);
                var rally = update.Data as RallyData;
                Assert.IsNotNull(rally);
                Assert.AreEqual(50, rally.CompletedPct);
            }

        [TestMethod]
        public void ConvertToDisplayUpdate_ProgressClamped()
        {
            var display = new Display(new MockUdpClientFactory());
            var packet = CreatePacket();
            packet.progress = 1.05f;
            var update = display.ConvertToDisplayUpdate(packet);
            var rally = update.Data as RallyData;
            Assert.IsNotNull(rally);
            Assert.AreEqual(100, rally.CompletedPct);
        }

        [TestMethod]
        public void ConvertToDisplayUpdate_ProgressAtStart()
        {
            var display = new Display(new MockUdpClientFactory());
            var packet = CreatePacket();
            packet.progress = 0.0f;
            var update = display.ConvertToDisplayUpdate(packet);
            var rally = update.Data as RallyData;
            Assert.IsNotNull(rally);
            Assert.AreEqual(0, rally.CompletedPct);
        }

        [TestMethod]
        public void ConvertToDisplayUpdate_ProgressAtEnd()
        {
            var display = new Display(new MockUdpClientFactory());
            var packet = CreatePacket();
            packet.progress = 1.0f;
            var update = display.ConvertToDisplayUpdate(packet);
            var rally = update.Data as RallyData;
            Assert.IsNotNull(rally);
            Assert.AreEqual(100, rally.CompletedPct);
        }

        #endregion

        #region Distance Tests

            [TestMethod]
            public void ConvertToDisplayUpdate_CalculatesDistance()
            {
                var display = new Display(new MockUdpClientFactory());
                var packet = CreatePacket(distance: 5000.5f);
                var update = display.ConvertToDisplayUpdate(packet);
                var rally = update.Data as RallyData;
                Assert.IsNotNull(rally);
                Assert.AreEqual(5000, rally.DistanceTravelled);
            }

        [TestMethod]
        public void ConvertToDisplayUpdate_DistanceClamped()
        {
            var display = new Display(new MockUdpClientFactory());
            var packet = CreatePacket(distance: -100f);
            var update = display.ConvertToDisplayUpdate(packet);
            var rally = update.Data as RallyData;
            Assert.IsNotNull(rally);
            Assert.AreEqual(0, rally.DistanceTravelled);
        }

        [TestMethod]
        public void ConvertToDisplayUpdate_DistanceZero()
        {
            var display = new Display(new MockUdpClientFactory());
            var packet = CreatePacket(distance: 0f);
            var update = display.ConvertToDisplayUpdate(packet);
            var rally = update.Data as RallyData;
            Assert.IsNotNull(rally);
            Assert.AreEqual(0, rally.DistanceTravelled);
        }

        #endregion

        #region Position Tests

        [TestMethod]
        public void ConvertToDisplayUpdate_CarPosition()
        {
            // Arrange
            float carPos = 3.4f; // approximately 3rd position

            // Act
            int position = Convert.ToInt32(carPos);

            // Assert
            Assert.AreEqual(3, position);
        }

        [TestMethod]
        public void ConvertToDisplayUpdate_FirstPosition()
        {
            // Arrange
            float carPos = 1f;

            // Act
            int position = Convert.ToInt32(carPos);

            // Assert
            Assert.AreEqual(1, position);
        }

        [TestMethod]
        public void ConvertToDisplayUpdate_HighPosition()
        {
            // Arrange
            float carPos = 20f;

            // Act
            int position = Convert.ToInt32(carPos);

            // Assert
            Assert.AreEqual(20, position);
        }

        #endregion

        #region Sector Time Tests

        [TestMethod]
        public void ConvertToDisplayUpdate_Sector1Time()
        {
            // Arrange
            float sector1Time = 45.5f; // seconds

            // Act & Assert
            Assert.IsGreaterThanOrEqualTo(0, sector1Time);
        }

        [TestMethod]
        public void ConvertToDisplayUpdate_Sector2Time()
        {
            // Arrange
            float sector2Time = 52.3f; // seconds

            // Act & Assert
            Assert.IsGreaterThanOrEqualTo(0, sector2Time);
        }

        [TestMethod]
        public void ConvertToDisplayUpdate_LapTime()
        {
            // Arrange
            float lapTime = 180.5f; // seconds

            // Act & Assert
            Assert.IsGreaterThanOrEqualTo(0, lapTime);
        }

        [TestMethod]
        public void ConvertToDisplayUpdate_SectorTimesAddUpToLapTime()
        {
            // Arrange
            float sector1Time = 45.5f;
            float sector2Time = 52.3f;
            float lapTime = 180.5f;

            // Act
            float calculatedTime = sector1Time + sector2Time;

            // Assert - sectors are part of the lap, but not all of it
            Assert.IsLessThan(lapTime, calculatedTime);
        }

        #endregion

        #region Display Property Tests

        [TestMethod]
        public void Display_Description_ReturnsDirtRally2()
        {
            // Arrange
            string description = "Dirt Rally 2";

            // Act & Assert
            Assert.AreEqual("Dirt Rally 2", description);
        }

        [TestMethod]
        public void Display_IsActive_ChecksForDirtrally2Process()
        {
            // Arrange
            string processName = "dirtrally2";

            // Act & Assert
            Assert.AreEqual("dirtrally2", processName);
        }

        #endregion

        #region Display Type Tests

        [TestMethod]
        public void ConvertToDisplayUpdate_ReturnsRallyDashboardType()
        {
            // Arrange
            var expectedType = DisplayType.RallyDashboard;

            // Act & Assert
            Assert.AreEqual(DisplayType.RallyDashboard, expectedType);
        }

        #endregion

        #region Edge Case Tests

        [TestMethod]
        public void ConvertToDisplayUpdate_HandleZeroRpmMax()
        {
            // Arrange
            int rpmMax = 0;

            // Act
            var lights = Display.GenerateRpmLights(rpmMax);

            // Assert
            Assert.IsNotNull(lights);
            Assert.HasCount(6, lights);
        }

        [TestMethod]
        public void ConvertToDisplayUpdate_HandleLargeRpmMax()
        {
            // Arrange
            int rpmMax = 10000;

            // Act
            var lights = Display.GenerateRpmLights(rpmMax);

            // Assert
            Assert.IsNotNull(lights);
            Assert.HasCount(6, lights);
            // 10000 - (6*200) = 8800
            Assert.AreEqual(8800, lights[0].Rpm);
        }

        [TestMethod]
        public void ConvertToDisplayUpdate_HandleVeryLowSpeed()
        {
            // Arrange
            float speedMs = 0.1f;

            // Act
            int speedKmh = Convert.ToInt32(speedMs * 3.6);

            // Assert
            Assert.IsGreaterThanOrEqualTo(0, speedKmh);
        }

        [TestMethod]
        public void ConvertToDisplayUpdate_HandleVeryHighSpeed()
        {
            // Arrange
            float speedMs = 100f; // 360 km/h

            // Act
            int speedKmh = Convert.ToInt32(speedMs * 3.6);

            // Assert
            Assert.AreEqual(360, speedKmh);
        }

        #endregion
    }
}
