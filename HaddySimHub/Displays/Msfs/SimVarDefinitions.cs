using HaddySimHub.Displays.Msfs.Interop;

namespace HaddySimHub.Displays.Msfs;

/// <summary>One simulation variable in the data definition.</summary>
/// <param name="Name">The simvar name, exactly as the SDK spells it.</param>
/// <param name="Unit">The unit to convert into, or null for the string types.</param>
/// <param name="DataType">How the sim should encode the value in the data block.</param>
internal sealed record SimVarDefinition(string Name, string? Unit, SimConnectDataType DataType);

/// <summary>
/// The variables subscribed to, in the order they are added to the data definition.
/// </summary>
/// <remarks>
/// <b>This order is the byte layout of <see cref="MsfsTelemetry"/>.</b> Adding,
/// removing or reordering an entry here without making the same change to that
/// struct shifts every field after it, and the sim reports nothing wrong -- the
/// values simply come back as the wrong quantity. <c>SimVarDefinitionsTests</c>
/// checks the two against each other so that mistake fails the build instead.
/// <para>
/// Percentages are all requested as <c>percent</c> so they arrive on a 0-100 scale,
/// even where the underlying variable is natively "percent over 100".
/// </para>
/// </remarks>
internal static class SimVarDefinitions
{
    private const string Bool = "bool";
    private const string Degrees = "degrees";
    private const string Feet = "feet";
    private const string Knots = "knots";
    private const string Meters = "meters";
    private const string Number = "number";
    private const string Percent = "percent";
    private const string Pounds = "pounds";
    private const string Seconds = "seconds";

    private static readonly SimVarDefinition[] Definitions =
    [
        // Speed
        Value("AIRSPEED INDICATED", Knots),
        Value("AIRSPEED TRUE", Knots),
        Value("GROUND VELOCITY", Knots),
        Value("AIRSPEED MACH", "mach"),
        Value("STALL WARNING", Bool),
        Value("OVERSPEED WARNING", Bool),

        // Attitude
        Value("PLANE PITCH DEGREES", "radians"),
        Value("PLANE BANK DEGREES", "radians"),
        Value("TURN COORDINATOR BALL", "position"),
        Value("TURN INDICATOR RATE", "radians per second"),

        // Altitude
        Value("INDICATED ALTITUDE", Feet),
        Value("PLANE ALT ABOVE GROUND", Feet),
        Value("VERTICAL SPEED", "feet per minute"),
        Value("KOHLSMAN SETTING MB", "millibars"),
        Value("SIM ON GROUND", Bool),

        // Heading
        Value("PLANE HEADING DEGREES MAGNETIC", Degrees),
        Value("PLANE HEADING DEGREES TRUE", Degrees),
        Value("GPS GROUND MAGNETIC TRACK", Degrees),
        Value("AMBIENT WIND DIRECTION", Degrees),
        Value("AMBIENT WIND VELOCITY", Knots),

        // Autopilot
        Value("AUTOPILOT MASTER", Bool),
        Value("AUTOPILOT HEADING LOCK", Bool),
        Value("AUTOPILOT HEADING LOCK DIR", Degrees),
        Value("AUTOPILOT ALTITUDE LOCK", Bool),
        Value("AUTOPILOT ALTITUDE LOCK VAR", Feet),
        Value("AUTOPILOT AIRSPEED HOLD", Bool),
        Value("AUTOPILOT AIRSPEED HOLD VAR", Knots),
        Value("AUTOPILOT VERTICAL HOLD VAR", "feet per minute"),
        Value("AUTOPILOT NAV1 LOCK", Bool),
        Value("AUTOPILOT APPROACH HOLD", Bool),

        // Navigation
        Value("GPS IS ACTIVE FLIGHT PLAN", Bool),
        Value("GPS WP DISTANCE", Meters),
        Value("GPS WP ETE", Seconds),
        Value("GPS ETE", Seconds),
        Value("GPS ETA", Seconds),
        Value("GPS WP CROSS TRK", Meters),
        Value("GPS IS APPROACH ACTIVE", Bool),

        // Navigation radio, for the course deviation indicator
        Value("NAV HAS NAV:1", Bool),
        Value("NAV HAS LOCALIZER:1", Bool),
        Value("NAV HAS GLIDE SLOPE:1", Bool),
        Value("NAV CDI:1", Number),
        Value("NAV GSI:1", Number),
        Value("NAV TOFROM:1", "enum"),
        Value("NAV OBS:1", Degrees),

        // Engine
        Value("NUMBER OF ENGINES", Number),
        Value("ENGINE TYPE", "enum"),
        Value("TURB ENG N1:1", Percent),
        Value("GENERAL ENG PCT MAX RPM:1", Percent),
        Value("GENERAL ENG RPM:1", "rpm"),
        Value("ENG FUEL FLOW PPH:1", "pounds per hour"),
        Value("GENERAL ENG OIL TEMPERATURE:1", "celsius"),
        Value("GENERAL ENG OIL PRESSURE:1", "psi"),
        Value("GENERAL ENG MANIFOLD PRESSURE:1", "inHg"),

        // Fuel
        Value("FUEL TOTAL QUANTITY WEIGHT", Pounds),
        Value("FUEL TOTAL CAPACITY", "gallons"),
        Value("FUEL WEIGHT PER GALLON", Pounds),

        // Configuration
        Value("FLAPS HANDLE INDEX", Number),
        Value("FLAPS NUM HANDLE POSITIONS", Number),
        Value("GEAR TOTAL PCT EXTENDED", Percent),
        Value("GEAR HANDLE POSITION", Bool),
        Value("SPOILERS HANDLE POSITION", Percent),
        Value("SPOILERS ARMED", Bool),
        Value("BRAKE PARKING POSITION", Bool),
        Value("ELEVATOR TRIM PCT", Percent),

        // Lights
        Value("LIGHT LANDING", Bool),
        Value("LIGHT TAXI", Bool),
        Value("LIGHT STROBE", Bool),
        Value("LIGHT NAV", Bool),
        Value("LIGHT BEACON", Bool),

        // Miscellaneous
        Value("ZULU TIME", Seconds),

        // Strings
        Text("GPS WP NEXT ID", SimConnectDataType.String32),
        Text("GPS APPROACH AIRPORT ID", SimConnectDataType.String32),
        Text("NAV IDENT:1", SimConnectDataType.String32),
        Text("TITLE", SimConnectDataType.String256),
    ];

    public static IReadOnlyList<SimVarDefinition> All => Definitions;

    /// <summary>
    /// Total size of the data block the sim will send, which must equal the marshalled
    /// size of <see cref="MsfsTelemetry"/>.
    /// </summary>
    public static int TotalSizeInBytes => Definitions.Sum(definition => SizeInBytes(definition.DataType));

    /// <summary>
    /// Payload size of a data type. Only the types actually used are mapped: an
    /// unmapped one means a definition and the struct have drifted apart, and that
    /// should be loud rather than silently sized.
    /// </summary>
    public static int SizeInBytes(SimConnectDataType dataType) => dataType switch
    {
        SimConnectDataType.Float64 => 8,
        SimConnectDataType.String32 => 32,
        SimConnectDataType.String256 => 256,
        _ => throw new ArgumentOutOfRangeException(nameof(dataType), dataType, "Unsupported SimConnect data type."),
    };

    private static SimVarDefinition Value(string name, string unit) =>
        new(name, unit, SimConnectDataType.Float64);

    private static SimVarDefinition Text(string name, SimConnectDataType dataType) =>
        new(name, null, dataType);
}
