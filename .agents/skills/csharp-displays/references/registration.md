# Registration

## DisplayFactory

Register displays in `DisplayFactory.cs`:

```csharp
public IDisplay Create(string displayTypeName)
{
    return displayTypeName switch
    {
        "GameName.Display" => CreateGameDisplay<Game.Packet>(
            "gameexe",
            "Game Name"),

        "GameName.TestDisplay" => new Game.TestDisplay(
            "rally",
            _serviceProvider.GetRequiredService<IDataConverter<DisplayUpdate, DisplayUpdate>>(),
            _serviceProvider.GetRequiredService<IDisplayUpdateSender>()),

        // ... other displays
    };
}
```

## DI Registration

Register in `Program.cs` or extension method:

```csharp
services.AddSingleton<IGameDataProvider<Packet>, GameDataProvider>();
services.AddSingleton<IDataConverter<Packet, DisplayUpdate>, GameDataConverter>();
services.AddTransient<Display>();
```

## Generic Factory Method

Use `CreateGameDisplay<T>` for standard provider-converter pattern:

```csharp
private IDisplay CreateGameDisplay<T>(
    string processName,
    string description)
{
    var gameDataProvider = _serviceProvider.GetRequiredService<IGameDataProvider<T>>();
    var dataConverter = _serviceProvider.GetRequiredService<IDataConverter<T, DisplayUpdate>>();
    var displayUpdateSender = _serviceProvider.GetRequiredService<IDisplayUpdateSender>();

    return new GenericGameDisplay<T>(
        processName,
        description,
        gameDataProvider,
        dataConverter,
        displayUpdateSender);
}
```
