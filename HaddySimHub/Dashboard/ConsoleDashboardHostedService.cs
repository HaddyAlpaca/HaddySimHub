using HaddySimHub.Displays;
using Microsoft.Extensions.Hosting;

namespace HaddySimHub.Dashboard;

/// <summary>
/// Hosts the live console dashboard for the lifetime of the application when the
/// process is attached to an interactive terminal.
/// </summary>
public sealed class ConsoleDashboardHostedService : BackgroundService
{
    private const int WebServerPort = 3333;
    private readonly ConsoleDashboard _dashboard;

    public ConsoleDashboardHostedService(IEnumerable<IDisplay> displays, DisplaysRunner displaysRunner)
    {
        _dashboard = new ConsoleDashboard(displays, displaysRunner, DashboardLogStore.Instance, WebServerPort);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!ConsoleDashboard.IsSupported)
        {
            return Task.CompletedTask;
        }

        return _dashboard.RunAsync(stoppingToken);
    }
}
