using HaddySimHub.GameData;
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

        public static async Task SendDisplayUpdate(DisplayUpdate displayUpdate)
        {
            if (hubContext != null)
            {
                await hubContext.Clients.All.SendAsync("displayUpdate", displayUpdate);
            }
        }
    }
}