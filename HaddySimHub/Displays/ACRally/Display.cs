using HaddySimHub.Interfaces;
using HaddySimHub.Models;
using HaddySimHub.Shared;

namespace HaddySimHub.Displays.ACRally;

/// <summary>
/// Display for Assetto Corsa Rally
/// </summary>
public sealed class Display : DisplayBase<ACRallyTelemetry>
{
    // Possible process names for AC Rally (UE5-based game)
    private static readonly string[] PossibleProcessNames = new[]
    {
        "ACR-Win64-Shipping",      // Most likely: UE5 shipping build
        "ACR",                      // Shortened version
        "acrally",                  // Full name lowercase
        "acs",                      // If sharing AC1 executable (unlikely but possible)
        "AssettoCorsaRally",        // Full name
    };

    private bool? _cachedIsActive = null;
    private static string? _detectedProcessName = null;

    public override string Description => "Assetto Corsa Rally";
    
    public override bool IsActive
    {
        get
        {
            // Check if we've already found a valid process name
            if (_detectedProcessName != null)
            {
                bool isRunning = ProcessHelper.IsProcessRunning(_detectedProcessName);
                if (!isRunning)
                {
                    // Process was running before but isn't now
                    _detectedProcessName = null;
                }
                else
                {
                    return isRunning;
                }
            }

            // Try each possible process name
            foreach (var processName in PossibleProcessNames)
            {
                if (ProcessHelper.IsProcessRunning(processName))
                {
                    _detectedProcessName = processName;
                    Logger.Info($"[ACRally] Detected AC Rally process: {processName}");
                    return true;
                }
            }

            // Log once when first checking (avoid spam)
            if (_cachedIsActive == null)
            {
                Logger.Debug($"[ACRally] AC Rally process not detected. Checked: {string.Join(", ", PossibleProcessNames)}");
            }
            _cachedIsActive = false;
            
            return false;
        }
    }

    public Display(
        IGameDataProvider<ACRallyTelemetry> gameDataProvider,
        IDataConverter<ACRallyTelemetry, DisplayUpdate> dataConverter,
        IDisplayUpdateSender displayUpdateSender)
        : base(gameDataProvider, dataConverter, displayUpdateSender)
    {
    }
}
