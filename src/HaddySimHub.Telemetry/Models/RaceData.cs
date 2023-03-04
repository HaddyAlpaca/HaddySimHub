namespace HaddySimHub.Telemetry.Models;

public enum Flag
{
    None,
    Green,
    Blue,
    Yellow,
    Red,
    BlackAndOrange,
    Black,
    White,
    Chequered
}

public enum GripLevel
{
    Unknown,
    Green,
    Fast,
    Optimum,
    Greasy,
    Damp,
    Wet,
    Flooded
}

public enum WeatherType
{
    Unknown,
    Dry,
    Drizzle,
    LightRain,
    MediumRain,
    HeavyRain,
    Thunderstorm
}

public struct RaceData
{
    /// <summary>
    /// Is this session timed instead of a fixed number of laps?
    /// </summary>
    public bool IsTimedSession { get; init; }
    /// <summary>
    /// No of completed laps in this session
    /// </summary>
    public int CompletedLaps { get; init; }
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
    /// Gear (-1 = Reverse, 0 = Neutral, >= 1 Forward gears)
    /// </summary>
    public int Gear { get; init; }
    /// <summary>
    /// Engine revs
    /// </summary>
    public int Rpm { get; init; }
    /// <summary>
    /// Maximum engine revs
    /// </summary>
    public int RpmMax { get; init; }
    /// <summary>
    /// Current grip level
    /// </summary>
    public GripLevel GripLevel { get; init; }
    /// <summary>
    /// Current flag
    /// </summary>
    public Flag Flag { get; init; }
    /// <summary>
    /// Current weather type
    /// </summary>
    public WeatherType WeatherType { get; init; }
    /// <summary>
    /// Track temperature
    /// </summary>
    public float TrackTemp { get; init; }
    /// <summary>
    /// Air temperature
    /// </summary>
    public float AirTemp { get; init; }
    /// <summary>
    /// Remaining fuel
    /// </summary>
    public float Fuel { get; init; }
    /// <summary>
    /// Fuel used per lap
    /// </summary>
    public float FuelPerLap { get; init; }
    /// <summary>
    /// Tyres temperatures [LF, RF, LR, RR]
    /// </summary>
    public TyreData<float> TyreTemps { get; init; }
    /// <summary>
    /// Tyres pressures [LF, RF, LR, RR]
    /// </summary>
    public TyreData<float> TyrePressures { get; init; }
    /// <summary>
    /// Brake temperatures [LF, RF, LR, RR]
    /// </summary>
    public TyreData<float> BrakeTemps { get; init; }
    /// <summary>
    /// Traction control level
    /// </summary>
    public int TcLevel { get; init; }
    /// <summary>
    /// ABS level
    /// </summary>
    public int AbsLevel { get; init; }
    /// <summary>
    /// Engine mapping
    /// </summary>
    public int EngineMapping { get; init; }
    /// <summary>
    /// Brake bias
    /// </summary>
    public float BrakeBias { get; init; }
    /// <summary>
    /// Estimated laptime in milliseconds
    /// </summary>
    public int EstimatedLapTime { get; init; }
    /// <summary>
    /// Last laptime in milliseconds
    /// </summary>
    public int LastLapTime { get; init; }
    /// <summary>
    /// Best laptime in milliseconds
    /// </summary>
    public int BestLapTime { get; init; }
    /// <summary>
    /// Delta time in milliseconds
    /// </summary>
    public int DeltaTime { get; init; }
    /// <summary>
    /// Gap behind in milliseconds
    /// </summary>
    public int GapBehind { get; init; }
    /// <summary>
    /// Gap ahead in milliseconds
    /// </summary>
    public int GapAhead { get; init; }
}
