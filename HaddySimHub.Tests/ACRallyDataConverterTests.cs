using HaddySimHub.Displays.ACRally;
using HaddySimHub.Models;
using System.Runtime.InteropServices;

namespace HaddySimHub.Tests
{
    [TestClass]
    public class ACRallyDataConverterTests
    {
        private static ACRallyTelemetry CreateTelemetry(
            float speedMps = 0,
            float rpm = 0,
            float maxRpm = 0,
            float gear = 0,
            float throttleInput = 0,
            float brakeInput = 0,
            float clutchInput = 0,
            int currentLapTime = 0,
            float normalizedSplinePos = 0,
            int currentLap = 0,
            int totalLaps = 0)
        {
            var telemetry = new ACRallyTelemetry
            {
                SpeedMps = speedMps,
                Rpm = rpm,
                MaxRpm = maxRpm,
                Gear = gear,
                ThrottleInput = throttleInput,
                BrakeInput = brakeInput,
                ClutchInput = clutchInput,
                CurrentLapTime = currentLapTime,
                NormalizedSplinePosTrack = normalizedSplinePos,
                CurrentLap = currentLap,
                TotalLaps = totalLaps,
                LocalAngularVelocityVector = new float[3]
            };
            return telemetry;
        }

        #region Speed Conversion Tests

        [TestMethod]
        public void Convert_ConvertsSpeedMsToKmh()
        {
            var converter = new ACRallyDataConverter();
            var telemetry = CreateTelemetry(speedMps: 27.78f); // 100 km/h
            var update = converter.Convert(telemetry);
            var rally = update.Data as RallyData;
            
            Assert.IsNotNull(rally);
            Assert.AreEqual(100, rally.Speed); // 27.78 * 3.6 = 100
        }

        [TestMethod]
        public void Convert_ZeroSpeedRemains()
        {
            var converter = new ACRallyDataConverter();
            var telemetry = CreateTelemetry(speedMps: 0);
            var update = converter.Convert(telemetry);
            var rally = update.Data as RallyData;
            
            Assert.IsNotNull(rally);
            Assert.AreEqual(0, rally.Speed);
        }

        [TestMethod]
        public void Convert_HighSpeedConvertsCorrectly()
        {
            var converter = new ACRallyDataConverter();
            var telemetry = CreateTelemetry(speedMps: 55.56f); // 200 km/h
            var update = converter.Convert(telemetry);
            var rally = update.Data as RallyData;
            
            Assert.IsNotNull(rally);
            Assert.AreEqual(200, rally.Speed);
        }

        #endregion

        #region RPM Conversion Tests

        [TestMethod]
        public void Convert_ConvertsRPMCorrectly()
        {
            var converter = new ACRallyDataConverter();
            var telemetry = CreateTelemetry(rpm: 3000, maxRpm: 7500);
            var update = converter.Convert(telemetry);
            var rally = update.Data as RallyData;
            
            Assert.IsNotNull(rally);
            Assert.AreEqual(30000, rally.Rpm);
            Assert.AreEqual(75000, rally.RpmMax);
        }

        [TestMethod]
        public void Convert_ZeroRPMRemains()
        {
            var converter = new ACRallyDataConverter();
            var telemetry = CreateTelemetry(rpm: 0);
            var update = converter.Convert(telemetry);
            var rally = update.Data as RallyData;
            
            Assert.IsNotNull(rally);
            Assert.AreEqual(0, rally.Rpm);
        }

        #endregion

        #region Gear Conversion Tests

        [TestMethod]
        public void Convert_NeutralGearReturnsN()
        {
            var converter = new ACRallyDataConverter();
            var telemetry = CreateTelemetry(gear: 0);
            var update = converter.Convert(telemetry);
            var rally = update.Data as RallyData;
            
            Assert.IsNotNull(rally);
            Assert.AreEqual("N", rally.Gear);
        }

        [TestMethod]
        public void Convert_ReverseGearReturnsR()
        {
            var converter = new ACRallyDataConverter();
            var telemetry = CreateTelemetry(gear: -1);
            var update = converter.Convert(telemetry);
            var rally = update.Data as RallyData;
            
            Assert.IsNotNull(rally);
            Assert.AreEqual("R", rally.Gear);
        }

        [TestMethod]
        public void Convert_FirstGearReturns1()
        {
            var converter = new ACRallyDataConverter();
            var telemetry = CreateTelemetry(gear: 1);
            var update = converter.Convert(telemetry);
            var rally = update.Data as RallyData;
            
            Assert.IsNotNull(rally);
            Assert.AreEqual("1", rally.Gear);
        }

        [TestMethod]
        public void Convert_ThirdGearReturns3()
        {
            var converter = new ACRallyDataConverter();
            var telemetry = CreateTelemetry(gear: 3);
            var update = converter.Convert(telemetry);
            var rally = update.Data as RallyData;
            
            Assert.IsNotNull(rally);
            Assert.AreEqual("3", rally.Gear);
        }

        #endregion

        #region Throttle/Brake Conversion Tests

        [TestMethod]
        public void Convert_ConvertsThrottlePercentage()
        {
            var converter = new ACRallyDataConverter();
            var telemetry = CreateTelemetry(throttleInput: 0.75f);
            var update = converter.Convert(telemetry);
            var rally = update.Data as RallyData;
            
            Assert.IsNotNull(rally);
            Assert.AreEqual(75, rally.Throttle);
        }

        [TestMethod]
        public void Convert_ConvertsBrakePercentage()
        {
            var converter = new ACRallyDataConverter();
            var telemetry = CreateTelemetry(brakeInput: 0.5f);
            var update = converter.Convert(telemetry);
            var rally = update.Data as RallyData;
            
            Assert.IsNotNull(rally);
            Assert.AreEqual(50, rally.Brake);
        }

        [TestMethod]
        public void Convert_ConvertsClutchPercentage()
        {
            var converter = new ACRallyDataConverter();
            var telemetry = CreateTelemetry(clutchInput: 0.25f);
            var update = converter.Convert(telemetry);
            var rally = update.Data as RallyData;
            
            Assert.IsNotNull(rally);
            Assert.AreEqual(25, rally.Clutch);
        }

        [TestMethod]
        public void Convert_MaxThrottleIsOneHundred()
        {
            var converter = new ACRallyDataConverter();
            var telemetry = CreateTelemetry(throttleInput: 1.0f);
            var update = converter.Convert(telemetry);
            var rally = update.Data as RallyData;
            
            Assert.IsNotNull(rally);
            Assert.AreEqual(100, rally.Throttle);
        }

        #endregion

        #region Stage Progress Tests

        [TestMethod]
        public void Convert_CalculatesCompletedPercentageFromLapCount()
        {
            var converter = new ACRallyDataConverter();
            var telemetry = CreateTelemetry(currentLap: 2, totalLaps: 5);
            var update = converter.Convert(telemetry);
            var rally = update.Data as RallyData;
            
            Assert.IsNotNull(rally);
            Assert.AreEqual(40, rally.CompletedPct); // 2/5 * 100 = 40
        }

        [TestMethod]
        public void Convert_CalculatesCompletedPercentageFromSplinePosition()
        {
            var converter = new ACRallyDataConverter();
            var telemetry = CreateTelemetry(normalizedSplinePos: 0.65f);
            var update = converter.Convert(telemetry);
            var rally = update.Data as RallyData;
            
            Assert.IsNotNull(rally);
            Assert.AreEqual(65, rally.CompletedPct);
        }

        [TestMethod]
        public void Convert_CompletedPercentageCappedAt100()
        {
            var converter = new ACRallyDataConverter();
            var telemetry = CreateTelemetry(currentLap: 10, totalLaps: 5);
            var update = converter.Convert(telemetry);
            var rally = update.Data as RallyData;
            
            Assert.IsNotNull(rally);
            Assert.AreEqual(100, rally.CompletedPct);
        }

        #endregion

        #region Display Type Tests

        [TestMethod]
        public void Convert_ReturnsRallyDashboardType()
        {
            var converter = new ACRallyDataConverter();
            var telemetry = CreateTelemetry();
            var update = converter.Convert(telemetry);
            
            Assert.AreEqual(DisplayType.RallyDashboard, update.Type);
        }

        #endregion

        #region Stage Time Tests

        [TestMethod]
        public void Convert_ConvertsCurrentLapTimeToSeconds()
        {
            var converter = new ACRallyDataConverter();
            var telemetry = CreateTelemetry(currentLapTime: 120000); // 120 seconds in ms
            var update = converter.Convert(telemetry);
            var rally = update.Data as RallyData;
            
            Assert.IsNotNull(rally);
            Assert.AreEqual(120f, rally.LapTime);
        }

        [TestMethod]
        public void Convert_SectorTimesAreHalves()
        {
            var converter = new ACRallyDataConverter();
            var telemetry = CreateTelemetry(currentLapTime: 100000); // 100 seconds
            var update = converter.Convert(telemetry);
            var rally = update.Data as RallyData;
            
            Assert.IsNotNull(rally);
            // Sectors are split 50/50
            Assert.AreEqual(50f, rally.Sector1Time);
            Assert.AreEqual(50f, rally.Sector2Time);
        }

        #endregion
    }
}
