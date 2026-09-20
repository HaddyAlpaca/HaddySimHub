# ADR-0002: Select one active display and separate detection from telemetry

- **Status:** Accepted
- **Date:** 2026-09-20

## Context

All game integrations publish to one SSE stream and the frontend expects one
coherent display type at a time. Multiple games may be open simultaneously.
Some simulators also expose shared-memory pages before telemetry is actually
available, and several games can share the same memory names.

## Decision

`DisplaysRunner` selects one active display at a time. The currently selected
display keeps its turn while its game remains active; another display is
considered only after it stops being active.

Process detection answers whether a game is running. The provider connection
answers whether telemetry is flowing. These are separate states and must not be
collapsed into a single “connected” check. Process names are used where shared
memory names cannot distinguish simulator variants.

## Consequences

- Frames from different simulators cannot interleave on the same stream.
- Operators can distinguish “game process detected” from “telemetry received”.
- Shared-memory providers need retry and diagnostic logging for the interval
  between process startup and telemetry availability.
- The runner deliberately does not support simultaneous displays in one stream;
  that would require a different frontend contract.
