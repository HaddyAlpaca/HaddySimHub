using HaddySimHub.Interfaces;
using HaddySimHub.Models;
using System.Threading.Channels;

namespace HaddySimHub.Services;

/// <summary>
/// Fans every display update out to the connected server-sent-event clients.
/// </summary>
/// <remarks>
/// Clients are tracked individually rather than as a single "current" client. A
/// browser refresh or a second dashboard opens a new stream while the old one is
/// still tearing down, and identifying clients by their writer keeps that teardown
/// from detaching the new stream.
/// </remarks>
public class SseBroadcastService : ISseBroadcastService, IDisplayUpdateSender
{
    private readonly Lock _lock = new();
    private readonly List<ChannelWriter<DisplayUpdate>> _clients = [];

    public void AddClient(ChannelWriter<DisplayUpdate> writer)
    {
        ArgumentNullException.ThrowIfNull(writer);

        lock (_lock)
        {
            _clients.Add(writer);
        }
    }

    public void RemoveClient(ChannelWriter<DisplayUpdate> writer)
    {
        ArgumentNullException.ThrowIfNull(writer);

        lock (_lock)
        {
            _clients.Remove(writer);
        }
    }

    public Task BroadcastAsync(DisplayUpdate displayUpdate)
    {
        ArgumentNullException.ThrowIfNull(displayUpdate);

        ChannelWriter<DisplayUpdate>[] clients;
        lock (_lock)
        {
            if (_clients.Count == 0)
            {
                return Task.CompletedTask;
            }

            clients = [.. _clients];
        }

        foreach (var client in clients)
        {
            // Each client has a bounded channel that drops the oldest frame when
            // full, so a slow reader falls behind rather than blocking the others.
            client.TryWrite(displayUpdate);
        }

        return Task.CompletedTask;
    }

    public Task SendDisplayUpdate(DisplayUpdate displayUpdate) => BroadcastAsync(displayUpdate);
}
