using HaddySimHub.Interfaces;
using HaddySimHub.Models;
using HaddySimHub.Shared;

namespace HaddySimHub.Displays.ACC;

public sealed class Display : DisplayBase<ACCTelemetry>
{
    public override string Description => "Assetto Corsa Competizione";
    
    public override bool IsActive
    {
        get
        {
            // Use shared memory detection instead of process detection
            // This is more reliable and doesn't depend on process names
            var isAvailable = ACCSharedMemoryReader.IsSharedMemoryAvailable();
            if (!isAvailable)
            {
                Logger.Debug("[ACC] Shared memory not available");
            }
            return isAvailable;
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
