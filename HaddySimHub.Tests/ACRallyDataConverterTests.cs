using HaddySimHub.Displays.ACRally;
using HaddySimHub.Models;
using System.Runtime.InteropServices;

namespace HaddySimHub.Tests
{
    [TestClass]
    public class ACRallyDataConverterTests
    {
        private static ACRallyTelemetry CreateTelemetry(
            float speedKmh = 0,
            int rpms = 0,
            int maxRpm = 0,
            float currentMaxRpm = 0,
            int gear = 1,
            int currentTime = 0,
            float normalizedCarPosition = 0,
            float distanceTraveled = 0,
            float trackSplineLength = 0,
            int currentSectorIndex = 0,
            int position = 0)
        {
            return new ACRallyTelemetry
            {
                SpeedKmh = speedKmh,
                Rpms = rpms,
                MaxRpm = maxRpm,
                CurrentMaxRpm = currentMaxRpm,
                Gear = gear,
                CurrentTime = currentTime,
                NormalizedCarPosition = normalizedCarPosition,
                DistanceTraveled = distanceTraveled,
                TrackSplineLength = trackSplineLength,
                CurrentSectorIndex = currentSectorIndex,
                Position = position,
                CarModel = string.Empty,
                Track = string.Empty,
                SmVersion = string.Empty,
                AcVersion = string.Empty,
            };
        }

        private static RallyData ConvertToRallyData(ACRallyDataConverter converter, ACRallyTelemetry telemetry)
        {
            var update = converter.Convert(telemetry);
            var rally = update.Data as RallyData;
            Assert.IsNotNull(rally);
            return rally;
        }

        #region Shared memory layout

        [TestMethod]
        public void Physics_MatchesSharedMemoryPageSize()
        {
            // The pages are read as raw memory images, so a layout change that alters
            // the size means every field past the change is being read from the wrong offset.
            Assert.AreEqual(800, Marshal.SizeOf<ACRallyPhysics>());
        }

        [TestMethod]
        public void Graphics_MatchesSharedMemoryPageSize()
        {
            Assert.AreEqual(1588, Marshal.SizeOf<ACRallyGraphics>());
        }

        [TestMethod]
        public void Static_MatchesSharedMemoryPageSize()
        {
            Assert.AreEqual(820, Marshal.SizeOf<ACRallyStatic>());
        }

        #endregion

        #region Speed

        [TestMethod]
        public void Convert_UsesSpeedInKmhDirectly()
        {
            // The physics page already reports km/h; no conversion should be applied.
            var rally = ConvertToRallyData(new ACRallyDataConverter(), CreateTelemetry(speedKmh: 100));

            Assert.AreEqual(100, rally.Speed);
        }

        [TestMethod]
        public void Convert_ZeroSpeedRemains()
        {
            var rally = ConvertToRallyData(new ACRallyDataConverter(), CreateTelemetry(speedKmh: 0));

            Assert.AreEqual(0, rally.Speed);
        }

        [TestMethod]
        public void Convert_ClampsNegativeSpeedToZero()
        {
            var rally = ConvertToRallyData(new ACRallyDataConverter(), CreateTelemetry(speedKmh: -3));

            Assert.AreEqual(0, rally.Speed);
        }

        #endregion

        #region RPM

        [TestMethod]
        public void Convert_UsesRawRpmAndStaticRevLimit()
        {
            var rally = ConvertToRallyData(new ACRallyDataConverter(), CreateTelemetry(rpms: 3000, maxRpm: 7500));

            Assert.AreEqual(3000, rally.Rpm);
            Assert.AreEqual(7500, rally.RpmMax);
        }

        [TestMethod]
        public void Convert_FallsBackToPhysicsRevLimitWhenStaticPageIsEmpty()
        {
            // The static page is written once per session and reads as zero before that.
            var rally = ConvertToRallyData(
                new ACRallyDataConverter(),
                CreateTelemetry(rpms: 3000, maxRpm: 0, currentMaxRpm: 7200));

            Assert.AreEqual(7200, rally.RpmMax);
        }

        #endregion

        #region Gear

        [TestMethod]
        public void Convert_ReverseGearReturnsR()
        {
            // Assetto Corsa encodes gears as 0 = reverse, 1 = neutral, 2 = first.
            var rally = ConvertToRallyData(new ACRallyDataConverter(), CreateTelemetry(gear: 0));

            Assert.AreEqual("R", rally.Gear);
        }

        [TestMethod]
        public void Convert_NeutralGearReturnsN()
        {
            var rally = ConvertToRallyData(new ACRallyDataConverter(), CreateTelemetry(gear: 1));

            Assert.AreEqual("N", rally.Gear);
        }

        [TestMethod]
        public void Convert_FirstGearReturns1()
        {
            var rally = ConvertToRallyData(new ACRallyDataConverter(), CreateTelemetry(gear: 2));

            Assert.AreEqual("1", rally.Gear);
        }

        [TestMethod]
        public void Convert_SixthGearReturns6()
        {
            var rally = ConvertToRallyData(new ACRallyDataConverter(), CreateTelemetry(gear: 7));

            Assert.AreEqual("6", rally.Gear);
        }

        #endregion

        #region Stage progress

        [TestMethod]
        public void Convert_DerivesProgressFromSplinePosition()
        {
            // A stage is a single lap, so progress comes from the spline position.
            var rally = ConvertToRallyData(new ACRallyDataConverter(), CreateTelemetry(normalizedCarPosition: 0.65f));

            Assert.AreEqual(65, rally.CompletedPct);
        }

        [TestMethod]
        public void Convert_ClampsProgressToTheStage()
        {
            var rally = ConvertToRallyData(new ACRallyDataConverter(), CreateTelemetry(normalizedCarPosition: 1.4f));

            Assert.AreEqual(100, rally.CompletedPct);
        }

        [TestMethod]
        public void Convert_UsesReportedDistanceTravelled()
        {
            var rally = ConvertToRallyData(
                new ACRallyDataConverter(),
                CreateTelemetry(distanceTraveled: 1234.6f, normalizedCarPosition: 0.5f, trackSplineLength: 10000));

            Assert.AreEqual(1235, rally.DistanceTravelled);
        }

        [TestMethod]
        public void Convert_FallsBackToSplineLengthForDistance()
        {
            var rally = ConvertToRallyData(
                new ACRallyDataConverter(),
                CreateTelemetry(distanceTraveled: 0, normalizedCarPosition: 0.25f, trackSplineLength: 10000));

            Assert.AreEqual(2500, rally.DistanceTravelled);
        }

        [TestMethod]
        public void Convert_ReportsFirstPositionWhenTheGameHasNone()
        {
            var rally = ConvertToRallyData(new ACRallyDataConverter(), CreateTelemetry(position: 0));

            Assert.AreEqual(1, rally.Position);
        }

        #endregion

        #region Stage and sector times

        [TestMethod]
        public void Convert_ConvertsStageTimeToSeconds()
        {
            var rally = ConvertToRallyData(new ACRallyDataConverter(), CreateTelemetry(currentTime: 120000));

            Assert.AreEqual(120f, rally.LapTime);
        }

        [TestMethod]
        public void Convert_SectorTimesAreZeroBeforeTheFirstSectorIsCrossed()
        {
            var rally = ConvertToRallyData(
                new ACRallyDataConverter(),
                CreateTelemetry(currentTime: 30000, currentSectorIndex: 0));

            Assert.AreEqual(0f, rally.Sector1Time);
            Assert.AreEqual(0f, rally.Sector2Time);
        }

        [TestMethod]
        public void Convert_RecordsElapsedTimeWhenCrossingIntoEachSector()
        {
            var converter = new ACRallyDataConverter();

            ConvertToRallyData(converter, CreateTelemetry(currentTime: 30000, currentSectorIndex: 0));
            var afterSector1 = ConvertToRallyData(converter, CreateTelemetry(currentTime: 45000, currentSectorIndex: 1));
            var afterSector2 = ConvertToRallyData(converter, CreateTelemetry(currentTime: 95000, currentSectorIndex: 2));

            Assert.AreEqual(45f, afterSector1.Sector1Time);
            Assert.AreEqual(45f, afterSector2.Sector1Time);
            Assert.AreEqual(95f, afterSector2.Sector2Time);
        }

        [TestMethod]
        public void Convert_KeepsSectorTimesWhileTheStageContinues()
        {
            var converter = new ACRallyDataConverter();

            ConvertToRallyData(converter, CreateTelemetry(currentTime: 45000, currentSectorIndex: 1));
            var later = ConvertToRallyData(converter, CreateTelemetry(currentTime: 60000, currentSectorIndex: 1));

            Assert.AreEqual(45f, later.Sector1Time);
        }

        [TestMethod]
        public void Convert_ClearsSectorTimesWhenANewStageStarts()
        {
            var converter = new ACRallyDataConverter();

            ConvertToRallyData(converter, CreateTelemetry(currentTime: 45000, currentSectorIndex: 1));
            ConvertToRallyData(converter, CreateTelemetry(currentTime: 95000, currentSectorIndex: 2));

            // Restarting rewinds both the stage clock and the sector index.
            var restarted = ConvertToRallyData(converter, CreateTelemetry(currentTime: 500, currentSectorIndex: 0));

            Assert.AreEqual(0f, restarted.Sector1Time);
            Assert.AreEqual(0f, restarted.Sector2Time);
        }

        #endregion

        #region Display type

        [TestMethod]
        public void Convert_ReturnsRallyDashboardType()
        {
            var update = new ACRallyDataConverter().Convert(CreateTelemetry());

            Assert.AreEqual(DisplayType.RallyDashboard, update.Type);
        }

        #endregion
    }
}
