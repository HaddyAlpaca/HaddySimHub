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

## Referenced/cloned repositories
* https://github.com/hfoxy/iRacingSDK.Net
* https://github.com/RenCloud/scs-sdk-plugin

## Console output

When the backend runs in an interactive terminal it shows a live, colour-coded
dashboard (similar in spirit to `btop`) with:

* a **Status** panel — web server port, the active game, and test-mode state;
* a **Games** panel — every supported game with an indicator showing which is
  currently detected;
* a colour-coded **Log** feed.

Keyboard shortcuts:

* `Ctrl+T` — cycle the test display (`race` → `rally` → `truck` → off);
* `Ctrl+C` — quit.

Environment variables:

* `HADDYSIMHUB_NO_DASHBOARD=1` — disable the dashboard and use plain colored log
  output. (This also happens automatically when output is redirected, e.g. in
  CI or when piping to a file.)
* `HADDYSIMHUB_DEBUG=1` — enable debug-level logging and per-frame data logs.

## Releases

- Changelog: [CHANGELOG.md](./CHANGELOG.md)
- Release tags follow the `v0.1.<build>` pattern (for example `v0.1.498`).
