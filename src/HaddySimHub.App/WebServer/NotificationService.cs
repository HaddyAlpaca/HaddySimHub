using HaddySimHub.GameData.Models;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace HaddySimHub.WebServer
{
    public static class NotificationService
    {
        private static IHubContext<GameDataHub>? _hubContext;

        public static void Init(IHubContext<GameDataHub> hubContext)
        {
            _hubContext = hubContext;
        }
        
        public static async Task SendTruckData(object data)
        {
            if (_hubContext != null)
            {
                await _hubContext.Clients.All.SendAsync("truckData", data);
            }
        }

        public static async Task SendRaceData(RaceData data)
        {
            if (_hubContext != null)
            {
                await _hubContext.Clients.All.SendAsync("raceData", data);
            }
        }

        public static async Task SendRawData(object data)
        {
            if (_hubContext != null)
            {
                await _hubContext.Clients.All.SendAsync("rawData", data);
            }
        }

        public static async Task SendIdle()
        {
            if (_hubContext != null)
            {
                await _hubContext.Clients.All.SendAsync("gameDataIdle");
            }
        }

    }
}
