using Microsoft.AspNetCore.SignalR;
using HaddySimHub.Shared;

namespace HaddySimHub;

public class GameDataHub : Hub
{
    private static IHubContext<GameDataHub>? hub;

    public GameDataHub(IHubContext<GameDataHub> hubContext)
    {
        hub = hubContext;
    }

    public static async Task SendDisplayUpdate(DisplayUpdate displayUpdate)
    {
        if (hub is null) return;

        await hub.Clients.All.SendAsync("displayUpdate", displayUpdate);
    }
}