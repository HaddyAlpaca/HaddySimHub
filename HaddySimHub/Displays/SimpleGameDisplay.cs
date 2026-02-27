using HaddySimHub.Interfaces;
using HaddySimHub.Models;
using HaddySimHub.Shared;

namespace HaddySimHub.Displays;

public sealed class SimpleGameDisplay<T> : DisplayBase<T>
{
    private readonly string _processName;
    private readonly string _description;

    public override string Description => _description;
    public override bool IsActive => ProcessHelper.IsProcessRunning(_processName);

    public SimpleGameDisplay(
        string processName,
        string description,
        IGameDataProvider<T> gameDataProvider,
        IDataConverter<T, DisplayUpdate> dataConverter,
        IDisplayUpdateSender displayUpdateSender)
        : base(gameDataProvider, dataConverter, displayUpdateSender)
    {
        _processName = processName ?? throw new ArgumentNullException(nameof(processName));
        _description = description ?? throw new ArgumentNullException(nameof(description));
    }
}
