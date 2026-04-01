# Display Implementation

## Production Display

```csharp
public sealed class Display : DisplayBase<Packet>
{
    public override string Description => "Game Name";
    public override bool IsActive => ProcessHelper.IsProcessRunning("gameexe");

    public Display(
        IGameDataProvider<Packet> gameDataProvider,
        IDataConverter<Packet, DisplayUpdate> dataConverter,
        IDisplayUpdateSender displayUpdateSender)
        : base(gameDataProvider, dataConverter, displayUpdateSender)
    {
    }
}
```

## DisplayBase<T>

The base class handles:
- Subscribing to `DataReceived` event on `Start()`
- Unsubscribing on `Stop()`
- Calling `DataConverter.Convert()` on each data received
- Sending the result via `DisplayUpdateSender`

Override `HandleDataReceivedAsync` for custom processing:

```csharp
public abstract class DisplayBase<T> : IDisplay
{
    protected readonly IGameDataProvider<T> _gameDataProvider;
    protected readonly IDataConverter<T, DisplayUpdate> _dataConverter;
    protected readonly IDisplayUpdateSender _displayUpdateSender;

    public abstract string Description { get; }
    public abstract bool IsActive { get; }

    public virtual void Start()
    {
        _gameDataProvider.DataReceived += HandleDataReceived;
        _gameDataProvider.Start();
    }

    public virtual void Stop()
    {
        _gameDataProvider.DataReceived -= HandleDataReceived;
        _gameDataProvider.Stop();
    }

    private void HandleDataReceived(object? sender, T data)
    {
        _ = HandleDataReceivedAsync(sender, data);
    }

    protected virtual async Task HandleDataReceivedAsync(object? sender, T data)
    {
        var displayUpdate = _dataConverter.Convert(data);
        await _displayUpdateSender.SendDisplayUpdate(displayUpdate);
    }
}
```
