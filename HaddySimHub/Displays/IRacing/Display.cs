using HaddySimHub.Models;
using HaddySimHub.Shared;
using iRacingSDK;
using HaddySimHub.Interfaces;

namespace HaddySimHub.Displays.IRacing;

public sealed class Display : DisplayBase<IDataSample>
{
    public override string Description => "IRacing";
    public override bool IsActive => ProcessHelper.IsProcessRunning("iracingui");

    public Display(
        IGameDataProvider<IDataSample> gameDataProvider,
        IDataConverter<IDataSample, DisplayUpdate> dataConverter,
        IDisplayUpdateSender displayUpdateSender)
        : base(gameDataProvider, dataConverter, displayUpdateSender)
    {
    }
}
