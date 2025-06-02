namespace HaddySimHub.Models;

public sealed record RallyData
{
    /// <summary>
    /// Speed in km/h
    /// </summary>
    public int Speed { get; init; }

    public required string Gear { get; init; }

    public int Rpm { get; init; }

    public required RpmLight[] RpmLights { get; init; }

    public int RpmMax { get; init; }

    public int Clutch { get; init; }

    public int Brake { get; init; }

    public int Throttle { get; init; }

    public int DistanceTravelled { get; init; }

    public int CompletedPct { get; init; }

    /// <summary>
    /// Sector 1 time in seconds
    /// </summary>
    public float Sector1Time { get; init; }

    /// <summary>
    /// Sector 2 time in seconds
    /// </summary>
    public float Sector2Time { get; init; }

    /// <summary>
    /// Stage/Lap time in seconds
    /// </summary>
    public float LapTime { get; init; }

    public int Position { get; init; }
}