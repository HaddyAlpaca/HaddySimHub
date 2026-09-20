# ADR-0001: Use a shared game-display pipeline

- **Status:** Accepted
- **Date:** 2026-09-20

## Context

HaddySimHub integrates multiple simulators. Their telemetry sources differ:
shared memory, UDP, vendor SDKs, and native SimConnect. The frontend should not
need to understand those source-specific formats.

## Decision

Each simulator follows the same pipeline:

```text
game data provider → data converter → display lifecycle → SSE → frontend
```

The provider owns acquisition of game-specific telemetry. The converter maps
that telemetry to the shared `DisplayUpdate` model. The display lifecycle
handles start/stop, buffering, and sending updates. The frontend receives only
the shared model over the SSE stream.

Game-specific acquisition and conversion code remains inside that game's
directory under `HaddySimHub/Displays/`.

## Consequences

- Adding a game has a predictable set of responsibilities.
- Game SDK and memory-layout details stay out of the frontend and shared models.
- Shared lifecycle behavior is implemented once and tested once.
- Some small game integrations still require provider and converter types even
  when their code is simple; that duplication is intentional and keeps the
  boundaries explicit.
