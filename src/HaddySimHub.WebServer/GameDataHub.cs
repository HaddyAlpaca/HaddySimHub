using Microsoft.AspNetCore.SignalR;

namespace HaddySimHub.WebServer;

public class GameDataHub : Hub
{
    public GameDataHub(IHubContext<GameDataHub> hubContext)
    {
        NotificationService.Init(hubContext);
    }
}