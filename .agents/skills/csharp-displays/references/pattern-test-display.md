# Test Display Pattern

Test displays generate mock data for development/testing without a running game.

## TestDisplayBase

Extends `DisplayBase<DisplayUpdate>` - generates test data every 0.5 seconds.

```csharp
public abstract class TestDisplayBase : DisplayBase<DisplayUpdate>
{
    protected abstract DisplayUpdate GenerateDisplayUpdate();
}
```

## Test Display Implementation

```csharp
public class TestDisplay : TestDisplayBase
{
    private readonly Random _random = new();

    public TestDisplay(
        string id,
        IDataConverter<DisplayUpdate, DisplayUpdate> identityDataConverter,
        IDisplayUpdateSender displayUpdateSender)
        : base(id, identityDataConverter, displayUpdateSender)
    {
    }

    protected override DisplayUpdate GenerateDisplayUpdate()
    {
        return new DisplayUpdate
        {
            Type = DisplayType.RallyDashboard,
            Data = new RallyData
            {
                Speed = _random.Next(0, 250),
                Gear = _random.Next(1, 7).ToString(),
                Rpm = _random.Next(1000, 8000),
                RpmMax = 8500,
                DistanceTravelled = _random.Next(0, 10000),
                CompletedPct = _random.Next(0, 100),
                Sector1Time = _random.NextSingle() * 60,
                Sector2Time = _random.NextSingle() * 60,
                LapTime = _random.NextSingle() * 120,
                Position = _random.Next(1, 20),
            }
        };
    }
}
```

## Race Test Display

```csharp
public class TestDisplay : TestDisplayBase
{
    private readonly Random _random = new();

    protected override DisplayUpdate GenerateDisplayUpdate()
    {
        return new DisplayUpdate
        {
            Type = DisplayType.RaceDashboard,
            Data = new RaceData
            {
                SessionType = "Race",
                IsLimitedTime = false,
                IsLimitedSessionLaps = false,
                CurrentLap = _random.Next(1, 10),
                TotalLaps = 15,
                SessionTimeRemaining = 1800f,
                Position = _random.Next(1, 30),
                Speed = _random.Next(80, 280),
                Gear = _random.Next(1, 7).ToString(),
                Rpm = _random.Next(3000, 8000),
                RpmMax = 9500,
                TrackTemp = 28.5f,
                AirTemp = 22.0f,
                FuelRemaining = _random.NextSingle() * 80,
                FuelAvgLap = 3.5f,
                CurrentLapTime = _random.NextSingle() * 100,
                LastLapTime = _random.NextSingle() * 100,
                CarNumber = "88",
                ClutchPct = _random.Next(0, 100),
                ThrottlePct = _random.Next(0, 100),
                BrakePct = _random.Next(0, 100),
                PitLimiterOn = false,
                SteeringPct = _random.Next(-100, 100),
            }
        };
    }
}
```
