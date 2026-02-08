using HaddySimHub.Interfaces;
using HaddySimHub.Models;
using HaddySimHub.Shared;

namespace HaddySimHub.Displays.AC;

/// <summary>
/// Display for Assetto Corsa
/// </summary>
public sealed class Display : DisplayBase<ACTelemetry>
{
    public override string Description => "Assetto Corsa";
    public override bool IsActive => ProcessHelper.IsProcessRunning("ac");

    public Display(
        IGameDataProvider<ACTelemetry> gameDataProvider,
        IDataConverter<ACTelemetry, DisplayUpdate> dataConverter,
        IDisplayUpdateSender displayUpdateSender)
        : base(gameDataProvider, dataConverter, displayUpdateSender)
    {
    }
}
