using System.Runtime.InteropServices;

namespace HaddySimHub.Displays.Msfs;

/// <summary>
/// The raw block SimConnect sends back for our data definition.
/// </summary>
/// <remarks>
/// <para>
/// <b>This layout is a contract with <see cref="SimVarDefinitions"/>.</b> The sim
/// writes the variables in exactly the order they were added to the definition, with
/// no padding, so field order and type here must match that list one for one. Get it
/// wrong and every field past the mistake reads a plausible but wrong value, with no
/// error from the sim -- which is why <c>SimVarDefinitionsTests</c> pins the total
/// size of the definition against <c>Marshal.SizeOf</c> of this struct.
/// </para>
/// <para>
/// Everything numeric is requested as <c>FLOAT64</c>, including the flags and enums,
/// because that is what keeps the block uniformly eight-byte aligned. The three
/// strings sit at the end for the same reason.
/// </para>
/// </remarks>
[StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
public struct MsfsTelemetry
{
    // --- Speed ---
    public double IndicatedAirspeed;
    public double TrueAirspeed;
    public double GroundSpeed;
    public double Mach;
    public double StallWarning;
    public double OverspeedWarning;

    // --- Attitude ---

    /// <summary>Radians, and positive nose <i>down</i> -- see the converter.</summary>
    public double PitchRadians;

    /// <summary>Radians, and positive banking <i>left</i> -- see the converter.</summary>
    public double BankRadians;

    /// <summary>Slip/skid ball, -127 (full left) to 127 (full right).</summary>
    public double TurnCoordinatorBall;

    public double TurnRateRadiansPerSecond;

    // --- Altitude ---
    public double IndicatedAltitude;
    public double AltitudeAboveGround;
    public double VerticalSpeed;
    public double AltimeterSettingMb;
    public double OnGround;

    // --- Heading ---
    public double HeadingMagnetic;
    public double HeadingTrue;
    public double GroundTrack;
    public double WindDirection;
    public double WindVelocity;

    // --- Autopilot ---
    public double AutopilotMaster;
    public double AutopilotHeadingLock;
    public double AutopilotHeadingBug;
    public double AutopilotAltitudeLock;
    public double AutopilotAltitudeTarget;
    public double AutopilotAirspeedHold;
    public double AutopilotAirspeedTarget;
    public double AutopilotVerticalSpeedTarget;
    public double AutopilotNavLock;
    public double AutopilotApproachHold;

    // --- Navigation ---
    public double FlightPlanActive;
    public double WaypointDistanceMeters;
    public double WaypointEteSeconds;

    /// <summary>Time en route to the end of the flight plan, in seconds.</summary>
    public double DestinationEteSeconds;

    /// <summary>Arrival time at the end of the flight plan, as seconds of the UTC day.</summary>
    public double DestinationEtaSeconds;

    public double CrossTrackMeters;

    /// <summary>Set once an approach is being flown, which tightens the GPS course scale.</summary>
    public double ApproachActive;

    // --- Navigation radio ---
    public double NavHasSignal;
    public double NavHasLocalizer;
    public double NavHasGlideSlope;

    /// <summary>Lateral deviation, -127 to 127 at full scale.</summary>
    public double NavCdi;

    /// <summary>Glideslope deviation, -119 to 119 at full scale.</summary>
    public double NavGsi;

    /// <summary>0 = off, 1 = to the station, 2 = from it.</summary>
    public double NavToFrom;

    /// <summary>The course selected on the radio, in degrees.</summary>
    public double NavObs;

    // --- Engine ---
    public double EngineCount;

    /// <summary>Maps onto <see cref="Models.EngineType"/>, whose members mirror these values.</summary>
    public double EngineType;

    public double TurbineN1Pct;
    public double PistonPctMaxRpm;
    public double EngineRpm;
    public double FuelFlowPph;
    public double OilTemperature;
    public double OilPressure;
    public double ManifoldPressure;

    // --- Fuel ---
    public double FuelQuantityLbs;

    /// <summary>
    /// Capacity comes in gallons; there is no total-capacity-by-weight variable, so
    /// the converter multiplies by <see cref="FuelWeightPerGallon"/>.
    /// </summary>
    public double FuelCapacityGallons;

    public double FuelWeightPerGallon;

    // --- Configuration ---
    public double FlapsHandleIndex;
    public double FlapsHandlePositions;
    public double GearPercentExtended;
    public double GearHandleDown;
    public double SpoilersHandlePct;
    public double SpoilersArmed;
    public double ParkingBrakeOn;
    public double ElevatorTrimPct;

    // --- Lights ---
    public double LightLanding;
    public double LightTaxi;
    public double LightStrobe;
    public double LightNav;
    public double LightBeacon;

    // --- Miscellaneous ---
    public double ZuluTimeSeconds;

    // --- Strings, last so the numeric block stays eight-byte aligned ---

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string NextWaypointId;

    /// <summary>
    /// Populated once a destination or approach is loaded; empty in free flight.
    /// </summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string DestinationId;

    /// <summary>Identifier of the tuned navaid; empty when nothing is received.</summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string NavIdent;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
    public string AircraftTitle;
}
