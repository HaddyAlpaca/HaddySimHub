using HaddySimHub.Interfaces;
using HaddySimHub.Shared;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace HaddySimHub.Displays.Dirt2;

public class Dirt2GameDataProvider : IGameDataProvider<Packet>
{
    private const int PORT = 20777;
    private IPEndPoint _endPoint = new(IPAddress.Any, PORT);
    private UdpClient? _client;
    private readonly IUdpClientFactory _udpClientFactory;

    public event EventHandler<Packet>? DataReceived;

    public Dirt2GameDataProvider(IUdpClientFactory udpClientFactory)
    {
        _udpClientFactory = udpClientFactory ?? throw new ArgumentNullException(nameof(udpClientFactory));
    }

    public void Start()
    {
        _client ??= _udpClientFactory.Create(PORT);
        _client.BeginReceive(new AsyncCallback(ReceiveCallback), null);
    }

    public void Stop()
    {
        if (_client is null)
        {
            return;
        }

        _client.Close();
        _client = null;
    }

    private void ReceiveCallback(IAsyncResult result)
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
            DataReceived?.Invoke(this, packet);
        }
        finally
        {
            handle.Free();
        }
    }
}
