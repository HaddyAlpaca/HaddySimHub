using HaddySimHub.Models;
using System.Threading.Channels;

namespace HaddySimHub.Interfaces;

public interface ISseBroadcastService
{
    /// <summary>
    /// Registers a connected client. Every subsequent update is written to it until
    /// it is removed again, so several clients can watch the same session.
    /// </summary>
    void AddClient(ChannelWriter<DisplayUpdate> writer);

    /// <summary>
    /// Removes a client that has disconnected. Removing a client that is no longer
    /// registered does nothing, so a late teardown cannot detach a newer client.
    /// </summary>
    void RemoveClient(ChannelWriter<DisplayUpdate> writer);

    Task BroadcastAsync(DisplayUpdate displayUpdate);
}
