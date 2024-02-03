using System.Threading.Tasks;
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

        public static async Task SendDisplayType(DisplayType displayType)
        {
            if (hubContext != null)
            {
                await hubContext.Clients.All.SendAsync("displayType", displayType);
            }
        }

        public static async Task SendDisplayData(object data)
        {
            if (hubContext != null)
            {
                await hubContext.Clients.All.SendAsync("displayData", data);
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