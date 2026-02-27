using HaddySimHub.Interfaces;
using HaddySimHub.Models;
using System.Threading.Tasks;

namespace HaddySimHub.Services;

public class DisplayUpdateSender : IDisplayUpdateSender
{
    private readonly IHubService _hubService;

    public DisplayUpdateSender(IHubService hubService)
    {
        _hubService = hubService ?? throw new ArgumentNullException(nameof(hubService));
    }

    public async Task SendDisplayUpdate(DisplayUpdate displayUpdate)
    {
        await _hubService.SendDisplayUpdateAsync(displayUpdate);
    }
}
