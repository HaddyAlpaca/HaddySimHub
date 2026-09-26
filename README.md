# Haddy SimHub

[![CI](https://github.com/HaddyAlpaca/HaddySimHub/actions/workflows/ci.yml/badge.svg)](https://github.com/HaddyAlpaca/HaddySimHub/actions/workflows/ci.yml)

[![CD](https://github.com/HaddyAlpaca/HaddySimHub/actions/workflows/cd.yml/badge.svg)](https://github.com/HaddyAlpaca/HaddySimHub/actions/workflows/cd.yml)

## Purpose

HaddySimHub is a tool that reads data from various racing simulators and provides a web-based interface to view real-time information and telemetry. It acts as a central hub for all your sim racing data.

## Supported Games

Currently, HaddySimHub supports the following games:

*   iRacing
*   Euro Truck Simulator 2
*   American Truck Simulator
*   DiRT Rally 2.0
*   Assetto Corsa
*   Assetto Corsa Competizione
*   Assetto Corsa Rally
*   Microsoft Flight Simulator 2020

### Microsoft Flight Simulator: SimConnect.dll

MSFS telemetry is read over SimConnect, whose client library ships with the
**MSFS SDK** rather than with the simulator itself. HaddySimHub looks for
`SimConnect.dll` in this order:

1.  next to `HaddySimHub.exe`;
2.  `%MSFS_SDK%\SimConnect SDK\lib\SimConnect.dll`;
3.  `C:\MSFS SDK\SimConnect SDK\lib\SimConnect.dll`.

If you have not installed the SDK (in MSFS: *Options → General → Developers →
Developer Mode*, then *Help → SDK Installer*), copy the 64-bit `SimConnect.dll`
next to the executable. Without it the flight dashboard simply stays inactive and
logs where it looked - nothing else is affected. A copy placed next to the
executable survives updates, because the updater only overwrites files that are in
the release archive.

Released archives do not contain `SimConnect.dll`: the release workflow builds on a
Linux runner with no MSFS SDK, and redistributing the library is a licensing
decision rather than something publishing should do by itself. To build a package
that includes it:

```bash
dotnet publish ./HaddySimHub -r win-x64 -p:IncludeSimConnectOnPublish=true
```

That reads the SDK location from `%MSFS_SDK%` and fails with a clear message if the
library is not there, so a package can never quietly ship without it. Override the
location with `-p:SimConnectDllPath=<path>`.

## Referenced/cloned repositories
* https://github.com/hfoxy/iRacingSDK.Net
* https://github.com/RenCloud/scs-sdk-plugin

## Console output

The backend writes colour-coded logs to the console and keeps a daily log file.
With `HADDYSIMHUB_DEBUG=1`, debug messages and per-frame telemetry logs are also
enabled.

Keyboard shortcuts:

* `Ctrl+C` — quit.

Environment variables:

* `HADDYSIMHUB_DEBUG=1` — enable debug-level logging and per-frame data logs.

## Releases

- Changelog: [CHANGELOG.md](./CHANGELOG.md)
- Release tags follow the `v0.1.<build>` pattern (for example `v0.1.498`).
