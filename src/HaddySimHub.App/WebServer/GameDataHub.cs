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

        //watcher.GameDataIdle += async (s, e) => await _hubContext.Clients.All.SendAsync("gameDataIdle");
        //watcher.RawDataUpdated += async (s, data) => await _hubContext.Clients.All.SendAsync("rawData", data);
        //watcher.TruckDataUpdated += async (s, data) => await _hubContext.Clients.All.SendAsync("truckData", data);
        //watcher.RaceDataUpdated += async (s, data) => await _hubContext.Clients.All.SendAsync("raceData", data);
    }
}