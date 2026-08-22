namespace HaddySimHub.Displays.ACRally;

/// <summary>
/// Flattened Assetto Corsa Rally telemetry, combining the physics, graphics and
/// static shared memory pages into the subset the display pipeline consumes.
/// </summary>
public struct ACRallyTelemetry
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

    /// <summary>Rev limit reported by the physics page; may be 0 before a car is loaded.</summary>
    public float CurrentMaxRpm;

    public float SpeedKmh;
    public float Fuel;
    public float TurboBoost;
    public float WaterTemperature;
    public int IsEngineRunning;
    public int PitLimiterOn;
    public int AbsInAction;
    public int TcInAction;

    // --- Graphics (Local\acpmf_graphics) ---

    public ACRallyStatus Status;
    public ACRallySessionType SessionType;

    /// <summary>Elapsed stage time in milliseconds.</summary>
    public int CurrentTime;

    public int LastTime;
    public int BestTime;
    public int CompletedLap;
    public int NumberOfLaps;
    public int Position;

    /// <summary>Stage progress along the spline, 0.0 at the start and 1.0 at the finish.</summary>
    public float NormalizedCarPosition;

    /// <summary>Distance covered on the stage in metres.</summary>
    public float DistanceTraveled;

    public int CurrentSectorIndex;

    /// <summary>Duration of the most recently completed sector in milliseconds.</summary>
    public int LastSectorTime;

    public int IsValidLap;
    public float SessionTimeLeft;
    public int DeltaLapTime;
    public int IsDeltaPositive;

    // --- Static (Local\acpmf_static) ---

    /// <summary>Rev limit for the current car; 0 until a session is loaded.</summary>
    public int MaxRpm;

    /// <summary>Stage length in metres.</summary>
    public float TrackSplineLength;

    public int SectorCount;
    public string CarModel;
    public string Track;

    /// <summary>Shared memory version string reported by the game.</summary>
    public string SmVersion;

    /// <summary>Game version string reported by the game.</summary>
    public string AcVersion;
}
