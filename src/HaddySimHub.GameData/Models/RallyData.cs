namespace HaddySimHub.GameData.Models;

public sealed record RallyData
{
    /// <summary>
    /// Speed in km/h
    /// </summary>
    public int Speed { get; init; }

    public int Gear { get; init; }

    public int Rpm { get; init; }

    public int MaxRpm { get; init; }

    public int DistanceTravelled { get; init; }

    public int CompletedPct { get; init; }

    /// <summary>
    /// Time elapsed in seconds
    /// </summary>
    public float TimeElapsed { get; init; }

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

    public int Sector { get; init; }
}