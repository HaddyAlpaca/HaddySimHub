# Changelog

All notable changes to this project are documented in this file.

## Unreleased

### Added
- Microsoft Flight Simulator 2020 support, with a new flight dashboard showing the primary instruments (airspeed, artificial horizon, altitude, heading tape with heading bug and ground track), autopilot modes and targets, flight plan progress with distances and ETA, and engine, fuel and airframe configuration.
- Telemetry is read over SimConnect through a hand-written interop layer, because the SDK's managed wrapper is a .NET Framework mixed-mode assembly that this application cannot load. `SimConnect.dll` is located next to the executable or in an MSFS SDK install; when it is missing the display stays inactive and logs where it looked.
- `--test flight` and a `flight` step in the `Ctrl+T` cycle serve sample flight data, so the dashboard can be developed without the simulator running.

### Tests
- Added `SimVarDefinitionsTests`, pinning the simvar list against the layout of `MsfsTelemetry`. SimConnect reports nothing when the two drift apart, so this is what turns a silently misread telemetry block into a build failure.
- Added `MsfsDataConverterTests` covering the unit and sign conventions the simulator uses, and `MsfsGameDataProviderTests` covering connect, reconnect and shutdown against a fake SimConnect client.

### Build
- `dotnet publish -p:IncludeSimConnectOnPublish=true` bundles `SimConnect.dll` from an MSFS SDK install into the published output, and fails with a clear message when the library is not found. It is opt-in, because the release workflow builds on a Linux runner without the SDK and redistributing the library is a licensing decision.

### Documentation
- Documented where `SimConnect.dll` comes from in the README, including how to bundle it into a build, and the SimConnect telemetry source in `Displays/README.md`.

## v0.1.503 - 2026-06-20

### Added
- Live console dashboard. When the backend runs in an interactive terminal it now shows a colour-coded, `btop`-style overview with the web server status, every supported game and which one is currently detected, the active test mode, and a live log feed. Set `HADDYSIMHUB_NO_DASHBOARD=1` to keep the plain coloured log output instead (this also happens automatically when output is redirected, e.g. in CI).

### Changed
- The application now shuts down gracefully on `Ctrl+C`, letting the web server and console dashboard restore the terminal before the process exits.

### Tests
- Added `DashboardLogStoreTests` covering the dashboard log buffer's capacity, eviction, and snapshot behaviour.

### Documentation
- Documented the console dashboard and its environment variables in the README.
- Added a `Displays/README.md` describing the `provider → converter → display → hub` game-display pipeline and how to add a new game.

## v0.1.499 - 2026-06-19

### Fixed
- ACC no longer fails to deliver data when the game starts after the app. The provider now retries connecting to shared memory on every polling tick, so telemetry begins flowing as soon as the game is available.

### Tests
- Added `SharedMemoryGameDataProviderTests` to verify that the provider retries until shared memory becomes available.

## v0.1.498 - 2026-06-19

### Changed
- Refactored backend startup composition to use dedicated application and pipeline extension methods.
- Replaced string-based display registration/factory wiring with typed display definitions and typed registration helpers.
- Centralized display metadata and test display IDs in a single shared definitions registry.

### Tests
- Added composition-root DI coverage for `AddHaddySimHubApplication`.
- Updated existing display factory and lifecycle tests to the typed registration API.
