using HaddySimHub.Models;
using HaddySimHub.Shared;
using HaddySimHub.Interfaces;
using SCSSdkClient.Object;

namespace HaddySimHub.Displays.ETS;

public sealed class Display : DisplayBase<SCSTelemetry>
{
    public override string Description => "Euro Truck Simulator 2";

    // Use shared memory detection instead of process detection
    // This works for both ETS2 and ATS (they use the same shared memory)
    public override bool IsActive => EtsSharedMemoryHelper.IsSharedMemoryAvailable();

    public Display(
        IGameDataProvider<SCSTelemetry> gameDataProvider,
        IDataConverter<SCSTelemetry, DisplayUpdate> dataConverter,
        IDisplayUpdateSender displayUpdateSender)
        : base(gameDataProvider, dataConverter, displayUpdateSender)
    {
    }
}
