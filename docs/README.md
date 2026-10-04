# HaddySimHub documentation

This directory documents the parts of HaddySimHub that are difficult to
discover from the folder names alone.

## Start here

- [Architecture overview](architecture.md) — repository map, runtime flow,
  crate boundaries, and how to add a game.
- [Checks that need a running game](live-verification.md) — the open questions
  only a play session can settle.
- [Existing Rust telemetry implementations](rust-telemetry-implementations.md)
  — surveys community readers and protocol libraries for the supported games,
  including coverage and license caveats.

## Architecture decisions

Architecture decisions record choices that should remain visible after the
implementation changes. They explain the context and consequences; they are
not a replacement for the architecture overview. ADRs 0001–0003 were written
for the C# application; their decisions carried over to the Rust one.

- [ADR-0001: Use a shared game-display pipeline](adr/0001-game-display-pipeline.md)
- [ADR-0002: Select one active display and separate detection from telemetry](adr/0002-single-active-display-and-telemetry-detection.md)
- [ADR-0003: Prefer explicit and boring application structure](adr/0003-prefer-explicit-and-boring-application-structure.md)
- [ADR-0004: Migrate the backend to Rust and the UI to Slint](adr/0004-migrate-backend-to-rust-and-ui-to-slint.md)
- [ADR-0005: Capture raw game-source telemetry](adr/0005-pin-telemetry-layout-with-a-generated-manifest.md) (superseded)
- [ADR-0006: Use existing crates, and drop raw telemetry capture](adr/0006-use-existing-crates-and-drop-raw-capture.md)

## Documentation rules

- Update `architecture.md` when a crate boundary, runtime flow, or extension
  point changes.
- Add an ADR when a change introduces or reverses a durable architectural
  decision, especially when alternatives and trade-offs matter.
- Keep implementation-specific notes in the module docs of the code they
  describe.
- Link new documents from this index so they can be found without knowing the
  repository layout.
