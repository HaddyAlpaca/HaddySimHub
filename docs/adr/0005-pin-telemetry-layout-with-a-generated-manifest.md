# ADR-0005: Capture raw game-source telemetry

- **Status:** Accepted
- **Date:** 2026-09-27

## Context

The migration to Rust must replace the telemetry readers as well as the
converters. In particular, the ETS2 and iRacing SDK projects should become
unnecessary after their source protocols are implemented in Rust. Capturing
objects emitted by those SDKs would preserve their decoded API as a dependency
and would not provide evidence for replacing their readers.

The source data arrives through game-published shared memory. A reader can be
compiled and still interpret a field at the wrong offset, so the original
source bytes and their layout need to be reviewable and replayable.

## Decision

The capture path records only bytes copied from each game's original source
transport, before any C# SDK decoder or converter runs. It may add framing
metadata such as the source name, page name, schema version, and sequence
number, but it must not store SDK objects, normalized telemetry, or
`DisplayUpdate` results.

For ACC this means capturing the physics and graphics shared-memory pages.
For ETS2 it means capturing the SCS shared-memory map before
`SCSSdkConvert`; for iRacing it means capturing the iRacing shared-memory map
before `DataFeed` parses it. Other sources are added at their transport boundary
when their original bytes can be captured.

Generated layout manifests remain useful for C# marshalled structures and are
checked against committed files by tests. Raw capture and layout manifests are
evidence for Rust reader implementations; they do not claim that C# and Rust
outputs match. Any output-parity check is a separate verification concern and
must not enrich the raw capture.

## Consequences

- Captures provide source bytes that can be replayed while building the Rust
  reader, without requiring a running simulator on the development machine.
- The C# ETS2 and iRacing SDK projects can be removed after their Rust readers
  have been implemented and verified against these captures.
- Captures can be large, especially the full iRacing shared-memory map; the
  opt-in writer applies frame and byte caps and reports incomplete files.
- A source without a raw-transport hook is not considered captured. SDK-decoded
  objects are not accepted as a fallback.
- A raw capture by itself does not prove Rust/C# converter parity; that requires
  a separate expected-result or differential test.
