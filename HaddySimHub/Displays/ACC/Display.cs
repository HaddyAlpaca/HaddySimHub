using HaddySimHub.Interfaces;
using HaddySimHub.Models;
using HaddySimHub.Shared;

namespace HaddySimHub.Displays.ACC;

/// <summary>
/// Display for Assetto Corsa Competizione
/// </summary>
public sealed class Display : DisplayBase<ACCTelemetry>
{
    public override string Description => "Assetto Corsa Competizione";
    
    public override bool IsActive
    {
        get
        {
            // Primary check: exact process name
            var isRunning = ProcessHelper.IsProcessRunning("ac2");
            Logger.Info($"[ACC Display] Process 'ac2' is {(isRunning ? "RUNNING" : "NOT RUNNING")}");
            
            if (!isRunning)
            {
                // Fallback: check by description
                var processes = ProcessHelper.FindProcessByDescription("Assetto Corsa Competizione");
                if (processes.Length > 0)
                {
                    Logger.Info($"[ACC Display] Found {processes.Length} processes matching 'Assetto Corsa Competizione':");
                    foreach (var p in processes)
                    {
                        Logger.Info($"[ACC Display] - {p.ProcessName} (PID: {p.Id})");
                    }
                    isRunning = true;
                }
                
                // Additional fallback: check common ACC executable names
                if (!isRunning)
                {
                    var altNames = new[] { "acc", "assettocorsa_competizione", "acc.exe" };
                    foreach (var name in altNames)
                    {
                        if (ProcessHelper.IsProcessRunning(name))
                        {
                            Logger.Info($"[ACC Display] Found ACC via alternative process name: {name}");
                            isRunning = true;
                            break;
                        }
                    }
                }
            }
            
            Logger.Info($"[ACC Display] Final detection result: {(isRunning ? "ACTIVE" : "INACTIVE")}");
            return isRunning;
        }
    }

    public Display(
        IGameDataProvider<ACCTelemetry> gameDataProvider,
        IDataConverter<ACCTelemetry, DisplayUpdate> dataConverter,
        IDisplayUpdateSender displayUpdateSender)
        : base(gameDataProvider, dataConverter, displayUpdateSender)
    {
    }
}
