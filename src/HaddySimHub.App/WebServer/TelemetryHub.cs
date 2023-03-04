using HaddySimHub.Telemetry;
using Microsoft.AspNetCore.SignalR;

namespace HaddySimHub.WebServer;

internal class TelemetryHub : Hub
{
    readonly IHubContext<TelemetryHub> _hubContext;

    public TelemetryHub(IHubContext<TelemetryHub> hubContext, ITelemetryWatcher telemetryWatcher)
    {
        _hubContext= hubContext;

        telemetryWatcher.TelemetryUpdated += async (s, e) =>
            await _hubContext.Clients.All.SendAsync("telemetry-update", new { e.TelemetryType, e.Data });
    }
}