# Architecture overview

HaddySimHub is a Windows desktop application that reads telemetry from racing
and flight simulators, converts it to a shared dashboard model, and serves a
web frontend on port `3333`. The backend owns simulator integration and
display selection. The frontend is a renderer: it receives display updates over
Server-Sent Events (SSE) and chooses the matching dashboard component.

## Repository map

```text
HaddySimHub.sln
├── HaddySimHub/              ASP.NET Core host and application composition
│   ├── Displays/              simulator providers, converters, and lifecycle
│   ├── Extensions/            dependency registration and HTTP endpoints
│   ├── Infrastructure/        process guard, update checks, and web host
│   ├── Interfaces/            small seams between application responsibilities
│   ├── Models/                shared backend display contracts
│   └── Services/              SSE broadcasting and shared conversions
├── HaddySimHub.Shared/        code shared by the application and helpers
├── HaddySimHub.Tests/         MSTest unit and integration-style tests
├── ClientApp/                 Lit + Vite dashboard frontend
│   ├── src/app/displays/      dashboard components and display-specific models
│   ├── src/app/state/         frontend display state
│   └── src/app/sse.service.ts SSE connection and reconnect behavior
├── SCSSdkClient/              vendored SCS telemetry SDK integration
├── iRacingSDK.Net/            vendored iRacing SDK integration
├── HaddySimHubUpdater/        updater project
├── tools/                     developer and telemetry-support tools
├── docs/                      architecture documentation and ADRs
└── .github/workflows/         build, test, and deployment automation
```

`bin/`, `obj/`, `node_modules/`, `dist/`, coverage output, and runtime logs are
generated artifacts. They are not architectural source and should not be
edited.

## Runtime flow

```text
Program
  └─ WebServerHost
       ├─ AddHaddySimHubApplication
       │    ├─ register providers, converters, and displays
       │    ├─ register SseBroadcastService
       │    └─ start DisplayRunnerHostedService
       └─ ConfigureHaddySimHubPipeline
            ├─ serve ClientApp static files
            └─ expose GET /display-data/stream

simulator
  → game data provider
  → game-specific converter
  → DisplayBase / SimpleGameDisplay
  → DisplaysRunner
  → SseBroadcastService
  → SSE stream
  → ClientApp AppStore
  → dashboard component
```

`Program` performs startup orchestration: logging, single-instance handling,
update checking, cancellation, and web-server startup. Dependency registration
and HTTP endpoint definitions live in `Extensions/ApplicationCompositionExtensions.cs`.
The web host listens on all interfaces in normal mode and only on loopback in
`--e2e` mode.

## Backend boundaries

### Simulator integration

Every game-specific integration lives below `HaddySimHub/Displays/<Game>/`.
The provider owns acquisition of raw telemetry; the converter maps that
telemetry to `Models.DisplayUpdate`; the display lifecycle handles subscription,
start/stop, and forwarding. Providers may use shared memory, UDP, a vendor SDK,
or native SimConnect interop. These protocol details must not leak into the
frontend.

The full contract, registration pattern, telemetry-source notes, and debugging
steps are documented in
[the game display pipeline guide](../HaddySimHub/Displays/README.md).

### Display selection

`DisplaysRunner` polls registered displays approximately every two seconds. It
selects one active display and keeps it selected while that game remains
active. This prevents different display types from interleaving on one SSE
stream. When no game is active, it publishes a `DisplayType.None` update.

Process detection and telemetry connectivity are separate concerns: a running
process can be detected before its provider has received telemetry.

### Transport contract

`SseBroadcastService` broadcasts each `DisplayUpdate` to all connected clients.
Each SSE client has a bounded channel with `DropOldest` behavior, so a slow
browser does not block telemetry production. The endpoint sends JSON using
camel-case property names:

```json
{
  "type": "RaceDashboard",
  "data": {}
}
```

The backend model is `HaddySimHub.Models.DisplayUpdate`. The frontend mirror is
the `DisplayUpdate` interface in `ClientApp/src/app/sse.service.ts`. Changes to
the display type or data shape must update both sides and their tests.

The `--e2e` mode adds loopback-only health and display-update endpoints and does
not start the normal display runner. It exists to drive the frontend without a
running simulator.

## Frontend boundaries

`ClientApp/src/main.ts` is the Vite entry point and registers the application
element. `AppElement` owns the composition of the page: clock, connection
status, and the dashboard selected by `DisplayType`.

The frontend flow is:

```text
SseService → AppStore → AppElement → haddy-*-display
```

`SseService` owns the browser `EventSource`, connection status, reconnect
handling, and parsing of backend updates. `AppStore` owns the current display
type and data. Individual dashboard elements render their own display-specific
data and styles. Shared controls belong under `src/app/shared/`.

The frontend must remain independent of simulator protocols. It should consume
only the shared update contract.

## How to extend the system

### Add a simulator

Follow the steps in the [game display pipeline guide](../HaddySimHub/Displays/README.md):

1. Add a game folder and provider.
2. Add a converter to `DisplayUpdate`.
3. Add a typed definition in `DisplayDefinitions`.
4. Register the display in `ApplicationCompositionExtensions`.
5. Add converter/provider tests and update the pipeline documentation when the
   integration has game-specific requirements.
6. Add or reuse a frontend dashboard only if the shared display contract needs
   a new display type.

### Change the display contract

Update the backend model, frontend types, rendering branch, and tests together.
Document compatibility or migration consequences in an ADR when the change
affects the SSE contract or existing dashboards.

### Change startup or hosting

Keep `Program` as orchestration and place concrete startup concerns in
`Infrastructure/`. Update this document when the process model, listening
address, test mode, or deployment boundary changes.

## Related decisions

- [ADR-0001: shared game-display pipeline](adr/0001-game-display-pipeline.md)
- [ADR-0002: one active display and separate telemetry detection](adr/0002-single-active-display-and-telemetry-detection.md)
- [ADR-0003: explicit application structure](adr/0003-prefer-explicit-and-boring-application-structure.md)
