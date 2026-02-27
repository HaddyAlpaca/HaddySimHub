using HaddySimHub.Models;
using HaddySimHub.Interfaces;
using System;
using System.Threading.Tasks;

namespace HaddySimHub.Displays;

public abstract class DisplayBase<T> : IDisplay
{
    protected readonly IGameDataProvider<T> _gameDataProvider;
    protected readonly IDataConverter<T, DisplayUpdate> _dataConverter;
    protected readonly IDisplayUpdateSender _displayUpdateSender;

    public abstract string Description { get; }
    public abstract bool IsActive { get; }

    public DisplayBase(
        IGameDataProvider<T> gameDataProvider,
        IDataConverter<T, DisplayUpdate> dataConverter,
        IDisplayUpdateSender displayUpdateSender)
    {
        _gameDataProvider = gameDataProvider ?? throw new ArgumentNullException(nameof(gameDataProvider));
        _dataConverter = dataConverter ?? throw new ArgumentNullException(nameof(dataConverter));
        _displayUpdateSender = displayUpdateSender ?? throw new ArgumentNullException(nameof(displayUpdateSender));
    }

    public virtual void Start()
    {
        _gameDataProvider.DataReceived += HandleDataReceived;
        _gameDataProvider.Start();
    }

    public virtual void Stop()
    {
        _gameDataProvider.Stop();
        _gameDataProvider.DataReceived -= HandleDataReceived;
    }

    private void HandleDataReceived(object? sender, T data)
    {
        _ = HandleDataReceivedAsync(sender, data).ConfigureAwait(false);
    }

    protected virtual async Task HandleDataReceivedAsync(object? sender, T data)
    {
        try
        {
            var update = _dataConverter.Convert(data);
            await _displayUpdateSender.SendDisplayUpdate(update);
        }
        catch (Exception ex)
        {
            Logger.Error($"Error in HandleDataReceived: {ex.Message}");
        }
    }
}
