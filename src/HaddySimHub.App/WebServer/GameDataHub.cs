using HaddySimHub.GameData;
using Microsoft.AspNetCore.SignalR;

namespace HaddySimHub.WebServer;

internal class GameDataHub : Hub
{
    readonly IHubContext<GameDataHub> _hubContext;

    public GameDataHub(IHubContext<GameDataHub> hubContext, IGameDataWatcher watcher)
    {
        _hubContext= hubContext;

        watcher.GameDataIdle += async (s, e) => await _hubContext.Clients.All.SendAsync("gameDataIdle");
        watcher.RawDataUpdated += async (s, data) => await _hubContext.Clients.All.SendAsync("rawData", data);
        watcher.TruckDataUpdated += async (s, data) => await _hubContext.Clients.All.SendAsync("truckData", data);
        watcher.RaceDataUpdated += async (s, data) => await _hubContext.Clients.All.SendAsync("raceData", data);
    }
}