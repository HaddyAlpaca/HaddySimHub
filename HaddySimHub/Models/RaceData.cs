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

    /// <summary>Current race position. Null when the sim does not expose it (e.g. Assetto Corsa).</summary>
    public int? Position { get; init; }

    public int Speed { get; init; }

    public required string Gear { get; init; }

    public int Rpm { get; init; }

    public int RpmMax { get; init; }

    public float TrackTemp { get; init; }

    public float AirTemp { get; init; }

    /// <summary>Fuel remaining in litres. Null when the sim does not expose it (e.g. Assetto Corsa).</summary>
    public float? FuelRemaining { get; init; }

    /// <summary>Average fuel used per lap. Null when the sim does not expose it.</summary>
    public float? FuelAvgLap { get; init; }

    /// <summary>Fuel used on the last lap. Null when the sim does not expose it.</summary>
    public float? FuelLastLap { get; init; }

    public float FuelEstLaps { get; init; }

    public float CurrentLapTime { get; init; }

    public float LastLapTime { get; init; }

    /// <summary>Delta of the last lap to the reference lap. Null when the sim does not expose it.</summary>
    public float? LastLapTimeDelta { get; init; }

    /// <summary>Best lap time. Null when the sim does not expose it (e.g. Assetto Corsa).</summary>
    public float? BestLapTime { get; init; }

    /// <summary>Delta to the best lap. Null when the sim does not expose it.</summary>
    public float? BestLapTimeDelta { get; init; }

    public int ClutchPct { get; init; }

    public int ThrottlePct { get; init; }

    public int BrakePct { get; init; }

    public bool PitLimiterOn { get; init; }

    public int SteeringPct { get; init; }

    // Optional sim-specific fields
    /// <summary>iRacing only: Expected finish position (derived from the assigned car number)</summary>
    public string? ExpectedPosition { get; init; }

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

    // Weather / track condition fields (ACC)
    /// <summary>ACC only: Rain intensity (0=no rain, 1=light, 2=medium, 3=heavy)</summary>
    public int? RainIntensity { get; init; }

    /// <summary>ACC only: Wind speed in m/s</summary>
    public float? WindSpeed { get; init; }

    /// <summary>ACC only: Wind direction in radians</summary>
    public float? WindDirection { get; init; }

    /// <summary>ACC only: Track grip status (0=green, 1=fast, 2=optimum, 3=wet)</summary>
    public int? TrackGripStatus { get; init; }
}
