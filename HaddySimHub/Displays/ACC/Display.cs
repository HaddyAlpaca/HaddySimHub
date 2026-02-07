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
    public override bool IsActive => ProcessHelper.IsProcessRunning("assettocorsa");

    public Display(
        IGameDataProvider<ACCTelemetry> gameDataProvider,
        IDataConverter<ACCTelemetry, DisplayUpdate> dataConverter,
        IDisplayUpdateSender displayUpdateSender)
        : base(gameDataProvider, dataConverter, displayUpdateSender)
    {
    }
}
