# ADR-0006: Use existing crates, and drop raw telemetry capture

- **Status:** Accepted
- **Date:** 2026-10-04

## Context

ADR-0004 planned a staged migration in which every Rust reader would be
verified against raw bytes captured from the C# application (ADR-0005), with
the C# app kept as the default until each game passed a parity gate.

Two things changed while carrying it out. Maintained Rust crates already read
most of the sources — `simetry` for Assetto Corsa, Competizione and iRacing —
and they decode the bytes themselves, so a corpus of raw bytes captured from
the C# app would test our understanding of a layout that we no longer
implement. And the capture infrastructure was itself a substantial body of
code that existed only to support the migration.

## Decision

Prefer an existing crate over our own code for every component: `simetry` for
telemetry where it fits, `self_update` for updating, `sysinfo` for process
detection, `clap`, `flexi_logger`. Write our own only where no maintained crate
covers the need, or where the crate fails a concrete requirement — and record
that reason beside the code.

Drop raw capture (`--capture`, the corpus format and its validator) and the
parity gates built on it. Replace the C# application in a single change rather
than running both side by side.

The committed layout manifests stay: they are the published layout the byte
decoders are tested against, now as fixed files rather than generated from C#.

## Consequences

- Less code of our own, and the hard acquisition work (iRacing's ring buffers
  and session YAML, the Assetto pages) is maintained upstream.
- Our own readers remain for AC Rally, ETS2, DiRT Rally 2, Forza Horizon 5 and
  MSFS, each for a stated reason (see `docs/architecture.md`).
- simetry is lossier than our decoders in places: ACC's session lap total is
  not exposed.
- Without captured corpora, correctness against a running game is checked by
  playing it. The open questions are listed in `docs/live-verification.md`.
- There is no fallback to the C# app; reverting means reverting the change.
