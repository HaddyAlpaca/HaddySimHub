# Architecture overview

HaddySimHub is a Windows desktop application that detects which simulator is
running, reads its telemetry, converts it to a shared dashboard model, and draws
the matching dashboard in a Slint window. It is a single Rust executable,
`HaddySimHub.exe`; there is no web server, browser or .NET runtime.

## Repository map

```text
Cargo.toml                    workspace manifest; rust-toolchain.toml pins Rust
├── crates/simhub-app/         the HaddySimHub executable: startup, logging, runner thread
├── crates/simhub-games/       the supported games and their feed threads
├── crates/simhub-telemetry/   reading each game's source: decoders and acquisition
├── crates/simhub-model/       the display contract (DisplayUpdate) and telemetry structs
├── crates/simhub-convert/     telemetry → DisplayUpdate, one module per game
├── crates/simhub-core/        display selection, and DisplayUpdate → dashboard snapshot
├── crates/simhub-ui/          the Slint window (ui/dashboard.slint) and its bindings
└── crates/simhub-update/      single-instance guard and self-update from GitHub releases
docs/                         architecture documentation and ADRs
.github/workflows/            CI and release automation
```

`target/` and the `log/` directory the app writes next to itself are
generated and not source.

## Runtime flow

```text
main (simhub-app)
  ├─ parse options (clap), set up logging (flexi_logger)
  ├─ stop other instances, update from GitHub unless --no-update
  ├─ runner thread, every 2 s:
  │    sysinfo process list → DisplaysRunner::tick
  │      → starts/stops one GameDisplay, or shows the waiting screen
  └─ UI thread: Slint event loop

GameDisplay feed thread (one at a time):
  game source (shared memory / UDP / SimConnect / simetry)
    → simhub-telemetry decoder → telemetry struct
    → simhub-convert → DisplayUpdate
    → simhub-core::live::LiveDashboard → DashboardSnapshot
    → simhub-ui::DashboardHandle::show → repaint on the UI thread
```

## Boundaries

### Telemetry sources

`simhub-telemetry` splits every source in two. **Decoding** — bytes to a
telemetry struct — is pure and unit-tested against the offsets each game
publishes, so a wrong offset fails a build rather than drawing
a plausible dashboard. **Acquisition** is thin:

| Game | Source | Reader |
| --- | --- | --- |
| Assetto Corsa, ACC | shared memory pages | `simetry` |
| iRacing | shared memory ring buffers + session YAML | `simetry` |
| Assetto Corsa Rally | shared memory pages | `shm` + byte decoder |
| Euro Truck Simulator 2 | SCS plugin map | `shm` + byte decoder |
| DiRT Rally 2.0 | UDP port 20777 | byte decoder |
| Forza Horizon 5 | UDP port 5300 | byte decoder |
| MSFS 2020 | SimConnect, loaded at runtime | `msfs` |

Existing crates are preferred over our own code (ADR-0006). Where this crate
reads a source itself, it is because nothing maintained covers it, or because
the crate that does fails a requirement: simetry does not know AC Rally, its SCS
client spins without yielding while ETS2 is paused, and its DiRT Rally 2 reader
drops the sector times the rally dashboard shows.

### Display selection

`simhub-core::lifecycle::DisplaysRunner` decides which game feeds the dashboard.
Only one runs at a time, so two open games never interleave frames of different
dashboard types, and the one already running keeps its turn while its game is
up. Detection is by process name (case-insensitive, without `.exe`): the three
Assetto Corsa titles publish under the same shared memory names, so only the
process tells them apart. A detected process does not mean telemetry is flowing;
each feed retries until its source is ready and logs when it connects.

### Display contract

`simhub_model::DisplayUpdate` is the seam between telemetry and the UI
(ADR-0001): `Race`, `Rally`, `Truck`, `Flight`, or `None`. Converters produce it;
nothing upstream of a converter knows about dashboards, and nothing downstream
knows about games.

### Dashboard

`simhub-core::live` turns a `DisplayUpdate` into a `DashboardSnapshot`: labels,
formatted values, warning flags, and the numbers the custom instruments draw
from. It follows the web dashboards this application replaced, including the
Dutch truck labels. `simhub-ui` owns the Slint window; `DashboardHandle` accepts
snapshots from any thread but keeps only the newest and queues at most one
repaint, so telemetry faster than the screen never builds a backlog.

The window is at least 1440×900 and may be larger. Its placement — position,
size and maximized state, in physical pixels — is checked every second and saved
to `%APPDATA%\HaddySimHub\config\window.json` when it changes, so arranging it on
a multi-monitor rig needs no restart or close. At startup it is restored, unless
the saved position no longer lies on a connected monitor.

### Update and single instance

`simhub-update` wraps `self_update`: at startup it compares the version baked in
at build time (`HADDYSIMHUB_VERSION`, set by CD to the release tag) with the
latest GitHub release, replaces `HaddySimHub.exe` with the one in
`haddy-simhub.zip`, and restarts with `--no-update`. Failures are logged and
never stop the app. A second launch stops the first instance.

## Adding a game

1. Add a telemetry struct to `simhub-model/src/telemetry/`.
2. Add a reader to `simhub-telemetry`: a pure decoder with tests, plus the
   acquisition, or an adapter from an existing crate.
3. Add a converter to `simhub-convert` producing a `DisplayUpdate`.
4. Add a feed function to `simhub-games/src/feeds.rs`, reusing the shared
   memory, UDP or simetry helpers there, and an entry in `GAMES` with the
   process name.

## Build and release

From the repository root: `cargo test --workspace --locked`, `cargo fmt --all -- --check`.
CI runs those and a release build on `windows-latest`. CD builds with
`HADDYSIMHUB_VERSION=v0.1.<run>`, zips `HaddySimHub.exe` into
`haddy-simhub.zip`, and publishes a GitHub release that installed copies update
from.
