# Game display pipeline

Each supported simulator is wired up the same way. Understanding this one
pattern is enough to read, debug, or extend any game integration.

```
IGameDataProvider<T>  →  IDataConverter<T, DisplayUpdate>  →  DisplayBase<T>  →  IDisplayUpdateSender  →  SSE  →  frontend
        (raw telemetry T)        (maps to DisplayUpdate)       (channel + send loop)     (/display-data/stream)
```

## The pieces

| Role | Type | Responsibility |
|---|---|---|
| Provider | `IGameDataProvider<T>` | Acquires raw telemetry from the running game (shared memory, UDP, SDK) and raises `DataReceived` with a payload of type `T`. |
| Converter | `IDataConverter<T, DisplayUpdate>` | `Convert(T)` maps the game-specific telemetry into the shared `DisplayUpdate` model the frontend understands. |
| Display | `DisplayBase<T>` / `SimpleGameDisplay<T>` | Subscribes to the provider, runs the converter, and pushes each `DisplayUpdate` through a bounded channel + send loop. Exposes `Description` and `IsActive`. |
| Sender | `IDisplayUpdateSender` → `SseBroadcastService` | Forwards each `DisplayUpdate` to all SSE clients via a `Channel<DisplayUpdate>` per connection (endpoint at `/display-data/stream`). |
| Runner | `DisplaysRunner` | Polls every display's `IsActive` every ~2s and keeps exactly one data feed running as games come and go. |

`SimpleGameDisplay<T>` reports `IsActive` via `ProcessHelper.IsProcessRunning(processName)`,
so a display becomes active when the game's process is detected.

Every display writes to the same stream, so `DisplaysRunner` feeds only one of them
at a time. The display already running keeps its turn for as long as its game is up,
and the rest report `standing by` in the console dashboard. Without that, two open
games would interleave frames of different types on one screen.

### Why detection uses the process and not the shared memory

For most games the shared memory name identifies the title, so "is the page
there?" would answer "is the game running?". The Assetto Corsa family is the
exception: Assetto Corsa, Assetto Corsa Competizione and Assetto Corsa Rally all
publish under the same names — `Local\acpmf_physics`, `Local\acpmf_graphics` and
`Local\acpmf_static`. Their page layouts differ, but the names do not, so the
presence of a page cannot tell the three titles apart and would activate every
Assetto Corsa display at once.

The process name is what distinguishes them, so detection stays process-based and
shared memory answers the separate question of whether telemetry is flowing. That
split is what the `◐ running · waiting for data` state in the console dashboard
reports.

| Title | Process |
|---|---|
| Assetto Corsa | `acs` |
| Assetto Corsa Competizione | `AC2-Win64-Shipping` |
| Assetto Corsa Rally | `acr` |

Assetto Corsa also ships a 32-bit build that runs as `acs_x86`; only the 64-bit
build is detected.

The three titles do not share a page *layout*, only the names. Each game folder
therefore carries its own structs, and each has a test pinning the size of every
page it maps, so a change that shifts a field fails the build instead of silently
reading from the wrong offset.

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

## Debugging a game that "doesn't work"

The pipeline distinguishes two failure modes and surfaces both:

- **Not detected** — `IsActive` is `false` because no process matches the
  configured name. With `HADDYSIMHUB_DEBUG=1`, `DisplaysRunner` logs the list of
  running process names when no display is active, so you can confirm the exact
  executable name to put in `DisplayDefinitions`.
- **Detected but no data** — the process runs (panel shows it) but no telemetry
  arrives. The console dashboard's Games panel marks each game:
  `○` not running · `◌ running · standing by` · `◐ running · waiting for data` ·
  `● live · <age> ago`.
  `DisplayBase` logs *"First telemetry received from …"* on the first frame and
  shared-memory providers warn *"process detected but shared memory is not
  connected"* via `SharedMemoryGameDataProviderBase`.

Additional aids:

- `Logger.Warn` is used for missed connections and dropped telemetry frames
  (visible without debug mode).
- With `HADDYSIMHUB_DEBUG=1`, every raw telemetry frame is serialised to
  `log/<date>-HaddySimHub-data.log` via `Logger.LogData`, wired centrally in
  `DisplayBase`, so converter mapping issues can be diagnosed offline.

## Test displays

Test displays push sample data so the frontend can be exercised without a game
running. They are registered with `RegisterTestDisplay<T>(id)` and toggled at
runtime by pressing `Ctrl+T` in the backend console, which cycles
`Program.TestId` through `race` → `rally` → `truck` → off.
