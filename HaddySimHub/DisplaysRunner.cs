using HaddySimHub.Displays;
using HaddySimHub.Interfaces;
using HaddySimHub.Models;
using HaddySimHub.Shared;
using System.Text;

namespace HaddySimHub;

public class DisplaysRunner
{
    private readonly DisplayUpdate _idleDisplayUpdate = new() { Type = DisplayType.None };
    private readonly IEnumerable<IDisplay> _displays;
    private readonly IDisplayUpdateSender _displayUpdateSender;
    private bool _lastStateHadDisplays = false;

    public DisplaysRunner(IEnumerable<IDisplay> displays, IDisplayUpdateSender displayUpdateSender)
    {
        _displays = displays ?? [];
        _displayUpdateSender = displayUpdateSender ?? throw new ArgumentNullException(nameof(displayUpdateSender));
    }

    public IDisplay? CurrentDisplay { get; private set; }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        IEnumerable<IDisplay> prevActiveDisplays = [];
        while (!cancellationToken.IsCancellationRequested)
        {
            var activeDisplays = _displays.Where(d => d.IsActive).ToList();
            if (activeDisplays.Count == 0)
            {
                if (_lastStateHadDisplays)
                {
                     Logger.Debug("No active displays found");
                    _lastStateHadDisplays = false;
                }
                await _displayUpdateSender.SendDisplayUpdate(_idleDisplayUpdate);
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
                _lastStateHadDisplays = true;
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

            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
        }
    }
}