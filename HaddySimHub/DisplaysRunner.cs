using HaddySimHub.Displays;
using HaddySimHub.Interfaces;
using HaddySimHub.Models;
using HaddySimHub.Shared;

namespace HaddySimHub;

public class DisplaysRunner
{
    private readonly DisplayUpdate _idleDisplayUpdate = new() { Type = DisplayType.None };
    private readonly IEnumerable<IDisplay> _displays;
    private readonly IDisplayUpdateSender _displayUpdateSender;
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(2);

    private bool? _lastStateHadDisplays;

    public DisplaysRunner(IEnumerable<IDisplay> displays, IDisplayUpdateSender displayUpdateSender)
    {
        _displays = displays ?? [];
        _displayUpdateSender = displayUpdateSender ?? throw new ArgumentNullException(nameof(displayUpdateSender));
    }

    public IDisplay? CurrentDisplay { get; private set; }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var display = SelectDisplay();

                if (display is null)
                {
                    ReportNoActiveDisplay();
                    await _displayUpdateSender.SendDisplayUpdate(_idleDisplayUpdate);
                }
                else
                {
                    _lastStateHadDisplays = true;
                }

                if (!ReferenceEquals(display, CurrentDisplay))
                {
                    Switch(from: CurrentDisplay, to: display);
                    CurrentDisplay = display;
                }

                await Task.Delay(PollInterval, cancellationToken);
            }
        }
        finally
        {
            // Cancelling the delay leaves through an exception, so the running
            // display is stopped here rather than after the loop.
            Switch(from: CurrentDisplay, to: null);
            CurrentDisplay = null;
        }
    }

    /// <summary>
    /// Picks the display to feed the dashboard from. Every update goes to the same
    /// stream, so running two at once would interleave frames of different types on
    /// one screen. The display already running keeps its turn for as long as its
    /// game is up, which stops the choice flip-flopping while two games are open.
    /// </summary>
    private IDisplay? SelectDisplay()
    {
        if (CurrentDisplay is not null && IsActive(CurrentDisplay))
        {
            return CurrentDisplay;
        }

        return _displays.FirstOrDefault(IsActive);
    }

    private static bool IsActive(IDisplay display)
    {
        try
        {
            return display.IsActive;
        }
        catch (Exception ex)
        {
            Logger.Error($"Error checking whether {display.Description} is active: {ex.Message}");
            return false;
        }
    }

    private void Switch(IDisplay? from, IDisplay? to)
    {
        if (from is not null)
        {
            try
            {
                Logger.Info($"Stop receiving data from {from.Description}");
                from.Stop();
            }
            catch (Exception ex)
            {
                Logger.Error($"Error stopping datafeed of game {from.Description}: {ex.Message}\n\n{ex.StackTrace}");
            }
        }

        if (to is not null)
        {
            try
            {
                Logger.Info($"Start receiving data from {to.Description}");
                to.Start();
            }
            catch (Exception ex)
            {
                Logger.Error($"Error starting datafeed of game {to.Description}: {ex.Message}\n\n{ex.StackTrace}");
            }
        }
    }

    private void ReportNoActiveDisplay()
    {
        if (_lastStateHadDisplays == false)
        {
            return;
        }

        Logger.Info("No active displays found");

        if (Logger.IsDataLoggingEnabled)
        {
            var processNames = ProcessHelper.GetRunningProcessNames();
            Logger.Debug($"No game process detected. Running processes ({processNames.Count}): {string.Join(", ", processNames)}");
        }

        _lastStateHadDisplays = false;
    }
}
