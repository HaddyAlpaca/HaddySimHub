namespace HaddySimHub.Models;

public sealed record RaceData
{
    /// <summary>
    /// Gets session type description.
    /// </summary>
    public string SessionType { get; init; } = string.Empty;

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
    /// Gets gear.
    /// </summary>
    public required string Gear { get; init; }

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

    public int LastSectorNum { get; init; }

    public double LastSectorTime { get; init; }

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
    public string Flag { get; init; } = string.Empty;

    /// <summary>
    /// Gets timing information for all cars.
    /// </summary>
    public TimingEntry[] TimingEntries { get; init; } = [];
}

public record TimingEntry
{
    public int Position { get; init; }
    public string DriverName { get; init; } = string.Empty;
    public string CarNumber { get; init; } = string.Empty;
    public string License { get; init; } = string.Empty;
    public string LicenseColor { get; init; } = string.Empty;
    public long IRating { get; init; }
    public int Laps { get; init; }
    public int LapCompletedPct { get; init; }
    public bool IsPlayer { get; init; }
    public bool IsSafetyCar { get; init; }
    public bool IsInPits { get; init; }
    public float TimeToPlayer { get; init; }
    public bool IsLapAhead { get; set; }
    public bool IsLapBehind { get; set; }
}
