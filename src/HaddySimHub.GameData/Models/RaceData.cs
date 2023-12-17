namespace HaddySimHub.GameData.Models;

public readonly struct RaceData
{
    /// <summary>
    /// Session type description
    /// </summary>
    public string SessionType { get; init; }
    public bool IsLimitedTime { get; init; }
    public bool IsLimitedSessionLaps { get; init; }
    /// <summary>
    /// Current lap this session
    /// </summary>
    public int CurrentLap { get; init; }
    /// <summary>
    /// Total laps in this session
    /// </summary>
    public int TotalLaps { get; init; }
    /// <summary>
    /// Session time remaining
    /// </summary>
    public float SessionTimeRemaining { get; init; }
    /// <summary>
    /// Position
    /// </summary>
    public int Position { get; init; }
    /// <summary>
    /// Speed in Km/h
    /// </summary>
    public int Speed { get; init; }
    /// <summary>
    /// Gear (-2 = N/A, -1 = Reverse, 0 = Neutral, >= 1 Forward gears)
    /// </summary>
    public int Gear { get; init; }
    /// <summary>
    /// Engine revs
    /// </summary>
    public int Rpm { get; init; }
    /// <summary>
    /// Track temperature
    /// </summary>
    public float TrackTemp { get; init; }
    /// <summary>
    /// Air temperature
    /// </summary>
    public float AirTemp { get; init; }
    /// <summary>
    /// Liters of remaining fuel
    /// </summary>
    public float FuelRemaining { get; init; }
    /// <summary>
    /// Brake bias
    /// </summary>
    public float BrakeBias { get; init; }
    public float CurrentLapTime { get; init; }
    /// <summary>
    /// Last laptime in seconds
    /// </summary>
    public float LastLapTime { get; init; }
    public float LastLapTimeDelta { get; init; }
    /// <summary>
    /// Best laptime in seconds
    /// </summary>
    public float BestLapTime { get; init; }
    public float BestLapTimeDelta { get; init; }
    /// <summary>
    /// Gap behind in seconds
    /// </summary>
    public float GapBehind { get; init; }
    /// <summary>
    /// Driver behind
    /// </summary>
    public string DriverBehindName { get; init; }
    /// <summary>
    /// Gap ahead in seconds
    /// </summary>
    public float GapAhead { get; init; }
    /// <summary>
    /// Driver ahead
    /// </summary>
    public string DriverAheadName { get; init; }
    public string DriverAheadLicenseColor { get; init; }
    /// <summary>
    /// Clutch percentage
    /// </summary>
    public int ClutchPct { get; init; }
    /// <summary>
    /// Throttle percentage
    /// </summary>
    public int ThrottlePct { get; init; }
    /// <summary>
    /// Brake percentage
    /// </summary>
    public int BrakePct { get; init; }
    /// <summary>
    /// Incidents occured
    /// </summary>
    public long Incidents { get; init; }
    /// <summary>
    /// Maximum incidents allowed
    /// </summary>
    public long MaxIncidents { get; init; }
    /// <summary>
    /// Pit limiter is on
    /// </summary>
    public bool PitLimiterOn { get; init; }
    /// <summary>
    /// Flag
    /// </summary>
    public string Flag { get; init; }
}
