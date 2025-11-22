using HaddySimHub.Models;
using HaddySimHub.Shared;
using HaddySimHub.Interfaces;

namespace HaddySimHub.Displays.Dirt2;

public sealed class Display : DisplayBase<Packet>
{
    public override string Description => "Dirt Rally 2";
    public override bool IsActive => ProcessHelper.IsProcessRunning("dirtrally2");

    public Display(
        IGameDataProvider<Packet> gameDataProvider,
        IDataConverter<Packet, DisplayUpdate> dataConverter,
        IDisplayUpdateSender displayUpdateSender)
        : base(gameDataProvider, dataConverter, displayUpdateSender)
    {
    }
}
