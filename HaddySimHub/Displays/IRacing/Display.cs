using HaddySimHub.Models;
using HaddySimHub.Shared;
using iRacingSDK;
using HaddySimHub.Interfaces;

namespace HaddySimHub.Displays.IRacing;

public sealed class Display : DisplayBase<IDataSample>
{
    public override string Description => "IRacing";
    // Use iRacing SDK's connection status instead of process detection
    // This is more reliable as it checks if the sim is actually connected and ready
    public override bool IsActive => iRacingSDK.iRacing.IsConnected;

    public Display(
        IGameDataProvider<IDataSample> gameDataProvider,
        IDataConverter<IDataSample, DisplayUpdate> dataConverter,
        IDisplayUpdateSender displayUpdateSender)
        : base(gameDataProvider, dataConverter, displayUpdateSender)
    {
    }
}
