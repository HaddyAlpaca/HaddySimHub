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
            var isRunning = ProcessHelper.IsProcessRunning("ac2");
            Logger.Debug($"[ACC Display] Process detection check - ac2 is {(isRunning ? "RUNNING" : "NOT RUNNING")}");
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
