using HaddySimHub.Displays.AC;
using HaddySimHub.Models;
using System.Runtime.InteropServices;

namespace HaddySimHub.Tests
{
    [TestClass]
    public class ACDataConverterTests
    {
        #region Shared memory layout

        [TestMethod]
        public void Physics_MatchesSharedMemoryPageSize()
        {
            // The page is read as a raw memory image, so a layout change that alters the
            // size means every field past the change is being read from the wrong offset.
            Assert.AreEqual(580, Marshal.SizeOf<ACPhysics>());
        }

        [TestMethod]
        public void Graphics_StopsBeforeTheVersionDependentPartOfThePage()
        {
            // Only the leading, stable part of the graphics page is mapped.
            Assert.AreEqual(252, Marshal.SizeOf<ACGraphics>());
        }

        [TestMethod]
        public void Static_StopsAfterTheFieldsTheDisplayNeeds()
        {
            Assert.AreEqual(420, Marshal.SizeOf<ACStatic>());
        }

        #endregion

        #region Display type

        [TestMethod]
        public void Convert_ReturnsRaceDashboard()
        {
            var update = new ACDataConverter().Convert(CreateTelemetry());

            Assert.AreEqual(DisplayType.RaceDashboard, update.Type);
        }

        #endregion

        #region Gear

        [TestMethod]
        public void Convert_GearReverse()
        {
            // Assetto Corsa encodes gears as 0 = reverse, 1 = neutral, 2 = first.
            Assert.AreEqual("R", Convert(CreateTelemetry(gear: 0)).Gear);
        }

        [TestMethod]
        public void Convert_GearNeutral()
        {
            Assert.AreEqual("N", Convert(CreateTelemetry(gear: 1)).Gear);
        }

        [TestMethod]
        public void Convert_GearFirst()
        {
            Assert.AreEqual("1", Convert(CreateTelemetry(gear: 2)).Gear);
        }

        [TestMethod]
        public void Convert_GearSixth()
        {
            Assert.AreEqual("6", Convert(CreateTelemetry(gear: 7)).Gear);
        }

        #endregion

        #region Speed and RPM

        [TestMethod]
        public void Convert_UsesSpeedInKmhDirectly()
        {
            // The physics page already reports km/h; no conversion should be applied.
            Assert.AreEqual(180, Convert(CreateTelemetry(speedKmh: 180)).Speed);
        }

        [TestMethod]
        public void Convert_ClampsNegativeSpeedToZero()
        {
            Assert.AreEqual(0, Convert(CreateTelemetry(speedKmh: -2)).Speed);
        }

        [TestMethod]
        public void Convert_UsesRawRpmAndStaticRevLimit()
        {
            var race = Convert(CreateTelemetry(rpms: 6500, maxRpm: 8000));

            Assert.AreEqual(6500, race.Rpm);
            Assert.AreEqual(8000, race.RpmMax);
        }

        #endregion

        #region Session

        [TestMethod]
        public void Convert_SessionTypePractice()
        {
            Assert.AreEqual("Practice", Convert(CreateTelemetry(sessionType: ACSessionType.Practice)).SessionType);
        }

        [TestMethod]
        public void Convert_SessionTypeQualifying()
        {
            Assert.AreEqual("Qualifying", Convert(CreateTelemetry(sessionType: ACSessionType.Qualifying)).SessionType);
        }

        [TestMethod]
        public void Convert_SessionTypeRace()
        {
            Assert.AreEqual("Race", Convert(CreateTelemetry(sessionType: ACSessionType.Race)).SessionType);
        }

        [TestMethod]
        public void Convert_CountsTheLapBeingDrivenRatherThanLapsFinished()
        {
            // The graphics page reports 0 completed laps while on the opening lap.
            Assert.AreEqual(1, Convert(CreateTelemetry(completedLaps: 0)).CurrentLap);
            Assert.AreEqual(4, Convert(CreateTelemetry(completedLaps: 3)).CurrentLap);
        }

        [TestMethod]
        public void Convert_TotalLaps()
        {
            var race = Convert(CreateTelemetry(numberOfLaps: 12));

            Assert.AreEqual(12, race.TotalLaps);
            Assert.IsTrue(race.IsLimitedSessionLaps);
        }

        [TestMethod]
        public void Convert_ReportsRacePosition()
        {
            Assert.AreEqual(4, Convert(CreateTelemetry(position: 4)).Position);
        }

        [TestMethod]
        public void Convert_PositionIsNullBeforeTheSessionReportsOne()
        {
            Assert.IsNull(Convert(CreateTelemetry(position: 0)).Position);
        }

        #endregion

        #region Lap times

        [TestMethod]
        public void Convert_ConvertsLapTimesToSeconds()
        {
            var race = Convert(CreateTelemetry(currentTime: 45500, lastTime: 92000, bestTime: 90250));

            Assert.AreEqual(45.5f, race.CurrentLapTime);
            Assert.AreEqual(92f, race.LastLapTime);
            Assert.AreEqual(90.25f, race.BestLapTime);
        }

        [TestMethod]
        public void Convert_BestLapIsNullUntilOneIsSet()
        {
            // The game reports an unset best lap as int.MaxValue, not as zero.
            Assert.IsNull(Convert(CreateTelemetry(bestTime: int.MaxValue)).BestLapTime);
            Assert.IsNull(Convert(CreateTelemetry(bestTime: 0)).BestLapTime);
        }

        #endregion

        #region Inputs, fuel and temperatures

        [TestMethod]
        public void Convert_MapsPedalAndPitLimiterInputs()
        {
            var race = Convert(CreateTelemetry(
                gas: 0.75f, brake: 0.25f, clutch: 0.5f, steerAngle: -0.4f, pitLimiterOn: 1));

            Assert.AreEqual(75, race.ThrottlePct);
            Assert.AreEqual(25, race.BrakePct);
            Assert.AreEqual(50, race.ClutchPct);
            Assert.AreEqual(40, race.SteeringPct);
            Assert.IsTrue(race.PitLimiterOn);
        }

        [TestMethod]
        public void Convert_ReportsFuelRemaining()
        {
            Assert.AreEqual(34.5f, Convert(CreateTelemetry(fuel: 34.5f)).FuelRemaining);
        }

        [TestMethod]
        public void Convert_UnavailableFieldsAreNull()
        {
            var race = Convert(CreateTelemetry());

            Assert.IsNull(race.FuelAvgLap);
            Assert.IsNull(race.FuelLastLap);
            Assert.IsNull(race.LastLapTimeDelta);
            Assert.IsNull(race.BestLapTimeDelta);
        }

        [TestMethod]
        public void Convert_Temperature()
        {
            var race = Convert(CreateTelemetry(airTemp: 22.5f, roadTemp: 31.5f));

            Assert.AreEqual(22.5f, race.AirTemp);
            Assert.AreEqual(31.5f, race.TrackTemp);
        }

        #endregion

        #region Helpers

        private static RaceData Convert(ACTelemetry telemetry)
        {
            var update = new ACDataConverter().Convert(telemetry);
            var race = update.Data as RaceData;
            Assert.IsNotNull(race);
            return race;
        }

        private static ACTelemetry CreateTelemetry(
            float speedKmh = 0,
            int rpms = 0,
            int maxRpm = 8000,
            int gear = 1,
            ACSessionType sessionType = ACSessionType.Practice,
            int completedLaps = 0,
            int numberOfLaps = 0,
            int position = 0,
            float airTemp = 20,
            float roadTemp = 30,
            float sessionTimeLeft = 0,
            int currentTime = 0,
            int lastTime = 0,
            int bestTime = 0,
            float fuel = 0,
            float gas = 0,
            float brake = 0,
            float clutch = 0,
            float steerAngle = 0,
            int pitLimiterOn = 0)
        {
            return new ACTelemetry
            {
                SpeedKmh = speedKmh,
                Rpms = rpms,
                MaxRpm = maxRpm,
                Gear = gear,
                SessionType = sessionType,
                CompletedLaps = completedLaps,
                NumberOfLaps = numberOfLaps,
                Position = position,
                AirTemp = airTemp,
                RoadTemp = roadTemp,
                SessionTimeLeft = sessionTimeLeft,
                CurrentTime = currentTime,
                LastTime = lastTime,
                BestTime = bestTime,
                Fuel = fuel,
                Gas = gas,
                Brake = brake,
                Clutch = clutch,
                SteerAngle = steerAngle,
                PitLimiterOn = pitLimiterOn,
                CarModel = string.Empty,
                Track = string.Empty,
                SmVersion = string.Empty,
                AcVersion = string.Empty,
            };
        }

        #endregion
    }
}
