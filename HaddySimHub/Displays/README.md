# Game display pipeline

Each supported simulator is wired up the same way. Understanding this one
pattern is enough to read, debug, or extend any game integration.

```
IGameDataProvider<T>  →  IDataConverter<T, DisplayUpdate>  →  DisplayBase<T>  →  IDisplayUpdateSender  →  GameDataHub (SignalR)  →  frontend
        (raw telemetry T)        (maps to DisplayUpdate)       (channel + send loop)      ("displayUpdate")
```

## The pieces

| Role | Type | Responsibility |
|---|---|---|
| Provider | `IGameDataProvider<T>` | Acquires raw telemetry from the running game (shared memory, UDP, SDK) and raises `DataReceived` with a payload of type `T`. |
| Converter | `IDataConverter<T, DisplayUpdate>` | `Convert(T)` maps the game-specific telemetry into the shared `DisplayUpdate` model the frontend understands. |
| Display | `DisplayBase<T>` / `SimpleGameDisplay<T>` | Subscribes to the provider, runs the converter, and pushes each `DisplayUpdate` through a bounded channel + send loop. Exposes `Description` and `IsActive`. |
| Sender | `IDisplayUpdateSender` → `HubService` | Forwards each `DisplayUpdate` to all SignalR clients on the `"displayUpdate"` event (hub mapped at `/display-data`). |
| Runner | `DisplaysRunner` | Polls every display's `IsActive` every ~2s and starts/stops data feeds as games come and go. |

`SimpleGameDisplay<T>` reports `IsActive` via `ProcessHelper.IsProcessRunning(processName)`,
so a display becomes active when the game's process is detected.

## Registration

Displays are registered in
[`../Extensions/ApplicationCompositionExtensions.cs`](../Extensions/ApplicationCompositionExtensions.cs)
using the `RegisterGameDisplay<TProvider, TConverter, TInput>` extension and a
typed `GameDisplayDefinition<TInput>` (process name + description) declared in
[`DisplayDefinitions.cs`](./DisplayDefinitions.cs):

```csharp
services.RegisterGameDisplay<IRacingGameDataProvider, IRacingDataConverter, iRacingSDK.IDataSample>(
    DisplayDefinitions.Game.IRacing);
```

## Folder layout per game

Each game lives in its own folder under `Displays/` (e.g. `Displays/IRacing/`):

- `*GameDataProvider.cs` — implements `IGameDataProvider<T>`.
- `*DataConverter.cs` — implements `IDataConverter<T, DisplayUpdate>`.
- `Display.cs` — the concrete display (often a thin `SimpleGameDisplay<T>` or a
  `DisplayBase<T>` subclass overriding `IsActive`).
- `TestDisplay.cs` — a `TestDisplayBase` that emits sample data without the game
  running (see below).

## Adding a new game

1. Create a `Displays/<Game>/` folder.
2. Implement a `IGameDataProvider<T>` for the game's telemetry source.
3. Implement a `IDataConverter<T, DisplayUpdate>` mapping `T` → `DisplayUpdate`.
4. Add a `GameDisplayDefinition<T>` (game process name + friendly description) to
   `DisplayDefinitions.Game`.
5. Register it with `services.RegisterGameDisplay<TProvider, TConverter, T>(...)`
   in `ApplicationCompositionExtensions`.

## Test displays

Test displays push sample data so the frontend can be exercised without a game
running. They are registered with `RegisterTestDisplay<T>(id)` and toggled at
runtime by pressing `Ctrl+T` in the backend console, which cycles
`Program.TestId` through `race` → `rally` → `truck` → off.
