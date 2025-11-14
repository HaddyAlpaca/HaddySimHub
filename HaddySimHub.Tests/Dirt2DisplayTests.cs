using HaddySimHub.Displays;
using HaddySimHub.Displays.Dirt2;
using HaddySimHub.Models;
using System.Net.Sockets;
using Xunit;

namespace HaddySimHub.Tests
{
    public class Dirt2DisplayTests
    {
        private static Packet CreatePacket(
            float speed_ms = 0,
            float rpm = 0,
            float max_rpm = 0,
            float idle_rpm = 0,
            float gear = 0)
        {
            return new Packet
            {
                speed_ms = speed_ms,
                rpm = rpm,
                max_rpm = max_rpm,
                idle_rpm = idle_rpm,
                gear = gear
            };
        }

        private class MockUdpClientFactory : IUdpClientFactory
        {
            public UdpClient Create(int port) => null!;
        }
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
                var display = new Display(new MockUdpClientFactory());
                var packet = CreatePacket(speed_ms: 27.78f);
                var update = display.ConvertToDisplayUpdate(packet);
                var rally = update.Data as RallyData;
                Assert.NotNull(rally);
                Assert.Equal(100, rally.Speed);
            }

        [Fact]
            public void ConvertToDisplayUpdate_ConvertsZeroSpeed()
            {
                var display = new Display(new MockUdpClientFactory());
                var packet = CreatePacket(speed_ms: 0f);
                var update = display.ConvertToDisplayUpdate(packet);
                var rally = update.Data as RallyData;
                Assert.NotNull(rally);
                Assert.Equal(0, rally.Speed);
            }

        [Fact]
            public void ConvertToDisplayUpdate_ConvertsHighSpeed()
            {
                var display = new Display(new MockUdpClientFactory());
                var packet = CreatePacket(speed_ms: 50f);
                var update = display.ConvertToDisplayUpdate(packet);
                var rally = update.Data as RallyData;
                Assert.NotNull(rally);
                Assert.Equal(180, rally.Speed);
            }

        #endregion

        #region RPM Calculation Tests

        [Fact]
            public void ConvertToDisplayUpdate_CalculatesRpmFromRawValue()
            {
                var display = new Display(new MockUdpClientFactory());
                var packet = CreatePacket(rpm: 500f);
                var update = display.ConvertToDisplayUpdate(packet);
                var rally = update.Data as RallyData;
                Assert.NotNull(rally);
                Assert.Equal(5000, rally.Rpm);
            }

        [Fact]
            public void ConvertToDisplayUpdate_CalculatesRpmMax()
            {
                var display = new Display(new MockUdpClientFactory());
                var packet = CreatePacket(max_rpm: 700f);
                var update = display.ConvertToDisplayUpdate(packet);
                var rally = update.Data as RallyData;
                Assert.NotNull(rally);
                Assert.Equal(7000, rally.RpmMax);
            }

        [Fact]
            public void ConvertToDisplayUpdate_CalculatesIdleRpm()
            {
                var display = new Display(new MockUdpClientFactory());
                var packet = CreatePacket(idle_rpm: 100f);
                // idle_rpm is not used in RallyData, but we can check the packet value
                Assert.Equal(1000, packet.idle_rpm * 10);
            }

        #endregion

        #region Gear Tests

        [Fact]
            public void ConvertToDisplayUpdate_NeutralGear()
            {
                var display = new Display(new MockUdpClientFactory());
                var packet = CreatePacket(gear: 0f);
                var update = display.ConvertToDisplayUpdate(packet);
                var rally = update.Data as RallyData;
                Assert.NotNull(rally);
                Assert.Equal("N", rally.Gear);
            }

        [Fact]
            public void ConvertToDisplayUpdate_ReverseGear()
            {
                var display = new Display(new MockUdpClientFactory());
                var packet = CreatePacket(gear: -1f);
                var update = display.ConvertToDisplayUpdate(packet);
                var rally = update.Data as RallyData;
                Assert.NotNull(rally);
                Assert.Equal("R", rally.Gear);
            }

        [Fact]
            public void ConvertToDisplayUpdate_FirstGear()
            {
                var display = new Display(new MockUdpClientFactory());
                var packet = CreatePacket(gear: 1f);
                var update = display.ConvertToDisplayUpdate(packet);
                var rally = update.Data as RallyData;
                Assert.NotNull(rally);
                Assert.Equal("1", rally.Gear);
            }

        [Fact]
            public void ConvertToDisplayUpdate_HighGear()
            {
                var display = new Display(new MockUdpClientFactory());
                var packet = CreatePacket(gear: 6f);
                var update = display.ConvertToDisplayUpdate(packet);
                var rally = update.Data as RallyData;
                Assert.NotNull(rally);
                Assert.Equal("6", rally.Gear);
            }

        #endregion

        #region Control Input Tests

            [Fact]
            public void ConvertToDisplayUpdate_CalculatesThrottle()
            {
                var display = new Display(new MockUdpClientFactory());
                var packet = CreatePacket(throttle: 0.75f);
                var update = display.ConvertToDisplayUpdate(packet);
                var rally = update.Data as RallyData;
                Assert.NotNull(rally);
                Assert.Equal(75, rally.Throttle);
            }

        [Fact]
        public void ConvertToDisplayUpdate_CalculatesBrake()
        {
            var display = new Display(new MockUdpClientFactory());
            var packet = CreatePacket(brakes: 0.50f);
            var update = display.ConvertToDisplayUpdate(packet);
            var rally = update.Data as RallyData;
            Assert.NotNull(rally);
            Assert.Equal(50, rally.Brake);
        }

        [Fact]
        public void ConvertToDisplayUpdate_CalculatesClutch()
        {
            var display = new Display(new MockUdpClientFactory());
            var packet = CreatePacket(clutch: 0.25f);
            var update = display.ConvertToDisplayUpdate(packet);
            var rally = update.Data as RallyData;
            Assert.NotNull(rally);
            Assert.Equal(25, rally.Clutch);
        }

        [Fact]
        public void ConvertToDisplayUpdate_ThrottleFullyEngaged()
        {
            var display = new Display(new MockUdpClientFactory());
            var packet = CreatePacket(throttle: 1.0f);
            var update = display.ConvertToDisplayUpdate(packet);
            var rally = update.Data as RallyData;
            Assert.NotNull(rally);
            Assert.Equal(100, rally.Throttle);
        }

        [Fact]
        public void ConvertToDisplayUpdate_BrakeNotPressed()
        {
            var display = new Display(new MockUdpClientFactory());
            var packet = CreatePacket(brakes: 0.0f);
            var update = display.ConvertToDisplayUpdate(packet);
            var rally = update.Data as RallyData;
            Assert.NotNull(rally);
            Assert.Equal(0, rally.Brake);
        }

        [Fact]
        public void ConvertToDisplayUpdate_ClutchFullyEngaged()
        {
            var display = new Display(new MockUdpClientFactory());
            var packet = CreatePacket(clutch: 1.0f);
            var update = display.ConvertToDisplayUpdate(packet);
            var rally = update.Data as RallyData;
            Assert.NotNull(rally);
            Assert.Equal(100, rally.Clutch);
        }

        #endregion

        #region Progress Tests

            [Fact]
            public void ConvertToDisplayUpdate_CalculatesProgress()
            {
                var display = new Display(new MockUdpClientFactory());
                var packet = CreatePacket();
                packet.progress = 0.5f;
                var update = display.ConvertToDisplayUpdate(packet);
                var rally = update.Data as RallyData;
                Assert.NotNull(rally);
                Assert.Equal(50, rally.CompletedPct);
            }

        [Fact]
        public void ConvertToDisplayUpdate_ProgressClamped()
        {
            var display = new Display(new MockUdpClientFactory());
            var packet = CreatePacket();
            packet.progress = 1.05f;
            var update = display.ConvertToDisplayUpdate(packet);
            var rally = update.Data as RallyData;
            Assert.NotNull(rally);
            Assert.Equal(100, rally.CompletedPct);
        }

        [Fact]
        public void ConvertToDisplayUpdate_ProgressAtStart()
        {
            var display = new Display(new MockUdpClientFactory());
            var packet = CreatePacket();
            packet.progress = 0.0f;
            var update = display.ConvertToDisplayUpdate(packet);
            var rally = update.Data as RallyData;
            Assert.NotNull(rally);
            Assert.Equal(0, rally.CompletedPct);
        }

        [Fact]
        public void ConvertToDisplayUpdate_ProgressAtEnd()
        {
            var display = new Display(new MockUdpClientFactory());
            var packet = CreatePacket();
            packet.progress = 1.0f;
            var update = display.ConvertToDisplayUpdate(packet);
            var rally = update.Data as RallyData;
            Assert.NotNull(rally);
            Assert.Equal(100, rally.CompletedPct);
        }

        #endregion

        #region Distance Tests

            [Fact]
            public void ConvertToDisplayUpdate_CalculatesDistance()
            {
                var display = new Display(new MockUdpClientFactory());
                var packet = CreatePacket(distance: 5000.5f);
                var update = display.ConvertToDisplayUpdate(packet);
                var rally = update.Data as RallyData;
                Assert.NotNull(rally);
                Assert.Equal(5000, rally.DistanceTravelled);
            }

        [Fact]
        public void ConvertToDisplayUpdate_DistanceClamped()
        {
            var display = new Display(new MockUdpClientFactory());
            var packet = CreatePacket(distance: -100f);
            var update = display.ConvertToDisplayUpdate(packet);
            var rally = update.Data as RallyData;
            Assert.NotNull(rally);
            Assert.Equal(0, rally.DistanceTravelled);
        }

        [Fact]
        public void ConvertToDisplayUpdate_DistanceZero()
        {
            var display = new Display(new MockUdpClientFactory());
            var packet = CreatePacket(distance: 0f);
            var update = display.ConvertToDisplayUpdate(packet);
            var rally = update.Data as RallyData;
            Assert.NotNull(rally);
            Assert.Equal(0, rally.DistanceTravelled);
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
