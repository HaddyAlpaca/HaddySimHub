using HaddySimHub.Displays;
using HaddySimHub.Models;
using HaddySimHub.Shared;
using System.Text;

namespace HaddySimHub;

public class DisplaysRunner
{
    private readonly DisplayUpdate _idleDisplayUpdate = new() { Type = DisplayType.None };
    private readonly IEnumerable<IDisplay> _displays;
    private bool _lastStateHadDisplays = false;

    public DisplaysRunner(IEnumerable<IDisplay> displays)
    {
        _displays = displays ?? Array.Empty<IDisplay>();
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
                // Only log suggestions once when transitioning from having displays to none
                if (_lastStateHadDisplays)
                {
                    Logger.Debug("No active displays found");
                    LogAvailableGames();
                    _lastStateHadDisplays = false;
                }
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

    private void LogAvailableGames()
    {
        var runningGames = ProcessHelper.GetRunningGameProcesses();
        
        StringBuilder sb = new();
        sb.AppendLine("=== Game Detection Status ===");
        
        if (runningGames.Count > 0)
        {
            sb.AppendLine("Currently running detected games:");
            foreach (var (processName, gameName) in runningGames)
            {
                sb.AppendLine($"  ✓ {gameName}");
            }
        }
        else
        {
            sb.AppendLine("No supported games detected.");
            sb.AppendLine("Please start one of the following games:");
            sb.AppendLine("  • Assetto Corsa Competizione (ac2.exe)");
            sb.AppendLine("  • Assetto Corsa (AC.exe)");
            sb.AppendLine("  • Assetto Corsa Rally (ACR.exe)");
            sb.AppendLine("  • Euro Truck Simulator 2 (eurotrucks2.exe)");
            sb.AppendLine("  • American Truck Simulator (americantrucks.exe)");
            sb.AppendLine("  • iRacing (iRacingUI.exe)");
            sb.AppendLine("  • Dirt Rally 2.0 (dirtrally2.exe)");
        }
        sb.AppendLine("==============================");
        
        Logger.Debug(sb.ToString());
