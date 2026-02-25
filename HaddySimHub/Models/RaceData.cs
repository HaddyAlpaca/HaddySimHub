namespace HaddySimHub.Models;

public sealed record RaceData
{
    // Universal mandatory fields
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

    public int RpmMax { get; init; }

    public float TrackTemp { get; init; }

    public float AirTemp { get; init; }

    public float FuelRemaining { get; init; }

    public float FuelAvgLap { get; init; }

    public float FuelLastLap { get; init; }

    public float FuelEstLaps { get; init; }

    public float CurrentLapTime { get; init; }

    public float LastLapTime { get; init; }

    public float LastLapTimeDelta { get; init; }

    public float BestLapTime { get; init; }

    public float BestLapTimeDelta { get; init; }

    public int ClutchPct { get; init; }

    public int ThrottlePct { get; init; }

    public int BrakePct { get; init; }

    public bool PitLimiterOn { get; init; }

    public required string CarNumber { get; init; }

    public int SteeringPct { get; init; }

    // Optional sim-specific fields
    /// <summary>iRacing, ACC: Brake bias percentage</summary>
    public float? BrakeBias { get; init; }

    /// <summary>iRacing only: Strength of Field</summary>
    public int? StrengthOfField { get; init; }

    /// <summary>iRacing only: Current incidents</summary>
    public long? Incidents { get; init; }

    /// <summary>iRacing only: Max incidents allowed</summary>
    public long? MaxIncidents { get; init; }

    /// <summary>iRacing only: iRating</summary>
    public int? IRating { get; init; }

    /// <summary>iRacing only: Safety Rating</summary>
    public int? SafetyRating { get; init; }

    /// <summary>ACC only: Penalty count</summary>
    public int? Penalties { get; init; }

    /// <summary>ACC only: Penalty time in seconds</summary>
    public int? PenaltyTime { get; init; }

    /// <summary>F1, ACC: DRS remaining uses</summary>
    public int? DrsRemaining { get; init; }

    /// <summary>F1, ACC: DRS enabled status</summary>
    public bool? DrsEnabled { get; init; }

    /// <summary>F1 only: Left brake temperature</summary>
    public int? BrakeTempLeft { get; init; }

    /// <summary>F1 only: Right brake temperature</summary>
    public int? BrakeTempRight { get; init; }

    /// <summary>F1 only: Tyre compound</summary>
    public string? TyreCompound { get; init; }
}
