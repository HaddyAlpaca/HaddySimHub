# ADR-0003: Prefer explicit and boring application structure

- **Status:** Accepted
- **Date:** 2026-09-20

## Context

This is a local desktop application maintained by a small team. Its complexity
comes primarily from simulator protocols and native integrations, not from a
large number of business workflows. Generic factories, automatic discovery,
and interactive infrastructure add maintenance cost when there is only one
implementation or one deployment shape.

## Decision

Prefer concrete classes, explicit dependency-registration calls, and direct
console logging. `Program` is limited to startup orchestration, while concrete
startup responsibilities live in small infrastructure helpers. New interfaces,
factories, base classes, and reflection-based registration require a concrete
second implementation or a clear external seam.

The application does not provide a live console TUI. Debugging uses coloured
console logs, daily log files, optional debug logging, and optional per-frame
telemetry logs.

## Consequences

- Code is easier to locate and follow with ordinary search and debugging tools.
- Startup and console behavior have fewer lifecycle and rendering dependencies.
- The project avoids a plugin framework and automatic assembly scanning.
- Some explicit registration and repeated game-specific code remains; this is
  preferred over hiding wiring behind conventions or reflection.
- A future need for multiple implementations can justify introducing a focused
  abstraction at that time.
