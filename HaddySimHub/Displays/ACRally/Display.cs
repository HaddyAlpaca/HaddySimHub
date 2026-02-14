using HaddySimHub.Interfaces;
using HaddySimHub.Models;
using HaddySimHub.Shared;

namespace HaddySimHub.Displays.ACRally;

/// <summary>
/// Display for Assetto Corsa Rally
/// </summary>
public sealed class Display : DisplayBase<ACRallyTelemetry>
{
    public override string Description => "Assetto Corsa Rally";
    
    public override bool IsActive => ProcessHelper.IsProcessRunning("acr");

    public Display(
        IGameDataProvider<ACRallyTelemetry> gameDataProvider,
        IDataConverter<ACRallyTelemetry, DisplayUpdate> dataConverter,
        IDisplayUpdateSender displayUpdateSender)
        : base(gameDataProvider, dataConverter, displayUpdateSender)
    {
    }
}
