# Haddy SimHub

## Purpose

HaddySimHub is a Windows app that detects which simulator you are running and
shows a live dashboard for it: race, rally, truck or flight. It is a single
executable written in Rust with a [Slint](https://slint.dev) UI.

## Supported games

| Game | Dashboard |
| --- | --- |
| iRacing | Race |
| Assetto Corsa | Race |
| Assetto Corsa Competizione | Race |
| Forza Horizon 5 | Race |
| DiRT Rally 2.0 | Rally |
| Assetto Corsa Rally | Rally |
| Euro Truck Simulator 2 | Truck |
| Microsoft Flight Simulator 2020 | Flight |

Some games need their telemetry switched on:

* **DiRT Rally 2.0** — set `udp enabled="true"`, port `20777` and `extradata="3"`
  in `hardware_settings_config.xml`.
* **Forza Horizon 5** — enable *Data Out* in the HUD settings, IP `127.0.0.1`,
  port `5300`.
* **Euro Truck Simulator 2** — install the
  [SCS telemetry plugin](https://github.com/RenCloud/scs-sdk-plugin).

### Microsoft Flight Simulator: SimConnect.dll

MSFS telemetry is read over SimConnect, whose client library ships with the
**MSFS SDK** rather than with the simulator itself. HaddySimHub loads
`SimConnect.dll` at runtime and looks for it in this order:

1.  next to `HaddySimHub.exe`;
2.  `%MSFS_SDK%\SimConnect SDK\lib\SimConnect.dll`;
3.  `C:\MSFS SDK\SimConnect SDK\lib\SimConnect.dll`.

If you have not installed the SDK (in MSFS: *Options → General → Developers →
Developer Mode*, then *Help → SDK Installer*), copy the 64-bit `SimConnect.dll`
next to the executable. Without it the flight dashboard stays inactive and the
log says where it looked; nothing else is affected. Releases do not include the
library: redistributing it is a licensing decision.

## Running

Start `HaddySimHub.exe`. It checks GitHub for a newer release, installs it and
restarts, then waits for a supported game.

```text
HaddySimHub [--no-update] [--demo <race|rally|truck|flight>]
```

* `--no-update` — skip the update check.
* `--demo <dashboard>` — show a dashboard with sample data, no game needed.

Logs go to the console and to a daily file in `log/` next to the executable.
Set `HADDYSIMHUB_DEBUG=1` for debug logging, or `RUST_LOG=trace` to also log every
display update.

## Development

- [Architecture overview](docs/architecture.md) — crates, runtime flow, and how
  to add a game.
- [Documentation index](docs/README.md) — architecture decisions and related
  guides.
- [AGENTS.md](AGENTS.md) — toolchain, build and test commands, conventions.

## Releases

- Changelog: [CHANGELOG.md](./CHANGELOG.md)
- Release tags follow the `v0.1.<build>` pattern (for example `v0.1.498`).
