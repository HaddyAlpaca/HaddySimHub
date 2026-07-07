using HaddySimHub.Interfaces;
using HaddySimHub.Models;
using System.Threading.Channels;

namespace HaddySimHub.Services;

public class SseBroadcastService : ISseBroadcastService
{
    private ChannelWriter<DisplayUpdate>? _clientWriter;
    private readonly Lock _lock = new();

    public void SetClient(ChannelWriter<DisplayUpdate> writer)
    {
        ChannelWriter<DisplayUpdate>? old;
        lock (_lock)
        {
            old = _clientWriter;
            _clientWriter = writer;
        }
        old?.TryComplete();
    }

    public void ClearClient()
    {
        lock (_lock)
            _clientWriter = null;
    }

    public Task BroadcastAsync(DisplayUpdate displayUpdate)
    {
        ArgumentNullException.ThrowIfNull(displayUpdate);
        ChannelWriter<DisplayUpdate>? writer;
        lock (_lock)
            writer = _clientWriter;
        writer?.TryWrite(displayUpdate);
        return Task.CompletedTask;
    }
}
