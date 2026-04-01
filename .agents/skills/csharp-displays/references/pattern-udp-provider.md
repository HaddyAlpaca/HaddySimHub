# UDP Provider Pattern

## Telemetry Struct

```csharp
#pragma warning disable CS0649
public struct Packet
{
    public float run_time;
    public float lap_time;
    public float distance;
    public float progress;
    public float speed_ms;
    public float gear;
    public float rpm;
    public float max_rpm;
    public float sector_1_time;
    public float sector_2_time;
    public float car_pos;
    // ... other fields
}
#pragma warning restore CS0649
```

## UDP GameDataProvider

```csharp
public class GameDataProvider : IGameDataProvider<Packet>
{
    private const int PORT = 20777;
    private UdpClient? _client;
    private readonly IUdpClientFactory _udpClientFactory;

    public event EventHandler<Packet>? DataReceived;

    public void Start()
    {
        _client ??= _udpClientFactory.Create(PORT);
        _client.BeginReceive(new AsyncCallback(ReceiveCallback), null);
    }

    public void Stop()
    {
        _client?.Close();
        _client = null;
    }

    private void ReceiveCallback(IAsyncResult result)
    {
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, PORT);
        byte[] data = _client!.EndReceive(result, ref endPoint);

        GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
        try
        {
            var packet = Marshal.PtrToStructure<Packet>(handle.AddrOfPinnedObject());
            DataReceived?.Invoke(this, packet);
        }
        finally
        {
            handle.Free();
            _client.BeginReceive(new AsyncCallback(ReceiveCallback), null);
        }
    }
}
```

## DataConverter

```csharp
public class GameDataConverter : IDataConverter<Packet, DisplayUpdate>
{
    public DisplayUpdate Convert(Packet data)
    {
        var displayData = new RallyData
        {
            Speed = Convert.ToInt32(data.speed_ms * 3.6),
            Rpm = Convert.ToInt32(data.rpm * 10),
            RpmMax = Convert.ToInt32(data.max_rpm * 10),
            Gear = data.gear == 0 ? "N" : data.gear < 0 ? "R" : data.gear.ToString(),
            CompletedPct = Math.Min(Convert.ToInt32(data.progress * 100), 100),
            // ... other mappings
        };

        return new DisplayUpdate { Type = DisplayType.RallyDashboard, Data = displayData };
    }
}
```
