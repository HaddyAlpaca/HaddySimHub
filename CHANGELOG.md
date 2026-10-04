# Changelog

All notable changes to this project are documented in this file.

## Unreleased

### Rewritten in Rust
- HaddySimHub is now a single Rust executable with a Slint dashboard window. The .NET backend, the Lit web frontend, the web server and the separate updater are gone; nothing needs the .NET runtime or a browser.
- All eight games are supported as before. Assetto Corsa, Competizione and iRacing are read through the `simetry` crate; AC Rally, ETS2, DiRT Rally 2, Forza Horizon 5 and MSFS through tested decoders of our own.
- The app shows a waiting screen while no game runs, and `--demo <race|rally|truck|flight>` shows a dashboard with sample data.
- The window remembers its monitor, position, size and maximized state, saved as soon as they change, and reopens there. A position on a monitor that is no longer connected is ignored.
- Self-update replaces `HaddySimHub.exe` in place from the latest GitHub release and only moves forward; it no longer installs an older release whose tag merely differs.
- CI and releases run on Windows runners only.

### Removed
- `--e2e` and `--capture`, together with the web host and the raw telemetry capture they served.
- The scheduled dependency-update workflow.

### Known gaps
- Behaviour against running games is still to be confirmed for the items in `docs/live-verification.md`.
- ACC no longer shows the session lap total in lap-limited races: `simetry` does not expose it.
- The course deviation indicator of the flight dashboard is shown as values rather than drawn as a needle.
- The dashboard window is at least 1440×900; unlike the web page it does not shrink below that.

### Added
- Microsoft Flight Simulator 2020 support, with a new flight dashboard showing the primary instruments (airspeed, artificial horizon, altitude, heading tape with heading bug and ground track), autopilot modes and targets, flight plan progress with distances and ETA, and engine, fuel and airframe configuration.
- Telemetry is read over SimConnect through a hand-written interop layer, because the SDK's managed wrapper is a .NET Framework mixed-mode assembly that this application cannot load. `SimConnect.dll` is located next to the executable or in an MSFS SDK install; when it is missing the display stays inactive and logs where it looked.
- A course deviation indicator, driven by the navigation radio when a station is received and by the flight plan otherwise, with the glideslope shown alongside when a localiser carries one. The flight plan scale tightens from two nautical miles to a third of one once an approach is active, so the needle keeps meaning something on final.
- Playwright end-to-end tests can publish controlled race, rally, truck and flight updates through a loopback-only backend route, then verify the rendered dashboards without a simulator.

### Tests
- Added `SimVarDefinitionsTests`, pinning the simvar list against the layout of `MsfsTelemetry`. SimConnect reports nothing when the two drift apart, so this is what turns a silently misread telemetry block into a build failure.
- Added `MsfsDataConverterTests` covering the unit and sign conventions the simulator uses, including which source drives the course deviation indicator, and `MsfsGameDataProviderTests` covering connect, reconnect and shutdown against a fake SimConnect client.
- Added browser end-to-end coverage for all four dashboards and the backend test-update endpoint, including invalid update rejection.

### Changed
- A rejected simulation variable now reports every rejection at once rather than stopping at the first, so diagnosing a bad simvar name against a real simulator takes one run instead of several.
- The live console dashboard was removed in favour of direct colour-coded console logging, reducing startup and runtime complexity while keeping debug and per-frame file logging available.
- Replaced manual test displays and the `Ctrl+T` test-mode toggle with explicit e2e-only data injection; normal application runs do not expose the injection route.

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
