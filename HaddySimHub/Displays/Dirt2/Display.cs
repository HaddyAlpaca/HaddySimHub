using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;
using HaddySimHub.Models;
using HaddySimHub.Shared;

namespace HaddySimHub.Displays.Dirt2;

internal sealed class Display() : DisplayBase<Packet>()
{
    private const int PORT = 20777;
    private IPEndPoint _endPoint = new(IPAddress.Any, PORT);
    private UdpClient? _client;

    public override string Description => "Dirt Rally 2";
    public override bool IsActive => ProcessHelper.IsProcessRunning("dirtrally2");
    public override void Start()
    {
        _client ??= new UdpClient(PORT);
        _client.BeginReceive(new AsyncCallback(ReceiveCallback), null);
    }

    public override void Stop()
    {
        if (_client is null)
        {
            return;
        }

        _client.Close();
        _client = null;
    }

    private async void ReceiveCallback(IAsyncResult result)
    {
        if (_client is null || _endPoint is null)
        {
            return;
        }

        // Get data we received.
#pragma warning disable CS8601 // Possible null reference assignment.
        byte[] data = _client.EndReceive(result, ref _endPoint);
#pragma warning restore CS8601 // Possible null reference assignment.

        // Start receiving again.
        _client.BeginReceive(new AsyncCallback(ReceiveCallback), null);

        GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);

        try
        {
            // Get the header to retrieve the packet ID.
            var packet = Marshal.PtrToStructure<Packet>(handle.AddrOfPinnedObject());
            await this.SendUpdate(packet);
        }
        finally
        {
            handle.Free();
        }
    }

    protected override DisplayUpdate ConvertToDisplayUpdate(Packet data)
    {
        var displayData = new RallyData
        {
            Speed = Convert.ToInt32(data.speed_ms * 3.6),
            Rpm = Convert.ToInt32(data.rpm * 10),
            RpmMax = Convert.ToInt32(data.max_rpm * 10),
            Gear = data.gear == 0 ? "N" : data.gear < 0 ? "R" : data.gear.ToString(),
            Clutch = Convert.ToInt32(data.clutch * 100),
            Brake = Convert.ToInt32(data.brakes * 100),
            Throttle = Convert.ToInt32(data.throttle * 100),
            CompletedPct = Math.Min(Convert.ToInt32(data.progress * 100), 100),
            DistanceTravelled = Math.Max(Convert.ToInt32(data.distance), 0),
            Position = Convert.ToInt32(data.car_pos),
            Sector1Time = data.sector_1_time,
            Sector2Time = data.sector_2_time,
            LapTime = data.lap_time,
        };

        return new DisplayUpdate { Type = DisplayType.RallyDashboard, Data = displayData };
    }
}
#pragma warning restore CS0649
