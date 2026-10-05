# AGENTS.md

## Build & Test

The Cargo workspace is the repository root; run cargo from there. The toolchain is
pinned in `rust-toolchain.toml` (Rust 1.92.0).

- Test: `cargo test --workspace --locked`
- Format check: `cargo fmt --all -- --check` (fix with `cargo fmt --all`)
- Build the app: `cargo build -p simhub-app` (release: add `--release`); the exe is
  `target/<profile>/HaddySimHub.exe`
- Run against games: `cargo run -p simhub-app -- --no-update`
- Run with sample data: `cargo run -p simhub-app -- --demo race` (also `rally`,
  `truck`, `flight`)

## Environment & Setup

- Windows only. CI and CD run on `windows-latest`. There is no CodeQL scanning.
- Needs the MSVC C++ build tools (Visual Studio Build Tools, "Desktop development
  with C++") and `rustup`. libclang is **not** needed: `simetry` is built without the
  features that run bindgen.
- `SimConnect.dll` is loaded at runtime, so building needs no MSFS SDK.

## Conventions

- **Crates** (see [`docs/architecture.md`](./docs/architecture.md)): `simhub-app`
  (the exe), `simhub-games` (game list and feed threads), `simhub-telemetry`
  (readers), `simhub-model` (display contract and telemetry structs),
  `simhub-convert` (telemetry → `DisplayUpdate`), `simhub-core` (display selection
  and dashboard snapshots), `simhub-ui` (Slint window), `simhub-update` (self-update
  and single instance).
- **Prefer existing crates** over in-house code. Write our own only where no
  maintained crate covers the need or the crate fails a concrete requirement, and
  say why in the module docs ([ADR-0006](./docs/adr/0006-use-existing-crates-and-drop-raw-capture.md)).
- **Readers split decoding from acquisition**: decoding bytes into a telemetry
  struct is pure and unit-tested against the offsets the game publishes;
  acquisition stays thin.
- **Adding a game**: telemetry struct → reader → converter → feed and `GAMES` entry.
  The steps are in `docs/architecture.md`.
- **Dashboard text** follows the web dashboards the app replaced; the truck
  dashboard is Dutch.
- **Zero warnings**: keep `cargo test` and `cargo build` free of warnings.
- **Tests** are named as sentences describing the behaviour
  (`a_short_datagram_is_rejected`).
- **Command-line options** are defined with clap in `crates/simhub-app/src/main.rs`;
  unknown options are rejected.
- **Logging**: `log` macros everywhere; `simhub-app` sets up flexi_logger (console
  plus daily file in `log/`). `HADDYSIMHUB_DEBUG=1` enables debug level,
  `RUST_LOG` overrides it.
- **Version**: CD bakes the release tag into the exe through `HADDYSIMHUB_VERSION`;
  the self-updater compares it with the latest GitHub release.
- **Project skills**: shared skills live in `.agents/skills/` and are committed.
  Claude-specific personal settings and IDE scratch files under `.claude/` stay
  ignored.
- **Live checks**: questions only a running game can answer are tracked in
  [`docs/live-verification.md`](./docs/live-verification.md). Remove an entry once
  a session has settled it.

## Do Not

- Do not commit `target/` or `log/`.
- Do not enable simetry's default features: they run bindgen and need libclang.

## Git & Pull Requests

- `main` is a protected branch. Direct pushes are rejected — land changes via a pull request.
- Required status check: **Rust tests** (defined in `.github/workflows/ci.yml`).
- Admin enforcement is on (`enforce_admins`), so even `gh pr merge --admin` still requires green checks. Auto-merge is not enabled for this repository.
- No approving review is required (required review count is 0), but the checks above are mandatory.
- GitHub Actions are pinned to commit SHAs with a trailing `# vX.Y.Z` comment in the workflow files. When updating an action, change both the SHA and the version comment.

## Security & Deployment

- **Runtime**: a single Windows executable; no web server, no network listener
  except the UDP telemetry ports of DiRT Rally 2 (20777) and Forza Horizon 5 (5300)
  while that game is selected.
- **Network access**: only the startup update check against the GitHub releases API
  (skipped with `--no-update`).
- **Release**: CD builds `HaddySimHub.exe`, zips it as `haddy-simhub.zip` and creates
  the `v0.1.<run>` GitHub release that installed copies update from.
