using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;
using HaddySimHub.GameData;

namespace HaddySimHub.DirtRally2;

public sealed class Dirt2Game : Game
{
    private const int PORT = 20777;
    private readonly UdpClient _client = new UdpClient(PORT);
    private IPEndPoint _endPoint = new IPEndPoint(IPAddress.Any, PORT);

    public override void Start()
    {
        base.Start();

        // Start receiving updates.
        _client.BeginReceive(new AsyncCallback(ReceiveCallback), null);
    }

    public override void Stop()
    {
        base.Stop();

        _client.Close();
    }

    public override string Description => "Dirt Rally 2";

    protected override string _processName => "dirtrally2";

    protected override Func<object, DisplayUpdate> GetDisplayUpdate => Dashboard.GetDisplayUpdate;

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
