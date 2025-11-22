using HaddySimHub.Models;
using HaddySimHub.Shared;
using HaddySimHub.Interfaces;
using SCSSdkClient.Object;

namespace HaddySimHub.Displays.ETS;

public sealed class Display : DisplayBase<SCSTelemetry>
{
    public override string Description => "Euro Truck Simulator 2";

    public override bool IsActive => ProcessHelper.IsProcessRunning("eurotrucks2");

    public Display(
        IGameDataProvider<SCSTelemetry> gameDataProvider,
        IDataConverter<SCSTelemetry, DisplayUpdate> dataConverter,
        IDisplayUpdateSender displayUpdateSender)
        : base(gameDataProvider, dataConverter, displayUpdateSender)
    {
    }
}
