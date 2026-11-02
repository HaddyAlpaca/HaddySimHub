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

### Added
- Microsoft Flight Simulator 2020 support, with a flight dashboard showing the primary instruments (airspeed, artificial horizon, altitude, heading tape with heading bug and ground track), autopilot modes and targets, flight plan progress with distances and ETA, and engine, fuel and airframe configuration.
- Telemetry is read over SimConnect through a hand-written FFI layer, because there is no maintained Rust binding for the vendor DLL. `SimConnect.dll` is located next to the executable or in an MSFS SDK install; when it is missing the display stays inactive and logs where it looked.
- A course deviation indicator, driven by the navigation radio when a station is received and by the flight plan otherwise, with the glideslope shown alongside when a localiser carries one. The flight plan scale tightens from two nautical miles to a third of one once an approach is active, so the deviation stays readable on final.
- The race dashboard uses larger text for the session and fuel cards, so lap times, lap deltas and brake balance are readable. The pedal trace covers a fixed twenty-second window at 20 Hz instead of the last few seconds of input.

### Removed
- The .NET backend, the Lit web frontend, the web server and the separate updater.
- `--e2e` and `--capture`, together with the loopback test-update route and the Playwright end-to-end suite they served.
- Raw telemetry capture and replay, which ADR-0006 replaced with readers built on existing crates.
- The scheduled dependency-update workflow.

### Changed
- Direct colour-coded console logging and a daily file in `log/` replace the live console dashboard, keeping debug and per-frame logging without a terminal UI.
- The dashboard keeps only the newest snapshot and queues at most one repaint, so telemetry faster than the screen never builds a backlog.

### Tests
- Every decoder is unit-tested against the byte layout its game publishes; the remaining checks that need a running game are tracked in `docs/live-verification.md`.

### Build
- CI checks formatting, tests, unused dependencies, licenses, bans, advisories and coverage on every pull request. CD builds `HaddySimHub.exe`, zips it as `haddy-simhub.zip` and publishes the GitHub release that installed copies update from.

### Known gaps
- Behaviour against running games is still to be confirmed for the items in `docs/live-verification.md`.
- ACC no longer shows the session lap total in lap-limited races: `simetry` does not expose it.
- The course deviation indicator of the flight dashboard is shown as values rather than drawn as a needle.
- The dashboard window is at least 1440×900; unlike the web page it does not shrink below that.

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
