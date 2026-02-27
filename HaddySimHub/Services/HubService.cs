using Microsoft.AspNetCore.SignalR;
using HaddySimHub.Interfaces;
using HaddySimHub.Models;

namespace HaddySimHub.Services;

public class HubService : IHubService
{
    private readonly IHubContext<GameDataHub> _hubContext;

    public HubService(IHubContext<GameDataHub> hubContext)
    {
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
    }

    public async Task SendDisplayUpdateAsync(DisplayUpdate displayUpdate)
    {
        if (displayUpdate == null) return;
        await _hubContext.Clients.All.SendAsync("displayUpdate", displayUpdate);
    }
}
