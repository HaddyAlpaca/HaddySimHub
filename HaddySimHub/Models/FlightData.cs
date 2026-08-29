namespace HaddySimHub.Models;

/// <summary>
/// Telemetry for the flight dashboard.
/// </summary>
/// <remarks>
/// A property is nullable where the value depends on the aircraft or on the flight:
/// a glider has no engine readouts, a piston aircraft has no N1, and free flight has
/// no waypoints. The dashboard hides those readouts rather than showing a zero, so
/// the converter must leave them null instead of defaulting them.
/// </remarks>
public sealed record FlightData
{
    /* Speed */

    /// <summary>
    /// Indicated airspeed in knots.
    /// </summary>
    public float IndicatedAirspeed { get; init; }

    /// <summary>
    /// True airspeed in knots.
    /// </summary>
    public float TrueAirspeed { get; init; }

    /// <summary>
    /// Ground speed in knots.
    /// </summary>
    public float GroundSpeed { get; init; }

    /// <summary>
    /// Mach number. Only meaningful for high-altitude flight.
    /// </summary>
    public float MachNumber { get; init; }

    public bool StallWarning { get; init; }

    public bool OverspeedWarning { get; init; }

    /* Attitude */

    /// <summary>
    /// Pitch in degrees, positive nose up.
    /// </summary>
    public float PitchDegrees { get; init; }

    /// <summary>
    /// Bank in degrees, positive banking right.
    /// </summary>
    public float BankDegrees { get; init; }

    /// <summary>
    /// Slip/skid ball deflection, -1 (full left) to 1 (full right).
    /// </summary>
    public float SlipBall { get; init; }

    /// <summary>
    /// Rate of turn in degrees per second, positive turning right.
    /// </summary>
    public float TurnRate { get; init; }

    /* Altitude */

    /// <summary>
    /// Indicated altitude in feet, i.e. corrected for the altimeter setting.
    /// </summary>
    public float IndicatedAltitude { get; init; }

    /// <summary>
    /// Height above the terrain in feet.
    /// </summary>
    public float AltitudeAboveGround { get; init; }

    /// <summary>
    /// Vertical speed in feet per minute, positive climbing.
    /// </summary>
    public float VerticalSpeed { get; init; }

    /// <summary>
    /// Altimeter setting in hectopascal.
    /// </summary>
    public float AltimeterSettingHpa { get; init; }

    public bool OnGround { get; init; }

    /* Heading */

    /// <summary>
    /// Magnetic heading in degrees (0-360).
    /// </summary>
    public float HeadingMagnetic { get; init; }

    /// <summary>
    /// True heading in degrees (0-360).
    /// </summary>
    public float HeadingTrue { get; init; }

    /// <summary>
    /// Magnetic ground track in degrees (0-360). Differs from the heading in a crosswind.
    /// </summary>
    public float GroundTrack { get; init; }

    /// <summary>
    /// Direction the wind is coming from, in degrees (0-360).
    /// </summary>
    public float WindDirection { get; init; }

    /// <summary>
    /// Wind speed in knots.
    /// </summary>
    public float WindSpeed { get; init; }

    /* Autopilot */

    public bool AutopilotMaster { get; init; }

    public bool HeadingHold { get; init; }

    /// <summary>
    /// Selected heading in degrees (0-360).
    /// </summary>
    public float HeadingBug { get; init; }

    public bool AltitudeHold { get; init; }

    /// <summary>
    /// Selected altitude in feet.
    /// </summary>
    public float AltitudeTarget { get; init; }

    public bool SpeedHold { get; init; }

    /// <summary>
    /// Selected airspeed in knots.
    /// </summary>
    public float SpeedTarget { get; init; }

    /// <summary>
    /// Selected vertical speed in feet per minute.
    /// </summary>
    public float VerticalSpeedTarget { get; init; }

    public bool NavHold { get; init; }

    public bool ApproachHold { get; init; }

    /* Navigation */

    /// <summary>
    /// False in free flight. The remaining navigation properties are null when this is false.
    /// </summary>
    public bool HasActiveFlightPlan { get; init; }

    /// <summary>
    /// Identifier of the next waypoint, e.g. an airport ICAO code or a fix name.
    /// </summary>
    public string? NextWaypointId { get; init; }

    /// <summary>
    /// Distance to the next waypoint in nautical miles.
    /// </summary>
    public float? DistanceToWaypointNm { get; init; }

    /// <summary>
    /// Estimated time en route to the next waypoint, in seconds.
    /// </summary>
    public int? WaypointEteSeconds { get; init; }

    /// <summary>
    /// Identifier of the final waypoint of the flight plan.
    /// </summary>
    public string? DestinationId { get; init; }

    /// <summary>
    /// Distance to the destination in nautical miles.
    /// </summary>
    public float? DistanceToDestinationNm { get; init; }

    /// <summary>
    /// Estimated time en route to the destination, in seconds.
    /// </summary>
    public int? DestinationEteSeconds { get; init; }

    /// <summary>
    /// Estimated time of arrival at the destination, as seconds since midnight UTC.
    /// </summary>
    public int? DestinationEtaUtcSeconds { get; init; }

    /// <summary>
    /// Cross-track error in nautical miles, positive right of the course line.
    /// </summary>
    public float? CrossTrackErrorNm { get; init; }

    /* Engine */

    public int EngineCount { get; init; }

    public EngineType EngineType { get; init; }

    /// <summary>
    /// Primary engine load readout for engine 1: N1 for a turbine, percentage of
    /// maximum RPM for a piston. Null when the aircraft has no engine.
    /// </summary>
    public float? EnginePrimaryPct { get; init; }

    /// <summary>
    /// Engine 1 RPM. Null for a jet, where RPM is not the readout pilots use.
    /// </summary>
    public int? EngineRpm { get; init; }

    /// <summary>
    /// Fuel flow for engine 1 in pounds per hour.
    /// </summary>
    public float? FuelFlowPph { get; init; }

    /// <summary>
    /// Engine 1 oil temperature in degrees Celsius.
    /// </summary>
    public float? OilTemperature { get; init; }

    /// <summary>
    /// Engine 1 oil pressure in psi.
    /// </summary>
    public float? OilPressure { get; init; }

    /// <summary>
    /// Engine 1 manifold pressure in inches of mercury. Piston engines only.
    /// </summary>
    public float? ManifoldPressure { get; init; }

    /* Fuel */

    /// <summary>
    /// Total fuel on board in pounds.
    /// </summary>
    public float FuelQuantityLbs { get; init; }

    /// <summary>
    /// Total fuel capacity in pounds.
    /// </summary>
    public float FuelCapacityLbs { get; init; }

    /// <summary>
    /// Flying time left at the current fuel flow, in seconds. Null when nothing is
    /// burning fuel, because then there is no meaningful endurance to show.
    /// </summary>
    public int? FuelEnduranceSeconds { get; init; }

    /* Configuration */

    /// <summary>
    /// Flap detent the handle is in, 0 being clean.
    /// </summary>
    public int FlapsHandleIndex { get; init; }

    /// <summary>
    /// Number of flap detents the aircraft has, including clean.
    /// </summary>
    public int FlapsHandlePositions { get; init; }

    /// <summary>
    /// Gear extension, 0 (up) to 100 (down). In between while the gear travels.
    /// </summary>
    public float GearPercentExtended { get; init; }

    public bool GearHandleDown { get; init; }

    /// <summary>
    /// Spoiler/speedbrake deployment, 0 to 100.
    /// </summary>
    public float SpoilersPct { get; init; }

    public bool SpoilersArmed { get; init; }

    public bool ParkingBrakeOn { get; init; }

    /// <summary>
    /// Elevator trim, -100 (full nose down) to 100 (full nose up).
    /// </summary>
    public float ElevatorTrimPct { get; init; }

    /* Lights */

    public bool LandingLightsOn { get; init; }

    public bool TaxiLightsOn { get; init; }

    public bool StrobeLightsOn { get; init; }

    public bool NavLightsOn { get; init; }

    public bool BeaconOn { get; init; }

    /* Miscellaneous */

    /// <summary>
    /// Name of the aircraft as the sim reports it.
    /// </summary>
    public string AircraftTitle { get; init; } = string.Empty;

    /// <summary>
    /// In-sim UTC time, as seconds since midnight.
    /// </summary>
    public int SimTimeUtcSeconds { get; init; }
}
