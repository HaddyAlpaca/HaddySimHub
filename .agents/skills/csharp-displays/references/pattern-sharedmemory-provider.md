# Shared Memory Provider Pattern

## Telemetry Struct

```csharp
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct GameTelemetry
{
    public float SteerAngle;
    public float ThrottleInput;
    public float BrakeInput;
    public float Rpm;
    public float MaxRpm;
    public float Gear;
    public float SpeedMs;
    public float AirTemp;
    public float RoadTemp;
    public int SessionType;
    public int CurrentLapCount;
    // ... other fields
}
```

## SharedMemoryReader

```csharp
public class GameSharedMemoryReader : IDisposable
{
    private const string SharedMemoryName = "Local\\game_physical";
    private MemoryMappedFile? _memoryMappedFile;
    private MemoryMappedViewAccessor? _viewAccessor;

    public bool IsConnected { get; private set; }

    public void Connect()
    {
        _memoryMappedFile = MemoryMappedFile.OpenExisting(SharedMemoryName);
        _viewAccessor = _memoryMappedFile.CreateViewAccessor(0, Marshal.SizeOf<GameTelemetry>());
        IsConnected = true;
    }

    public void Disconnect()
    {
        _viewAccessor?.Dispose();
        _viewAccessor = null;
        _memoryMappedFile?.Dispose();
        _memoryMappedFile = null;
        IsConnected = false;
    }

    public bool TryReadTelemetry(out GameTelemetry telemetry)
    {
        telemetry = default;
        if (_viewAccessor == null) return false;
        _viewAccessor.Read(0, out telemetry);
        return true;
    }

    public void Dispose()
    {
        Disconnect();
    }
}
```

## GameDataProvider

Extend `SharedMemoryGameDataProviderBase<TReader, TTelemetry>`:

```csharp
public class GameDataProvider : SharedMemoryGameDataProviderBase<GameSharedMemoryReader, GameTelemetry>
{
    protected override GameSharedMemoryReader CreateReader() => new();
    protected override void ConnectReader(GameSharedMemoryReader reader) => reader.Connect();
    protected override void DisconnectReader(GameSharedMemoryReader? reader) => reader?.Dispose();
    protected override bool IsConnected(GameSharedMemoryReader? reader) => reader?.IsConnected ?? false;
    protected override bool TryReadTelemetry(GameSharedMemoryReader reader, out GameTelemetry telemetry)
        => reader.TryReadTelemetry(out telemetry);
    protected override bool HasDataChanged(GameTelemetry current, GameTelemetry last)
        => current.Rpm != last.Rpm || current.Gear != last.Gear;
}
```

## DataConverter

```csharp
public class GameDataConverter : IDataConverter<GameTelemetry, DisplayUpdate>
{
    public DisplayUpdate Convert(GameTelemetry data)
    {
        var raceData = new RaceData
        {
            SessionType = GetSessionType(data.SessionType),
            CurrentLap = data.CurrentLapCount,
            Speed = Convert.ToInt32(data.SpeedMs * 3.6f),
            Gear = data.Gear == 0 ? "N" : data.Gear < 0 ? "R" : data.Gear.ToString(),
            Rpm = Convert.ToInt32(data.Rpm),
            RpmMax = Convert.ToInt32(data.MaxRpm),
            AirTemp = data.AirTemp,
            TrackTemp = data.RoadTemp,
            // ... other mappings
        };

        return new DisplayUpdate { Type = DisplayType.RaceDashboard, Data = raceData };
    }
}
```
