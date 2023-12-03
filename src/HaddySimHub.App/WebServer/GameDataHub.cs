using Microsoft.AspNetCore.SignalR;

namespace HaddySimHub.WebServer;

public class GameDataHub : Hub
{
    private readonly IHubContext<GameDataHub> _hubContext;

    public GameDataHub(IHubContext<GameDataHub> hubContext)
    {
        _hubContext= hubContext;

        //Init notification service
        NotificationService.Init(hubContext);
    }
}