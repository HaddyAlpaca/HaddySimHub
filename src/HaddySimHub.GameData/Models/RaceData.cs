namespace HaddySimHub.GameData.Models;

public readonly struct RaceData
{
    /// <summary>
    /// Gets session type description.
    /// </summary>
    public string SessionType { get; init; }

    public bool IsLimitedTime { get; init; }

    public bool IsLimitedSessionLaps { get; init; }

    /// <summary>
    /// Gets current lap this session.
    /// </summary>
    public int CurrentLap { get; init; }

    /// <summary>
    /// Gets total laps in this session.
    /// </summary>
    public int TotalLaps { get; init; }

    /// <summary>
    /// Gets session time remaining.
    /// </summary>
    public float SessionTimeRemaining { get; init; }

    /// <summary>
    /// Gets position.
    /// </summary>
    public int Position { get; init; }

    /// <summary>
    /// Gets speed in Km/h.
    /// </summary>
    public int Speed { get; init; }

    /// <summary>
    /// Gets gear (-2 = N/A, -1 = Reverse, 0 = Neutral, >= 1 Forward gears).
    /// </summary>
    public int Gear { get; init; }

    /// <summary>
    /// Gets engine revs.
    /// </summary>
    public int Rpm { get; init; }

    /// <summary>
    /// Gets track temperature.
    /// </summary>
    public float TrackTemp { get; init; }

    /// <summary>
    /// Gets air temperature.
    /// </summary>
    public float AirTemp { get; init; }

    /// <summary>
    /// Gets liters of remaining fuel.
    /// </summary>
    public float FuelRemaining { get; init; }

    /// <summary>
    /// Gets brake bias.
    /// </summary>
    public float BrakeBias { get; init; }

    public float CurrentLapTime { get; init; }

    /// <summary>
    /// Gets strength of field.
    /// </summary>
    public int StrengthOfField { get; init; }

    /// <summary>
    /// Gets last laptime in seconds.
    /// </summary>
    public float LastLapTime { get; init; }

    public float LastLapTimeDelta { get; init; }

    /// <summary>
    /// Gets best laptime in seconds.
    /// </summary>
    public float BestLapTime { get; init; }

    public float BestLapTimeDelta { get; init; }

    /// <summary>
    /// Gets gap behind in seconds.
    /// </summary>
    public float GapBehind { get; init; }

    /// <summary>
    /// Gets driver behind.
    /// </summary>
    public string DriverBehindName { get; init; }

    public string DriverBehindLicense { get; init; }

    public string DriverBehindLicenseColor { get; init; }

    public string DriverBehindCarNumber { get; init; }

    public long DriverBehindIRating { get; init; }

    /// <summary>
    /// Gets gap ahead in seconds.
    /// </summary>
    public float GapAhead { get; init; }

    /// <summary>
    /// Gets driver ahead.
    /// </summary>
    public string DriverAheadName { get; init; }

    public string DriverAheadLicense { get; init; }

    public string DriverAheadLicenseColor { get; init; }

    public string DriverAheadCarNumber { get; init; }

    public long DriverAheadIRating { get; init; }

    /// <summary>
    /// Gets clutch percentage.
    /// </summary>
    public int ClutchPct { get; init; }

    /// <summary>
    /// Gets throttle percentage.
    /// </summary>
    public int ThrottlePct { get; init; }

    /// <summary>
    /// Gets brake percentage.
    /// </summary>
    public int BrakePct { get; init; }

    /// <summary>
    /// Gets incidents occured.
    /// </summary>
    public long Incidents { get; init; }

    /// <summary>
    /// Gets maximum incidents allowed.
    /// </summary>
    public long MaxIncidents { get; init; }

    /// <summary>
    /// Gets a value indicating whether pit limiter is on.
    /// </summary>
    public bool PitLimiterOn { get; init; }

    /// <summary>
    /// Gets flag.
    /// </summary>
    public string Flag { get; init; }
}
