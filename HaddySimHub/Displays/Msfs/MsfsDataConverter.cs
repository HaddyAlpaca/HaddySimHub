using HaddySimHub.Interfaces;
using HaddySimHub.Models;

namespace HaddySimHub.Displays.Msfs;

/// <summary>
/// Converts Microsoft Flight Simulator telemetry to the flight dashboard model.
/// </summary>
/// <remarks>
/// This is where the sim's conventions are translated into the dashboard's: radians
/// to degrees, metres to nautical miles, gallons to pounds, and the sim's sign
/// conventions for pitch and bank to the ones a display expects.
/// </remarks>
public class MsfsDataConverter : IDataConverter<MsfsTelemetry, DisplayUpdate>
{
    private const double MetresPerNauticalMile = 1852d;
    private const int SecondsPerDay = 86400;

    public DisplayUpdate Convert(MsfsTelemetry source)
    {
        var engineType = ToEngineType(source.EngineType);
        var engineCount = (int)source.EngineCount;
        var hasEngine = engineType != EngineType.None && engineType != EngineType.Unsupported && engineCount > 0;
        var isPiston = engineType == EngineType.Piston;
        var hasFlightPlan = IsSet(source.FlightPlanActive);

        var flightData = new FlightData
        {
            // Speed
            IndicatedAirspeed = (float)source.IndicatedAirspeed,
            TrueAirspeed = (float)source.TrueAirspeed,
            GroundSpeed = (float)source.GroundSpeed,
            MachNumber = (float)source.Mach,
            StallWarning = IsSet(source.StallWarning),
            OverspeedWarning = IsSet(source.OverspeedWarning),

            // Attitude. The sim reports both angles in radians despite the "DEGREES"
            // in their names, and signs them the opposite way round from how an
            // instrument reads: positive is nose down and wing down on the left.
            PitchDegrees = (float)ToDegrees(-source.PitchRadians),
            BankDegrees = (float)ToDegrees(-source.BankRadians),
            SlipBall = (float)Math.Clamp(source.TurnCoordinatorBall / 127d, -1d, 1d),
            TurnRate = (float)ToDegrees(source.TurnRateRadiansPerSecond),

            // Altitude
            IndicatedAltitude = (float)source.IndicatedAltitude,
            AltitudeAboveGround = (float)source.AltitudeAboveGround,
            VerticalSpeed = (float)source.VerticalSpeed,
            AltimeterSettingHpa = (float)source.AltimeterSettingMb,
            OnGround = IsSet(source.OnGround),

            // Heading
            HeadingMagnetic = (float)Normalize(source.HeadingMagnetic),
            HeadingTrue = (float)Normalize(source.HeadingTrue),
            GroundTrack = (float)Normalize(source.GroundTrack),
            WindDirection = (float)Normalize(source.WindDirection),
            WindSpeed = (float)source.WindVelocity,

            // Autopilot
            AutopilotMaster = IsSet(source.AutopilotMaster),
            HeadingHold = IsSet(source.AutopilotHeadingLock),
            HeadingBug = (float)Normalize(source.AutopilotHeadingBug),
            AltitudeHold = IsSet(source.AutopilotAltitudeLock),
            AltitudeTarget = (float)source.AutopilotAltitudeTarget,
            SpeedHold = IsSet(source.AutopilotAirspeedHold),
            SpeedTarget = (float)source.AutopilotAirspeedTarget,
            VerticalSpeedTarget = (float)source.AutopilotVerticalSpeedTarget,
            NavHold = IsSet(source.AutopilotNavLock),
            ApproachHold = IsSet(source.AutopilotApproachHold),

            // Navigation. Without a flight plan the sim leaves these at zero, which
            // would read as "next waypoint 0 NM away" rather than "nothing planned".
            HasActiveFlightPlan = hasFlightPlan,
            NextWaypointId = hasFlightPlan ? Trimmed(source.NextWaypointId) : null,
            DistanceToWaypointNm = hasFlightPlan ? (float)ToNauticalMiles(source.WaypointDistanceMeters) : null,
            WaypointEteSeconds = hasFlightPlan ? ToSeconds(source.WaypointEteSeconds) : null,
            DestinationId = hasFlightPlan ? Trimmed(source.DestinationId) : null,
            DistanceToDestinationNm = hasFlightPlan ? DestinationDistanceNm(source) : null,
            DestinationEteSeconds = hasFlightPlan ? ToSeconds(source.DestinationEteSeconds) : null,
            DestinationEtaUtcSeconds = hasFlightPlan ? ToTimeOfDay(source.DestinationEtaSeconds) : null,
            CrossTrackErrorNm = hasFlightPlan ? (float)ToNauticalMiles(source.CrossTrackMeters) : null,

            // Engine
            EngineCount = engineCount,
            EngineType = engineType,
            EnginePrimaryPct = hasEngine ? (float)(isPiston ? source.PistonPctMaxRpm : source.TurbineN1Pct) : null,

            // A jet has no propeller or crankshaft speed a pilot reads; everything else does.
            EngineRpm = hasEngine && engineType != EngineType.Jet ? (int)Math.Round(source.EngineRpm) : null,
            FuelFlowPph = hasEngine ? (float)source.FuelFlowPph : null,
            OilTemperature = hasEngine ? (float)source.OilTemperature : null,
            OilPressure = hasEngine ? (float)source.OilPressure : null,
            ManifoldPressure = isPiston ? (float)source.ManifoldPressure : null,

            // Fuel
            FuelQuantityLbs = (float)source.FuelQuantityLbs,
            FuelCapacityLbs = (float)(source.FuelCapacityGallons * source.FuelWeightPerGallon),
            FuelEnduranceSeconds = EnduranceSeconds(source),

            // Configuration
            FlapsHandleIndex = (int)source.FlapsHandleIndex,
            FlapsHandlePositions = (int)source.FlapsHandlePositions,
            GearPercentExtended = (float)Math.Clamp(source.GearPercentExtended, 0d, 100d),
            GearHandleDown = IsSet(source.GearHandleDown),
            SpoilersPct = (float)Math.Clamp(source.SpoilersHandlePct, 0d, 100d),
            SpoilersArmed = IsSet(source.SpoilersArmed),
            ParkingBrakeOn = IsSet(source.ParkingBrakeOn),
            ElevatorTrimPct = (float)source.ElevatorTrimPct,

            // Lights
            LandingLightsOn = IsSet(source.LightLanding),
            TaxiLightsOn = IsSet(source.LightTaxi),
            StrobeLightsOn = IsSet(source.LightStrobe),
            NavLightsOn = IsSet(source.LightNav),
            BeaconOn = IsSet(source.LightBeacon),

            // Miscellaneous
            AircraftTitle = Trimmed(source.AircraftTitle) ?? string.Empty,
            SimTimeUtcSeconds = ToTimeOfDay(source.ZuluTimeSeconds) ?? 0,
        };

        return new DisplayUpdate
        {
            Type = DisplayType.FlightDashboard,
            Data = flightData,
        };
    }

    /// <summary>
    /// Every flag arrives as a double, so anything off zero counts as set rather than
    /// comparing for exact equality with 1.
    /// </summary>
    private static bool IsSet(double value) => Math.Abs(value) > double.Epsilon;

    private static double ToDegrees(double radians) => radians * 180d / Math.PI;

    private static double ToNauticalMiles(double metres) => metres / MetresPerNauticalMile;

    /// <summary>Wraps a bearing into 0-360, since the sim can report slightly outside it.</summary>
    private static double Normalize(double degrees) => ((degrees % 360d) + 360d) % 360d;

    private static int? ToSeconds(double value) => value > 0 ? (int)Math.Round(value) : null;

    /// <summary>Folds an absolute time into a time of day, which is how the dashboard shows it.</summary>
    private static int? ToTimeOfDay(double seconds) =>
        seconds > 0 ? (int)Math.Round(seconds) % SecondsPerDay : null;

    /// <summary>
    /// The SDK has no total-distance-to-destination variable, so it is derived from
    /// the time en route the sim does report and the current ground speed -- the same
    /// arithmetic the sim used to produce that estimate.
    /// </summary>
    private static float? DestinationDistanceNm(MsfsTelemetry source)
    {
        if (source.DestinationEteSeconds <= 0 || source.GroundSpeed <= 0)
        {
            return null;
        }

        return (float)(source.GroundSpeed * source.DestinationEteSeconds / 3600d);
    }

    private static int? EnduranceSeconds(MsfsTelemetry source)
    {
        if (source.FuelFlowPph <= 0)
        {
            return null;
        }

        return (int)Math.Round(source.FuelQuantityLbs / source.FuelFlowPph * 3600d);
    }

    private static string? Trimmed(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }

    /// <summary>
    /// <see cref="EngineType"/> mirrors the simvar's own numbering, so anything the
    /// sim reports outside that range maps to the SDK's own "unsupported".
    /// </summary>
    private static EngineType ToEngineType(double value)
    {
        var index = (int)Math.Round(value);
        return Enum.IsDefined(typeof(EngineType), index) ? (EngineType)index : EngineType.Unsupported;
    }
}
