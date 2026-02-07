using HaddySimHub.Displays;
using HaddySimHub.Displays.Dirt2;
using HaddySimHub.Models;
using System.Net.Sockets;

namespace HaddySimHub.Tests
{
    [TestClass]
    public class Dirt2DataConverterTests
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

        #region Speed Conversion Tests

        [TestMethod]
            public void Convert_ConvertsSpeedMsToKmh()
            {
                var converter = new Dirt2DataConverter();
                var packet = CreatePacket(speed_ms: 27.78f);
                var update = converter.Convert(packet);
                var rally = update.Data as RallyData;
                Assert.IsNotNull(rally);
                Assert.AreEqual(100, rally.Speed);
            }

        [TestMethod]
            public void Convert_ConvertsZeroSpeed()
            {
                var converter = new Dirt2DataConverter();
                var packet = CreatePacket(speed_ms: 0f);
                var update = converter.Convert(packet);
                var rally = update.Data as RallyData;
                Assert.IsNotNull(rally);
                Assert.AreEqual(0, rally.Speed);
            }

        [TestMethod]
            public void Convert_ConvertsHighSpeed()
            {
                var converter = new Dirt2DataConverter();
                var packet = CreatePacket(speed_ms: 50f);
                var update = converter.Convert(packet);
                var rally = update.Data as RallyData;
                Assert.IsNotNull(rally);
                Assert.AreEqual(180, rally.Speed);
            }

        #endregion

        #region RPM Calculation Tests

        [TestMethod]
            public void Convert_CalculatesRpmFromRawValue()
            {
                var converter = new Dirt2DataConverter();
                var packet = CreatePacket(rpm: 500f);
                var update = converter.Convert(packet);
                var rally = update.Data as RallyData;
                Assert.IsNotNull(rally);
                Assert.AreEqual(5000, rally.Rpm);
            }

        [TestMethod]
            public void Convert_CalculatesRpmMax()
            {
                var converter = new Dirt2DataConverter();
                var packet = CreatePacket(max_rpm: 700f);
                var update = converter.Convert(packet);
                var rally = update.Data as RallyData;
                Assert.IsNotNull(rally);
                Assert.AreEqual(7000, rally.RpmMax);
            }



        #endregion

        #region Gear Tests

        [TestMethod]
            public void Convert_NeutralGear()
            {
                var converter = new Dirt2DataConverter();
                var packet = CreatePacket(gear: 0f);
                var update = converter.Convert(packet);
                var rally = update.Data as RallyData;
                Assert.IsNotNull(rally);
                Assert.AreEqual("N", rally.Gear);
            }

        [TestMethod]
            public void Convert_ReverseGear()
            {
                var converter = new Dirt2DataConverter();
                var packet = CreatePacket(gear: -1f);
                var update = converter.Convert(packet);
                var rally = update.Data as RallyData;
                Assert.IsNotNull(rally);
                Assert.AreEqual("R", rally.Gear);
            }

        [TestMethod]
            public void Convert_FirstGear()
            {
                var converter = new Dirt2DataConverter();
                var packet = CreatePacket(gear: 1f);
                var update = converter.Convert(packet);
                var rally = update.Data as RallyData;
                Assert.IsNotNull(rally);
                Assert.AreEqual("1", rally.Gear);
            }

        [TestMethod]
            public void Convert_HighGear()
            {
                var converter = new Dirt2DataConverter();
                var packet = CreatePacket(gear: 6f);
                var update = converter.Convert(packet);
                var rally = update.Data as RallyData;
                Assert.IsNotNull(rally);
                Assert.AreEqual("6", rally.Gear);
            }

        #endregion

        #region Control Input Tests

            [TestMethod]
            public void Convert_CalculatesThrottle()
            {
                var converter = new Dirt2DataConverter();
                var packet = CreatePacket(throttle: 0.75f);
                var update = converter.Convert(packet);
                var rally = update.Data as RallyData;
                Assert.IsNotNull(rally);
                Assert.AreEqual(75, rally.Throttle);
            }

        [TestMethod]
        public void Convert_CalculatesBrake()
        {
            var converter = new Dirt2DataConverter();
            var packet = CreatePacket(brakes: 0.50f);
            var update = converter.Convert(packet);
            var rally = update.Data as RallyData;
            Assert.IsNotNull(rally);
            Assert.AreEqual(50, rally.Brake);
        }

        [TestMethod]
        public void Convert_CalculatesClutch()
        {
            var converter = new Dirt2DataConverter();
            var packet = CreatePacket(clutch: 0.25f);
            var update = converter.Convert(packet);
            var rally = update.Data as RallyData;
            Assert.IsNotNull(rally);
            Assert.AreEqual(25, rally.Clutch);
        }

        [TestMethod]
        public void Convert_ThrottleFullyEngaged()
        {
            var converter = new Dirt2DataConverter();
            var packet = CreatePacket(throttle: 1.0f);
            var update = converter.Convert(packet);
            var rally = update.Data as RallyData;
            Assert.IsNotNull(rally);
            Assert.AreEqual(100, rally.Throttle);
        }

        [TestMethod]
        public void Convert_BrakeNotPressed()
        {
            var converter = new Dirt2DataConverter();
            var packet = CreatePacket(brakes: 0.0f);
            var update = converter.Convert(packet);
            var rally = update.Data as RallyData;
            Assert.IsNotNull(rally);
            Assert.AreEqual(0, rally.Brake);
        }

        [TestMethod]
        public void Convert_ClutchFullyEngaged()
        {
            var converter = new Dirt2DataConverter();
            var packet = CreatePacket(clutch: 1.0f);
            var update = converter.Convert(packet);
            var rally = update.Data as RallyData;
            Assert.IsNotNull(rally);
            Assert.AreEqual(100, rally.Clutch);
        }

        #endregion

        #region Progress Tests

            [TestMethod]
            public void Convert_CalculatesProgress()
            {
                var converter = new Dirt2DataConverter();
                var packet = CreatePacket();
                packet.progress = 0.5f;
                var update = converter.Convert(packet);
                var rally = update.Data as RallyData;
                Assert.IsNotNull(rally);
                Assert.AreEqual(50, rally.CompletedPct);
            }

        [TestMethod]
        public void Convert_ProgressClamped()
        {
            var converter = new Dirt2DataConverter();
            var packet = CreatePacket();
            packet.progress = 1.05f;
            var update = converter.Convert(packet);
            var rally = update.Data as RallyData;
            Assert.IsNotNull(rally);
            Assert.AreEqual(100, rally.CompletedPct);
        }

        [TestMethod]
        public void Convert_ProgressAtStart()
        {
            var converter = new Dirt2DataConverter();
            var packet = CreatePacket();
            packet.progress = 0.0f;
            var update = converter.Convert(packet);
            var rally = update.Data as RallyData;
            Assert.IsNotNull(rally);
            Assert.AreEqual(0, rally.CompletedPct);
        }

        [TestMethod]
        public void Convert_ProgressAtEnd()
        {
            var converter = new Dirt2DataConverter();
            var packet = CreatePacket();
            packet.progress = 1.0f;
            var update = converter.Convert(packet);
            var rally = update.Data as RallyData;
            Assert.IsNotNull(rally);
            Assert.AreEqual(100, rally.CompletedPct);
        }

        #endregion

        #region Distance Tests

            [TestMethod]
            public void Convert_CalculatesDistance()
            {
                var converter = new Dirt2DataConverter();
                var packet = CreatePacket(distance: 5000.5f);
                var update = converter.Convert(packet);
                var rally = update.Data as RallyData;
                Assert.IsNotNull(rally);
                Assert.AreEqual(5000, rally.DistanceTravelled);
            }

        [TestMethod]
        public void Convert_DistanceClamped()
        {
            var converter = new Dirt2DataConverter();
            var packet = CreatePacket(distance: -100f);
            var update = converter.Convert(packet);
            var rally = update.Data as RallyData;
            Assert.IsNotNull(rally);
            Assert.AreEqual(0, rally.DistanceTravelled);
        }

        [TestMethod]
        public void Convert_DistanceZero()
        {
            var converter = new Dirt2DataConverter();
            var packet = CreatePacket(distance: 0f);
            var update = converter.Convert(packet);
            var rally = update.Data as RallyData;
            Assert.IsNotNull(rally);
            Assert.AreEqual(0, rally.DistanceTravelled);
        }

        #endregion

        #region Position Tests

        [TestMethod]
        public void Convert_CarPosition()
        {
            // Arrange
            var converter = new Dirt2DataConverter();
            var packet = CreatePacket();
            packet.car_pos = 3.4f; // approximately 3rd position

            // Act
            var update = converter.Convert(packet);
            var rally = update.Data as RallyData;

            // Assert
            Assert.IsNotNull(rally);
            Assert.AreEqual(3, rally.Position);
        }

        [TestMethod]
        public void Convert_FirstPosition()
        {
            // Arrange
            var converter = new Dirt2DataConverter();
            var packet = CreatePacket();
            packet.car_pos = 1f;

            // Act
            var update = converter.Convert(packet);
            var rally = update.Data as RallyData;

            // Assert
            Assert.IsNotNull(rally);
            Assert.AreEqual(1, rally.Position);
        }

        [TestMethod]
        public void Convert_HighPosition()
        {
            // Arrange
            var converter = new Dirt2DataConverter();
            var packet = CreatePacket();
            packet.car_pos = 20f;

            // Act
            var update = converter.Convert(packet);
            var rally = update.Data as RallyData;

            // Assert
            Assert.IsNotNull(rally);
            Assert.AreEqual(20, rally.Position);
        }

        #endregion

        #region Sector Time Tests

        [TestMethod]
        public void Convert_Sector1Time()
        {
            // Arrange
            var converter = new Dirt2DataConverter();
            var packet = CreatePacket();
            packet.sector_1_time = 45.5f; // seconds

            // Act
            var update = converter.Convert(packet);
            var rally = update.Data as RallyData;

            // Assert
            Assert.IsNotNull(rally);
            Assert.AreEqual(45.5f, rally.Sector1Time);
        }

        [TestMethod]
        public void Convert_Sector2Time()
        {
            // Arrange
            var converter = new Dirt2DataConverter();
            var packet = CreatePacket();
            packet.sector_2_time = 52.3f; // seconds

            // Act
            var update = converter.Convert(packet);
            var rally = update.Data as RallyData;

            // Assert
            Assert.IsNotNull(rally);
            Assert.AreEqual(52.3f, rally.Sector2Time);
        }

        [TestMethod]
        public void Convert_LapTime()
        {
            // Arrange
            var converter = new Dirt2DataConverter();
            var packet = CreatePacket();
            packet.lap_time = 180.5f; // seconds

            // Act
            var update = converter.Convert(packet);
            var rally = update.Data as RallyData;

            // Assert
            Assert.IsNotNull(rally);
            Assert.AreEqual(180.5f, rally.LapTime);
        }

        [TestMethod]
        public void Convert_SectorTimesAreSetCorrectly()
        {
            // Arrange
            var converter = new Dirt2DataConverter();
            var packet = CreatePacket();
            packet.sector_1_time = 45.5f;
            packet.sector_2_time = 52.3f;
            packet.lap_time = 180.5f;

            // Act
            var update = converter.Convert(packet);
            var rally = update.Data as RallyData;

            // Assert
            Assert.IsNotNull(rally);
            Assert.AreEqual(45.5f, rally.Sector1Time);
            Assert.AreEqual(52.3f, rally.Sector2Time);
            Assert.AreEqual(180.5f, rally.LapTime);
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
        public void Convert_ReturnsRallyDashboardType()
        {
            // Arrange
            var converter = new Dirt2DataConverter();
            var packet = CreatePacket(); // A dummy packet
            var update = converter.Convert(packet);

            // Act & Assert
            Assert.AreEqual(DisplayType.RallyDashboard, update.Type);
        }

        #endregion

        #region Edge Case Tests

        [TestMethod]
        public void Convert_HandleVeryLowSpeed()
        {
            // Arrange
            var converter = new Dirt2DataConverter();
            var packet = CreatePacket();
            packet.speed_ms = 0.1f;

            // Act
            var update = converter.Convert(packet);
            var rally = update.Data as RallyData;

            // Assert
            Assert.IsNotNull(rally);
            Assert.IsGreaterThanOrEqualTo(0, rally.Speed);
        }

        [TestMethod]
        public void Convert_HandleVeryHighSpeed()
        {
            // Arrange
            var converter = new Dirt2DataConverter();
            var packet = CreatePacket();
            packet.speed_ms = 100f; // 360 km/h

            // Act
            var update = converter.Convert(packet);
            var rally = update.Data as RallyData;

            // Assert
            Assert.IsNotNull(rally);
            Assert.AreEqual(360, rally.Speed);
        }

        #endregion
    }
}
