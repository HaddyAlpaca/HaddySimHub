using System.Threading.Tasks;
using HaddySimHub.GameData.Models;
using Microsoft.AspNetCore.SignalR;

namespace HaddySimHub.WebServer
{
    public static class NotificationService
    {
        private static IHubContext<GameDataHub>? hubContext;

        public static void Init(IHubContext<GameDataHub> hubContext)
        {
            NotificationService.hubContext = hubContext;
        }

        public static async Task SendTruckData(object data)
        {
            if (hubContext != null)
            {
                await hubContext.Clients.All.SendAsync("truckData", data);
            }
        }

        public static async Task SendRaceData(RaceData data)
        {
            if (hubContext != null)
            {
                await hubContext.Clients.All.SendAsync("raceData", data);
            }
        }

        public static async Task SendIdle()
        {
            if (hubContext != null)
            {
                await hubContext.Clients.All.SendAsync("gameDataIdle");
            }
        }

        public static async Task SendNotification(string message)
        {
            if (hubContext != null)
            {
                await hubContext.Clients.All.SendAsync("notification", message);
            }
        }
    }
}