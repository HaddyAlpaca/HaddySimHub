using HaddySimHub.Interfaces;
using HaddySimHub.Models;
using System.Threading.Tasks;

namespace HaddySimHub.Services;

public class DisplayUpdateSender : IDisplayUpdateSender
{
    private readonly ISseBroadcastService _broadcastService;

    public DisplayUpdateSender(ISseBroadcastService broadcastService)
    {
        _broadcastService = broadcastService ?? throw new ArgumentNullException(nameof(broadcastService));
    }

    public async Task SendDisplayUpdate(DisplayUpdate displayUpdate)
    {
        await _broadcastService.BroadcastAsync(displayUpdate);
    }
}
