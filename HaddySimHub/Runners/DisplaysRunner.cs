using HaddySimHub.Displays;
using HaddySimHub.Displays.Dirt2;
using HaddySimHub.Models;
using System.Text;

namespace HaddySimHub.Runners;

internal class DisplaysRunner() : IRunner
{
    private readonly DisplayUpdate _idleDisplayUpdate = new() { Type = DisplayType.None };
    private readonly IEnumerable<IDisplay> _displays =
        [
            new Dirt2DashboardDisplay(GameDataHub.SendDisplayUpdate),
            new IRacingDashboardDisplay(GameDataHub.SendDisplayUpdate),
            new Ets2DashboardDisplay(GameDataHub.SendDisplayUpdate),
        ];


    public async Task RunAsync(CancellationToken cancellationToken)
    {
        IEnumerable<IDisplay> prevActiveDisplays = [];
        while (!cancellationToken.IsCancellationRequested)
        {
            var activeDisplays = _displays.Where(d => d.IsActive).ToList();
            if (activeDisplays.Count == 0)
            {
                Logger.Debug("No active displays found");
                await GameDataHub.SendDisplayUpdate(_idleDisplayUpdate);
            }
            else
            {
                StringBuilder sb = new();
                sb.AppendLine("Active displays:");
                foreach (var display in activeDisplays)
                {
                    sb.AppendLine($"{display.Description}");
                }

                Logger.Debug(sb.ToString());
            }

            activeDisplays.Where(g => !prevActiveDisplays.Any(x => x.Description == g.Description)).ForEach(d => {
                try
                {
                    Logger.Info($"Start receiving data from {d.Description}");
                    d.Start();
                }
                catch (Exception ex)
                {
                    Logger.Error($"Error starting datafeed of game {d.Description}: {ex.Message}\n\n{ex.StackTrace}");
                }
            });
            prevActiveDisplays.Where(g => !activeDisplays.Any(x => x.Description == g.Description)).ForEach(d => {
                try
                {
                    Logger.Info($"Stop receiving data from {d.Description}");
                    d.Stop();
                }
                catch (Exception ex)
                {
                    Logger.Error($"Error stopping datafeed of game {d.Description}: {ex.Message}\n\n{ex.StackTrace}");
                }
            });
            prevActiveDisplays = activeDisplays;
        }

        await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
    }
}
