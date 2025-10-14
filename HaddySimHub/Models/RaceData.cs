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

    public required RpmLight[] RpmLights { get; init; }

    public int RpmMax { get; init; }

    public float TrackTemp { get; init; }

    public float AirTemp { get; init; }

    public float FuelRemaining { get; init; }

    public float FuelAvgLap { get; init; }

    public float FuelLastLap { get; init; }

    public float FuelEstLaps { get; init; }

    public float BrakeBias { get; init; }

    public float CurrentLapTime { get; init; }

    public int StrengthOfField { get; init; }

    public int LastSectorNum { get; init; }

    public double LastSectorTime { get; init; }

    public float LastLapTime { get; init; }

    public float LastLapTimeDelta { get; init; }

    public float BestLapTime { get; init; }

    public float BestLapTimeDelta { get; init; }

    public int EstimatedLaps { get; init; }

    public int ClutchPct { get; init; }

    public int ThrottlePct { get; init; }

    public int BrakePct { get; init; }

    public long Incidents { get; init; }

    public long MaxIncidents { get; init; }

    public bool PitLimiterOn { get; init; }

    public required string CarNumber { get; init; }
}
