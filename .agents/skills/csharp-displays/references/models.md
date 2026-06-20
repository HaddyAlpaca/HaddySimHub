# Data Models

## DisplayUpdate

```csharp
public sealed record DisplayUpdate
{
    public DisplayType Type { get; init; }
    public object? Data { get; init; }
}
```

## DisplayType Enum

```csharp
public enum DisplayType
{
    None,
    TruckDashboard,
    RaceDashboard,
    RallyDashboard,
}
```

## RaceData

```csharp
public sealed record RaceData
{
    // Mandatory fields
    public string SessionType { get; init; }
    public bool IsLimitedTime { get; init; }
    public bool IsLimitedSessionLaps { get; init; }
    public int CurrentLap { get; init; }
    public int TotalLaps { get; init; }
    public float SessionTimeRemaining { get; init; }
    public int? Position { get; init; }       // null when the sim doesn't expose it (e.g. AC)
    public int Speed { get; init; }
    public required string Gear { get; init; }
    public int Rpm { get; init; }
    public int RpmMax { get; init; }
    public float TrackTemp { get; init; }
    public float AirTemp { get; init; }
    public float? FuelRemaining { get; init; } // null when the sim doesn't expose it
    public float? FuelAvgLap { get; init; }    // null when the sim doesn't expose it
    public float? FuelLastLap { get; init; }   // null when the sim doesn't expose it
    public float FuelEstLaps { get; init; }
    public float CurrentLapTime { get; init; }
    public float LastLapTime { get; init; }
    public float? LastLapTimeDelta { get; init; } // null when the sim doesn't expose it
    public float? BestLapTime { get; init; }      // null when the sim doesn't expose it (e.g. AC)
    public float? BestLapTimeDelta { get; init; } // null when the sim doesn't expose it
    public int ClutchPct { get; init; }
    public int ThrottlePct { get; init; }
    public int BrakePct { get; init; }
    public bool PitLimiterOn { get; init; }
    public int SteeringPct { get; init; }

    // Optional sim-specific fields
    public string? ExpectedPosition { get; init; } // iRacing: expected finish position (from car number)
    public float? BrakeBias { get; init; }
    public int? StrengthOfField { get; init; }
    public long? Incidents { get; init; }
    public long? MaxIncidents { get; init; }
    public int? IRating { get; init; }
    public int? SafetyRating { get; init; }
    public int? Penalties { get; init; }
    public int? PenaltyTime { get; init; }
    public int? DrsRemaining { get; init; }
    public bool? DrsEnabled { get; init; }
}
```

## RallyData

```csharp
public sealed record RallyData
{
    public int Speed { get; init; }
    public required string Gear { get; init; }
    public int Rpm { get; init; }
    public int RpmMax { get; init; }
    public int DistanceTravelled { get; init; }
    public int CompletedPct { get; init; }
    public float Sector1Time { get; init; }
    public float Sector2Time { get; init; }
    public float LapTime { get; init; }
    public int Position { get; init; }
}
```

## TruckData

See existing `Models/TruckData.cs` for full structure with navigation, job info, damage, and dashboard data.
