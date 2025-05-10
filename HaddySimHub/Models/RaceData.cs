namespace HaddySimHub.Models;

public sealed record RaceData
{
    public string SessionType { get; init; } = string.Empty;

    public bool IsLimitedTime { get; init; }

    public bool IsLimitedSessionLaps { get; init; }

    public int CurrentLap { get; init; }

    public int TotalLaps { get; init; }

    public float SessionTimeRemaining { get; init; }

    public int Position { get; init; }

    public int Speed { get; init; }

    public required string Gear { get; init; }

    public int Rpm { get; init; }

    public int RpmGreen { get; init; }

    public int RpmRed { get; init; }

    public int RpmMax { get; init; }

    public float TrackTemp { get; init; }

    public float AirTemp { get; init; }

    public float FuelRemaining { get; init; }

    public float BrakeBias { get; init; }

    public float CurrentLapTime { get; init; }

    public int StrengthOfField { get; init; }

    public int LastSectorNum { get; init; }

    public double LastSectorTime { get; init; }

    public float LastLapTime { get; init; }

    public float LastLapTimeDelta { get; init; }

    public float BestLapTime { get; init; }

    public float BestLapTimeDelta { get; init; }

    public int ClutchPct { get; init; }

    public int ThrottlePct { get; init; }

    public int BrakePct { get; init; }

    public long Incidents { get; init; }

    public long MaxIncidents { get; init; }

    public bool PitLimiterOn { get; init; }

    public string Flag { get; init; } = string.Empty;

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
