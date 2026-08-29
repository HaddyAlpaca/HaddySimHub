using HaddySimHub.Displays.Msfs;
using HaddySimHub.Models;

namespace HaddySimHub.Tests;

[TestClass]
public class MsfsDataConverterTests
{
    private readonly MsfsDataConverter _converter = new();

    [TestMethod]
    public void Convert_ReturnsFlightDashboard()
    {
        var update = _converter.Convert(CreateTelemetry());

        Assert.AreEqual(DisplayType.FlightDashboard, update.Type);
        Assert.IsInstanceOfType<FlightData>(update.Data);
    }

    #region Speed

    [TestMethod]
    public void Convert_MapsSpeeds()
    {
        var telemetry = CreateTelemetry();
        telemetry.IndicatedAirspeed = 268;
        telemetry.TrueAirspeed = 318;
        telemetry.GroundSpeed = 312;
        telemetry.Mach = 0.62;

        var data = ConvertData(telemetry);

        Assert.AreEqual(268f, data.IndicatedAirspeed);
        Assert.AreEqual(318f, data.TrueAirspeed);
        Assert.AreEqual(312f, data.GroundSpeed);
        Assert.AreEqual(0.62f, data.MachNumber, 0.0001f);
    }

    [TestMethod]
    public void Convert_ReadsFlagsThatArriveAsDoubles()
    {
        var telemetry = CreateTelemetry();
        telemetry.StallWarning = 1;
        telemetry.OverspeedWarning = 0;

        var data = ConvertData(telemetry);

        Assert.IsTrue(data.StallWarning);
        Assert.IsFalse(data.OverspeedWarning);
    }

    #endregion

    #region Attitude

    [TestMethod]
    public void Convert_TurnsRadiansIntoDegreesAndFlipsThePitchSign()
    {
        // Despite the name, PLANE PITCH DEGREES is in radians, and the sim reports a
        // nose-up attitude as a negative value.
        var telemetry = CreateTelemetry();
        telemetry.PitchRadians = -Math.PI / 18;

        Assert.AreEqual(10f, ConvertData(telemetry).PitchDegrees, 0.001f);
    }

    [TestMethod]
    public void Convert_FlipsTheBankSignSoRightIsPositive()
    {
        // The sim reports a right bank as a negative value.
        var telemetry = CreateTelemetry();
        telemetry.BankRadians = -Math.PI / 6;

        Assert.AreEqual(30f, ConvertData(telemetry).BankDegrees, 0.001f);
    }

    [TestMethod]
    public void Convert_NormalisesTheSlipBallToPlusMinusOne()
    {
        var telemetry = CreateTelemetry();

        telemetry.TurnCoordinatorBall = 127;
        Assert.AreEqual(1f, ConvertData(telemetry).SlipBall, 0.001f);

        telemetry.TurnCoordinatorBall = -127;
        Assert.AreEqual(-1f, ConvertData(telemetry).SlipBall, 0.001f);

        telemetry.TurnCoordinatorBall = 0;
        Assert.AreEqual(0f, ConvertData(telemetry).SlipBall, 0.001f);
    }

    [TestMethod]
    public void Convert_ClampsASlipBallBeyondFullDeflection()
    {
        var telemetry = CreateTelemetry();
        telemetry.TurnCoordinatorBall = 400;

        Assert.AreEqual(1f, ConvertData(telemetry).SlipBall, 0.001f);
    }

    [TestMethod]
    public void Convert_TurnsTheTurnRateIntoDegreesPerSecond()
    {
        var telemetry = CreateTelemetry();
        telemetry.TurnRateRadiansPerSecond = Math.PI / 60;

        Assert.AreEqual(3f, ConvertData(telemetry).TurnRate, 0.001f);
    }

    #endregion

    #region Altitude and heading

    [TestMethod]
    public void Convert_MapsAltitudes()
    {
        var telemetry = CreateTelemetry();
        telemetry.IndicatedAltitude = 24000;
        telemetry.AltitudeAboveGround = 23860;
        telemetry.VerticalSpeed = 720;
        telemetry.AltimeterSettingMb = 1013;
        telemetry.OnGround = 0;

        var data = ConvertData(telemetry);

        Assert.AreEqual(24000f, data.IndicatedAltitude);
        Assert.AreEqual(23860f, data.AltitudeAboveGround);
        Assert.AreEqual(720f, data.VerticalSpeed);
        Assert.AreEqual(1013f, data.AltimeterSettingHpa);
        Assert.IsFalse(data.OnGround);
    }

    [TestMethod]
    public void Convert_WrapsBearingsIntoZeroToThreeSixty()
    {
        var telemetry = CreateTelemetry();
        telemetry.HeadingMagnetic = 365;
        telemetry.HeadingTrue = -5;
        telemetry.GroundTrack = 720;
        telemetry.WindDirection = -95;

        var data = ConvertData(telemetry);

        Assert.AreEqual(5f, data.HeadingMagnetic, 0.001f);
        Assert.AreEqual(355f, data.HeadingTrue, 0.001f);
        Assert.AreEqual(0f, data.GroundTrack, 0.001f);
        Assert.AreEqual(265f, data.WindDirection, 0.001f);
    }

    #endregion

    #region Navigation

    [TestMethod]
    public void Convert_TurnsWaypointMetresIntoNauticalMiles()
    {
        var telemetry = CreateTelemetry();
        telemetry.FlightPlanActive = 1;
        telemetry.WaypointDistanceMeters = 18520;

        Assert.AreEqual(10f, ConvertData(telemetry).DistanceToWaypointNm!.Value, 0.001f);
    }

    [TestMethod]
    public void Convert_TurnsCrossTrackMetresIntoNauticalMilesKeepingTheSign()
    {
        var telemetry = CreateTelemetry();
        telemetry.FlightPlanActive = 1;
        telemetry.CrossTrackMeters = -926;

        Assert.AreEqual(-0.5f, ConvertData(telemetry).CrossTrackErrorNm!.Value, 0.001f);
    }

    [TestMethod]
    public void Convert_DerivesTheDistanceToDestinationFromGroundSpeedAndTimeEnRoute()
    {
        // The SDK exposes no total-distance variable, so it comes back out of the
        // sim's own time estimate.
        var telemetry = CreateTelemetry();
        telemetry.FlightPlanActive = 1;
        telemetry.GroundSpeed = 300;
        telemetry.DestinationEteSeconds = 1800;

        Assert.AreEqual(150f, ConvertData(telemetry).DistanceToDestinationNm!.Value, 0.001f);
    }

    [TestMethod]
    public void Convert_LeavesTheDistanceToDestinationUnknownWhenStopped()
    {
        var telemetry = CreateTelemetry();
        telemetry.FlightPlanActive = 1;
        telemetry.GroundSpeed = 0;
        telemetry.DestinationEteSeconds = 1800;

        Assert.IsNull(ConvertData(telemetry).DistanceToDestinationNm);
    }

    [TestMethod]
    public void Convert_FoldsTheArrivalTimeIntoATimeOfDay()
    {
        var telemetry = CreateTelemetry();
        telemetry.FlightPlanActive = 1;
        telemetry.DestinationEtaSeconds = (14 * 3600) + (12 * 60);

        Assert.AreEqual((14 * 3600) + (12 * 60), ConvertData(telemetry).DestinationEtaUtcSeconds);
    }

    [TestMethod]
    public void Convert_ReportsNoNavigationDataWithoutAFlightPlan()
    {
        // Without a plan the sim leaves these at zero, which must not read as "the
        // next waypoint is right here".
        var telemetry = CreateTelemetry();
        telemetry.FlightPlanActive = 0;
        telemetry.WaypointDistanceMeters = 0;
        telemetry.NextWaypointId = "ARTIP";
        telemetry.DestinationId = "EHAM";

        var data = ConvertData(telemetry);

        Assert.IsFalse(data.HasActiveFlightPlan);
        Assert.IsNull(data.NextWaypointId);
        Assert.IsNull(data.DistanceToWaypointNm);
        Assert.IsNull(data.WaypointEteSeconds);
        Assert.IsNull(data.DestinationId);
        Assert.IsNull(data.DistanceToDestinationNm);
        Assert.IsNull(data.DestinationEteSeconds);
        Assert.IsNull(data.DestinationEtaUtcSeconds);
        Assert.IsNull(data.CrossTrackErrorNm);
    }

    [TestMethod]
    public void Convert_TreatsAnEmptyDestinationAsUnknown()
    {
        // The destination identifier only appears once a destination or approach is loaded.
        var telemetry = CreateTelemetry();
        telemetry.FlightPlanActive = 1;
        telemetry.NextWaypointId = "ARTIP";
        telemetry.DestinationId = "   ";

        var data = ConvertData(telemetry);

        Assert.AreEqual("ARTIP", data.NextWaypointId);
        Assert.IsNull(data.DestinationId);
    }

    #endregion

    #region Engine

    [TestMethod]
    public void Convert_ReadsATurbineOnN1()
    {
        var telemetry = CreateTelemetry();
        telemetry.EngineType = (double)EngineType.Jet;
        telemetry.EngineCount = 2;
        telemetry.TurbineN1Pct = 78;
        telemetry.PistonPctMaxRpm = 12;

        var data = ConvertData(telemetry);

        Assert.AreEqual(EngineType.Jet, data.EngineType);
        Assert.AreEqual(78f, data.EnginePrimaryPct!.Value, 0.001f);
    }

    [TestMethod]
    public void Convert_ReadsAPistonOnPercentageOfMaximumRpm()
    {
        var telemetry = CreateTelemetry();
        telemetry.EngineType = (double)EngineType.Piston;
        telemetry.EngineCount = 1;
        telemetry.TurbineN1Pct = 0;
        telemetry.PistonPctMaxRpm = 65;
        telemetry.EngineRpm = 2400;
        telemetry.ManifoldPressure = 24.5;

        var data = ConvertData(telemetry);

        Assert.AreEqual(EngineType.Piston, data.EngineType);
        Assert.AreEqual(65f, data.EnginePrimaryPct!.Value, 0.001f);
        Assert.AreEqual(2400, data.EngineRpm);
        Assert.AreEqual(24.5f, data.ManifoldPressure!.Value, 0.001f);
    }

    [TestMethod]
    public void Convert_ReportsNoRpmOrManifoldPressureForAJet()
    {
        var telemetry = CreateTelemetry();
        telemetry.EngineType = (double)EngineType.Jet;
        telemetry.EngineCount = 2;
        telemetry.EngineRpm = 8000;
        telemetry.ManifoldPressure = 30;

        var data = ConvertData(telemetry);

        Assert.IsNull(data.EngineRpm);
        Assert.IsNull(data.ManifoldPressure);
    }

    [TestMethod]
    public void Convert_ReportsPropellerSpeedForATurboprop()
    {
        var telemetry = CreateTelemetry();
        telemetry.EngineType = (double)EngineType.Turboprop;
        telemetry.EngineCount = 1;
        telemetry.EngineRpm = 1900;

        var data = ConvertData(telemetry);

        Assert.AreEqual(1900, data.EngineRpm);
        Assert.IsNull(data.ManifoldPressure);
    }

    [TestMethod]
    public void Convert_ReportsNoEngineReadoutsForAGlider()
    {
        var telemetry = CreateTelemetry();
        telemetry.EngineType = (double)EngineType.None;
        telemetry.EngineCount = 0;
        telemetry.TurbineN1Pct = 55;
        telemetry.FuelFlowPph = 100;
        telemetry.OilTemperature = 80;
        telemetry.OilPressure = 40;

        var data = ConvertData(telemetry);

        Assert.AreEqual(EngineType.None, data.EngineType);
        Assert.IsNull(data.EnginePrimaryPct);
        Assert.IsNull(data.EngineRpm);
        Assert.IsNull(data.FuelFlowPph);
        Assert.IsNull(data.OilTemperature);
        Assert.IsNull(data.OilPressure);
    }

    [TestMethod]
    public void Convert_MapsAnUnknownEngineTypeToUnsupported()
    {
        var telemetry = CreateTelemetry();
        telemetry.EngineType = 99;

        Assert.AreEqual(EngineType.Unsupported, ConvertData(telemetry).EngineType);
    }

    #endregion

    #region Fuel

    [TestMethod]
    public void Convert_TurnsTheGallonCapacityIntoPounds()
    {
        var telemetry = CreateTelemetry();
        telemetry.FuelCapacityGallons = 200;
        telemetry.FuelWeightPerGallon = 6.7;

        Assert.AreEqual(1340f, ConvertData(telemetry).FuelCapacityLbs, 0.01f);
    }

    [TestMethod]
    public void Convert_DerivesEnduranceFromQuantityAndFlow()
    {
        var telemetry = CreateTelemetry();
        telemetry.FuelQuantityLbs = 1280;
        telemetry.FuelFlowPph = 640;

        Assert.AreEqual(2 * 3600, ConvertData(telemetry).FuelEnduranceSeconds);
    }

    [TestMethod]
    public void Convert_ReportsNoEnduranceWhenNothingIsBurningFuel()
    {
        // Dividing by a zero flow would otherwise produce infinity.
        var telemetry = CreateTelemetry();
        telemetry.FuelQuantityLbs = 1280;
        telemetry.FuelFlowPph = 0;

        Assert.IsNull(ConvertData(telemetry).FuelEnduranceSeconds);
    }

    #endregion

    #region Configuration

    [TestMethod]
    public void Convert_MapsTheAirframeConfiguration()
    {
        var telemetry = CreateTelemetry();
        telemetry.FlapsHandleIndex = 2;
        telemetry.FlapsHandlePositions = 5;
        telemetry.GearPercentExtended = 100;
        telemetry.GearHandleDown = 1;
        telemetry.SpoilersHandlePct = 40;
        telemetry.SpoilersArmed = 1;
        telemetry.ParkingBrakeOn = 1;
        telemetry.ElevatorTrimPct = -12;

        var data = ConvertData(telemetry);

        Assert.AreEqual(2, data.FlapsHandleIndex);
        Assert.AreEqual(5, data.FlapsHandlePositions);
        Assert.AreEqual(100f, data.GearPercentExtended);
        Assert.IsTrue(data.GearHandleDown);
        Assert.AreEqual(40f, data.SpoilersPct);
        Assert.IsTrue(data.SpoilersArmed);
        Assert.IsTrue(data.ParkingBrakeOn);
        Assert.AreEqual(-12f, data.ElevatorTrimPct);
    }

    [TestMethod]
    public void Convert_ClampsGearAndSpoilersToAPercentage()
    {
        var telemetry = CreateTelemetry();
        telemetry.GearPercentExtended = 140;
        telemetry.SpoilersHandlePct = -8;

        var data = ConvertData(telemetry);

        Assert.AreEqual(100f, data.GearPercentExtended);
        Assert.AreEqual(0f, data.SpoilersPct);
    }

    [TestMethod]
    public void Convert_MapsTheLights()
    {
        var telemetry = CreateTelemetry();
        telemetry.LightLanding = 1;
        telemetry.LightTaxi = 0;
        telemetry.LightStrobe = 1;
        telemetry.LightNav = 1;
        telemetry.LightBeacon = 0;

        var data = ConvertData(telemetry);

        Assert.IsTrue(data.LandingLightsOn);
        Assert.IsFalse(data.TaxiLightsOn);
        Assert.IsTrue(data.StrobeLightsOn);
        Assert.IsTrue(data.NavLightsOn);
        Assert.IsFalse(data.BeaconOn);
    }

    #endregion

    #region Autopilot and miscellaneous

    [TestMethod]
    public void Convert_MapsTheAutopilotModesAndTargets()
    {
        var telemetry = CreateTelemetry();
        telemetry.AutopilotMaster = 1;
        telemetry.AutopilotHeadingLock = 1;
        telemetry.AutopilotHeadingBug = 106;
        telemetry.AutopilotAltitudeLock = 1;
        telemetry.AutopilotAltitudeTarget = 24000;
        telemetry.AutopilotAirspeedHold = 0;
        telemetry.AutopilotAirspeedTarget = 270;
        telemetry.AutopilotVerticalSpeedTarget = 1200;
        telemetry.AutopilotNavLock = 1;
        telemetry.AutopilotApproachHold = 0;

        var data = ConvertData(telemetry);

        Assert.IsTrue(data.AutopilotMaster);
        Assert.IsTrue(data.HeadingHold);
        Assert.AreEqual(106f, data.HeadingBug, 0.001f);
        Assert.IsTrue(data.AltitudeHold);
        Assert.AreEqual(24000f, data.AltitudeTarget);
        Assert.IsFalse(data.SpeedHold);
        Assert.AreEqual(270f, data.SpeedTarget);
        Assert.AreEqual(1200f, data.VerticalSpeedTarget);
        Assert.IsTrue(data.NavHold);
        Assert.IsFalse(data.ApproachHold);
    }

    [TestMethod]
    public void Convert_TrimsTheAircraftTitle()
    {
        var telemetry = CreateTelemetry();
        telemetry.AircraftTitle = "  Cessna Citation Longitude  ";

        Assert.AreEqual("Cessna Citation Longitude", ConvertData(telemetry).AircraftTitle);
    }

    [TestMethod]
    public void Convert_NeverLeavesTheAircraftTitleNull()
    {
        Assert.AreEqual(string.Empty, ConvertData(CreateTelemetry()).AircraftTitle);
    }

    [TestMethod]
    public void Convert_FoldsTheSimulatorClockIntoATimeOfDay()
    {
        var telemetry = CreateTelemetry();
        telemetry.ZuluTimeSeconds = 86400 + (9 * 3600);

        Assert.AreEqual(9 * 3600, ConvertData(telemetry).SimTimeUtcSeconds);
    }

    #endregion

    private FlightData ConvertData(MsfsTelemetry telemetry) => (FlightData)_converter.Convert(telemetry).Data!;

    /// <summary>
    /// A twin jet with everything at rest. Strings start empty because the sim always
    /// fills the fixed-width field rather than sending a null.
    /// </summary>
    private static MsfsTelemetry CreateTelemetry() => new()
    {
        EngineType = (double)HaddySimHub.Models.EngineType.Jet,
        EngineCount = 2,
        NextWaypointId = string.Empty,
        DestinationId = string.Empty,
        AircraftTitle = string.Empty,
    };
}
