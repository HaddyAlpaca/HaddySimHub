namespace HaddySimHub.GameData.Models;

public readonly struct RaceData
{
    /// <summary>
    /// Session type description
    /// </summary>
    public string SessionType { get; init; }
    /// <summary>
    /// Is this session timed instead of a fixed number of laps?
    /// </summary>
    public bool IsTimedSession { get; init; }
    /// <summary>
    /// No of laps started in this session
    /// </summary>
    public int Laps { get; init; }
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
    /// Total number of cars in session
    /// </summary>
    public int NumberOfCars { get; init; }
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
    /// Number of remaining for current fuel
    /// </summary>
    public float FuelLaps { get; init; }
    /// <summary>
    /// Brake bias
    /// </summary>
    public float BrakeBias { get; init; }
    /// <summary>
    /// Last laptime in milliseconds
    /// </summary>
    public int LastLapTime { get; init; }
    /// <summary>
    /// Best laptime in secondss
    /// </summary>
    public float BestLapTime { get; init; }
    /// <summary>
    /// Delta time in seconds
    /// </summary>
    public float DeltaTime { get; init; }
    /// <summary>
    /// Gap behind in seconds
    /// </summary>
    public float GapBehind { get; init; }
    /// <summary>
    /// Driver behind
    /// </summary>
    public string DriverBehind { get; init; }
    /// <summary>
    /// Gap ahead in seconds
    /// </summary>
    public float GapAhead { get; init; }
    /// <summary>
    /// Driver ahead
    /// </summary>
    public string DriverAhead { get; init; }
    /// <summary>
    /// Throttle percentage
    /// </summary>
    public int ThrottlePct { get; init; }
    /// <summary>
    /// Brake percentage
    /// </summary>
    public int BrakePct { get; init; }
}
