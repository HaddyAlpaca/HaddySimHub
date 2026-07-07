using HaddySimHub.Models;
using System.Threading.Channels;

namespace HaddySimHub.Interfaces;

public interface ISseBroadcastService
{
    void SetClient(ChannelWriter<DisplayUpdate> writer);
    void ClearClient();
    Task BroadcastAsync(DisplayUpdate displayUpdate);
}
