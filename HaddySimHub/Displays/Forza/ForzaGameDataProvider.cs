using HaddySimHub.Interfaces;
using HaddySimHub.Shared;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace HaddySimHub.Displays.Forza;

public sealed class ForzaGameDataProvider : IGameDataProvider<ForzaTelemetry>, IDisposable
{
    public const int DefaultPort = 5300;
    private static readonly int PacketSize = Marshal.SizeOf<ForzaTelemetry>();
    private readonly object _sync = new();
    private CancellationTokenSource? _cts;
    private Task? _receiveTask;
    private UdpClient? _client;
    private bool _disposed;

    public event EventHandler<ForzaTelemetry>? DataReceived;

    public void Start()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        lock (_sync)
        {
            if (_client is not null)
            {
                return;
            }

            try
            {
                _client = new UdpClient(new IPEndPoint(IPAddress.Any, DefaultPort));
            }
            catch (SocketException ex)
            {
                Logger.Error($"[Forza Horizon 5] Could not listen on UDP port {DefaultPort}: {ex.Message}");
                return;
            }

            _cts = new CancellationTokenSource();
            _receiveTask = ReceiveLoopAsync(_cts.Token);
        }
    }

    public void Stop()
    {
        CancellationTokenSource? cts;
        UdpClient? client;
        Task? receiveTask;

        lock (_sync)
        {
            cts = _cts;
            client = _client;
            receiveTask = _receiveTask;
            _cts = null;
            _client = null;
            _receiveTask = null;
        }

        cts?.Cancel();
        client?.Dispose();

        if (receiveTask is not null)
        {
            try
            {
                receiveTask.Wait(TimeSpan.FromSeconds(1));
            }
            catch (AggregateException ex) when (ex.InnerExceptions.All(e => e is OperationCanceledException))
            {
            }
        }

        cts?.Dispose();
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

    private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                UdpReceiveResult result = await _client!.ReceiveAsync(cancellationToken);
                if (result.Buffer.Length < PacketSize)
                {
                    Logger.Debug($"[Forza Horizon 5] Ignoring UDP packet with {result.Buffer.Length} bytes; expected at least {PacketSize}");
                    continue;
                }

                var handle = GCHandle.Alloc(result.Buffer, GCHandleType.Pinned);
                try
                {
                    var telemetry = Marshal.PtrToStructure<ForzaTelemetry>(handle.AddrOfPinnedObject());
                    DataReceived?.Invoke(this, telemetry);
                }
                finally
                {
                    handle.Free();
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (ObjectDisposedException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (SocketException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            Logger.Error($"[Forza Horizon 5] UDP receive loop stopped: {ex.Message}");
        }
    }
}
