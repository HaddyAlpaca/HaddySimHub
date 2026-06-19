using HaddySimHub.Interfaces;
using HaddySimHub.Shared;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace HaddySimHub.Displays.Dirt2;

public class Dirt2GameDataProvider : IGameDataProvider<Packet>, IDisposable
{
    private const int PORT = 20777;
    private readonly IUdpClientFactory _udpClientFactory;
    private readonly ConcurrentQueue<Packet> _packetQueue = new();
    private readonly object _sync = new();
    private CancellationTokenSource? _cts;
    private Task? _processingTask;
    private UdpClient? _client;
    private IPEndPoint? _senderEndPoint;
    private bool _disposed;

    public event EventHandler<Packet>? DataReceived;

    public Dirt2GameDataProvider(IUdpClientFactory udpClientFactory)
    {
        _udpClientFactory = udpClientFactory ?? throw new ArgumentNullException(nameof(udpClientFactory));
    }

    public void Start()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        lock (_sync)
        {
            if (_client is not null)
            {
                return;
            }

            _cts = new CancellationTokenSource();
            _processingTask = Task.Run(() => ProcessPacketsAsync(_cts.Token));
            _client = _udpClientFactory.Create(PORT);
            _senderEndPoint = new IPEndPoint(IPAddress.Any, PORT);
            _client.BeginReceive(ReceiveCallback, null);
        }
    }

    public void Stop()
    {
        CancellationTokenSource? cts;
        Task? processingTask;
        UdpClient? client;

        lock (_sync)
        {
            cts = _cts;
            processingTask = _processingTask;
            client = _client;

            _cts = null;
            _processingTask = null;
            _client = null;
            _senderEndPoint = null;
        }

        cts?.Cancel();
        client?.Close();
        client?.Dispose();

        if (processingTask is not null)
        {
            try
            {
                processingTask.Wait(TimeSpan.FromSeconds(1));
            }
            catch (AggregateException ex) when (ex.InnerExceptions.All(e => e is TaskCanceledException || e is OperationCanceledException))
            {
            }
        }

        cts?.Dispose();
        while (_packetQueue.TryDequeue(out _))
        {
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        Stop();
        GC.SuppressFinalize(this);
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
        catch (Exception ex)
        {
            Logger.Debug($"[Dirt2] Failed to receive UDP packet: {ex.Message}");
            return;
        }

        if (!(_cts?.Token.IsCancellationRequested ?? true))
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

    private async Task ProcessPacketsAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            if (_packetQueue.TryDequeue(out var packet))
            {
                DataReceived?.Invoke(this, packet);
            }
            else
            {
                await Task.Delay(1, token);
            }
        }
    }
}
