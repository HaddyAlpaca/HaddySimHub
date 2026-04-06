using HaddySimHub.Interfaces;
using HaddySimHub.Models;
using HaddySimHub.Shared;

namespace HaddySimHub.Displays.ACC;

public sealed class Display : DisplayBase<ACCTelemetry>
{
    private static readonly string[] ACCProcessNames = { "acc", "ACC", "Acc" };
    
    public override string Description => "Assetto Corsa Competizione";
    
    public override bool IsActive
    {
        get
        {
            var isRunning = ACCProcessNames.Any(name => ProcessHelper.IsProcessRunning(name));
            if (!isRunning)
            {
                Logger.Debug($"[ACC] Game not detected (checked: {string.Join(", ", ACCProcessNames)})");
            }
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
