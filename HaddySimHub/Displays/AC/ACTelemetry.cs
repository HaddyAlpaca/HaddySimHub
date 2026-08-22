namespace HaddySimHub.Displays.AC;

/// <summary>
/// Flattened Assetto Corsa telemetry, combining the physics, graphics and static
/// shared memory pages into the subset the display pipeline consumes.
/// </summary>
public struct ACTelemetry
{
    // --- Physics (Local\acpmf_physics) ---

    /// <summary>Increments on every physics frame; used to detect fresh data.</summary>
    public int PacketId;

    public float Gas;
    public float Brake;
    public float Clutch;
    public float SteerAngle;

    /// <summary>Raw gear index: 0 = reverse, 1 = neutral, 2 = first gear.</summary>
    public int Gear;

    public int Rpms;
    public float SpeedKmh;

    /// <summary>Fuel left in the tank, in litres.</summary>
    public float Fuel;

    public float AirTemp;
    public float RoadTemp;
    public float BrakeBias;
    public int PitLimiterOn;

    // --- Graphics (Local\acpmf_graphics) ---

    public ACStatus Status;
    public ACSessionType SessionType;

    /// <summary>Laps finished so far; 0 while on the first lap.</summary>
    public int CompletedLaps;

    public int NumberOfLaps;
    public int Position;

    /// <summary>Elapsed time on the current lap in milliseconds.</summary>
    public int CurrentTime;

    public int LastTime;

    /// <summary>Best lap in milliseconds, or <see cref="int.MaxValue"/> when none is set.</summary>
    public int BestTime;

    /// <summary>Session time left in milliseconds.</summary>
    public float SessionTimeLeft;

    public float NormalizedCarPosition;
    public float DistanceTraveled;

    // --- Static (Local\acpmf_static) ---

    /// <summary>Rev limit for the current car; 0 until a session is loaded.</summary>
    public int MaxRpm;

    /// <summary>Fuel tank capacity in litres.</summary>
    public float MaxFuel;

    public string CarModel;
    public string Track;

    /// <summary>Shared memory version string reported by the game.</summary>
    public string SmVersion;

    /// <summary>Game version string reported by the game.</summary>
    public string AcVersion;
}
