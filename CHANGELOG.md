# Changelog

All notable changes to this project are documented in this file.

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
