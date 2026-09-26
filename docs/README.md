# HaddySimHub documentation

This directory documents the parts of HaddySimHub that are difficult to
discover from the folder names alone.

## Start here

- [Architecture overview](architecture.md) — repository map, runtime flow,
  project boundaries, and extension points.
- [Game display pipeline](../HaddySimHub/Displays/README.md) — the detailed
  provider-to-frontend flow and instructions for adding a simulator.

## Architecture decisions

Architecture decisions record choices that should remain visible after the
implementation changes. They explain the context and consequences; they are
not a replacement for the architecture overview.

- [ADR-0001: Use a shared game-display pipeline](adr/0001-game-display-pipeline.md)
- [ADR-0002: Select one active display and separate detection from telemetry](adr/0002-single-active-display-and-telemetry-detection.md)
- [ADR-0003: Prefer explicit and boring application structure](adr/0003-prefer-explicit-and-boring-application-structure.md)

## Documentation rules

- Update `architecture.md` when a project boundary, runtime flow, or extension
  point changes.
- Add an ADR when a change introduces or reverses a durable architectural
  decision, especially when alternatives and trade-offs matter.
- Keep implementation-specific instructions beside the subsystem they
  describe, such as `HaddySimHub/Displays/README.md`.
- Link new documents from this index so they can be found without knowing the
  repository layout.
