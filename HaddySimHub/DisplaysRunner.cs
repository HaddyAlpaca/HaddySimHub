using HaddySimHub.Displays;
using HaddySimHub.Models;
using System.Text;

namespace HaddySimHub;

internal class DisplaysRunner()
{
    private readonly DisplayUpdate _idleDisplayUpdate = new() { Type = DisplayType.None };
    private readonly IEnumerable<IDisplay> _displays =
        [
            new Displays.Dirt2.Display(),
            new Displays.Dirt2.TestDisplay("rally"),
            new Displays.IRacing.Display(),
            new Displays.IRacing.TestDisplay("race"),
            new Displays.ETS.Display(),
            new Displays.ETS.TestDisplay("truck"),
        ];


    public IDisplay? CurrentDisplay { get; private set; }

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
            this.CurrentDisplay = activeDisplays.FirstOrDefault();
        }

        await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
    }
}
