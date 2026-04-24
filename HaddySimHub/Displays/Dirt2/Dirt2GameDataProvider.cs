using HaddySimHub.Interfaces;
using HaddySimHub.Shared;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace HaddySimHub.Displays.Dirt2;

public class Dirt2GameDataProvider : IGameDataProvider<Packet>
{
    private const int PORT = 20777;
    private readonly IUdpClientFactory _udpClientFactory;
    private readonly ConcurrentQueue<Packet> _packetQueue = new();
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _processingTask;
    private UdpClient? _client;
    private IPEndPoint? _senderEndPoint;

    public event EventHandler<Packet>? DataReceived;

    public Dirt2GameDataProvider(IUdpClientFactory udpClientFactory)
    {
        _udpClientFactory = udpClientFactory ?? throw new ArgumentNullException(nameof(udpClientFactory));
        _processingTask = Task.Run(ProcessPacketsAsync);
    }

    public void Start()
    {
        if (_client is not null)
        {
            return;
        }

        _client = _udpClientFactory.Create(PORT);
        _senderEndPoint = new IPEndPoint(IPAddress.Any, PORT);
        _client.BeginReceive(ReceiveCallback, null);
    }

    public void Stop()
    {
        _cts.Cancel();
        _client?.Close();
        _client = null;
    }

    private void ReceiveCallback(IAsyncResult result)
    {
        if (_client is null)
        {
            return;
        }

        byte[] data;
        try
        {
            data = _client.EndReceive(result, ref _senderEndPoint!);
        }
        catch
        {
            return;
        }

        if (!_cts.Token.IsCancellationRequested)
        {
            _client.BeginReceive(ReceiveCallback, null);
        }

        if (data.Length < 4)
        {
            return;
        }

        GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
        try
        {
            var packet = Marshal.PtrToStructure<Packet>(handle.AddrOfPinnedObject());
            _packetQueue.Enqueue(packet);
        }
        finally
        {
            handle.Free();
        }
    }

    private async Task ProcessPacketsAsync()
    {
        while (!_cts.Token.IsCancellationRequested)
        {
            if (_packetQueue.TryDequeue(out var packet))
            {
                DataReceived?.Invoke(this, packet);
            }
            else
            {
                await Task.Delay(1, _cts.Token);
            }
        }
    }
}
