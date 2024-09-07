using HaddySimHub.GameData;
using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;

namespace HaddySimHub.DirtRally2;

public sealed class Dirt2Game : Game
{
    private readonly int _port = 20777;
    private readonly UdpClient _client;
    private IPEndPoint _endPoint;

    public Dirt2Game(IProcessMonitor processMonitor, CancellationToken cancellationToken)
    : base(processMonitor, cancellationToken)
    {
        _client = new UdpClient(this._port);
        _endPoint = new IPEndPoint(IPAddress.Any, this._port);

        // Start receiving updates.
        _client.BeginReceive(new AsyncCallback(ReceiveCallback), null);
    }

    public override string Description => "Dirt Rally 2";

    protected override string ProcessName => "dirtrally2";

    protected override IDisplay CurrentDisplay => new DashboardDisplay();

    private void ReceiveCallback(IAsyncResult result)
    {
        // Get data we received.
        byte[] data = _client.EndReceive(result, ref this._endPoint!);

        // Start receiving again.
        _client.BeginReceive(new AsyncCallback(ReceiveCallback), null);

        GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);

        try
        {
            // Get the header to retrieve the packet ID.
#pragma warning disable CS8605 // Unboxing a possibly null value.
            var packet = (Packet)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(Packet));
#pragma warning restore CS8605 // Unboxing a possibly null value.
            this.ProcessData(packet);
        }
        catch
        {
            Console.WriteLine("Failed to receive Dirt Rally 2 packet.");
        }
        finally
        {
            handle.Free();
        }
    }
}
