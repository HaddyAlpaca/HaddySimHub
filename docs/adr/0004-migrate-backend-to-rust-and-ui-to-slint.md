# ADR-0004: Migrate the backend to Rust and the UI to Slint

- **Status:** Accepted; completed 2026-10-04, with the deviations recorded in [ADR-0006](0006-use-existing-crates-and-drop-raw-capture.md)
- **Date:** 2026-09-27

## Context

The backend is .NET 10 with vendored C# SDK integrations (`SCSSdkClient` and
`iRacingSDK.Net`). The frontend is Lit, served by Kestrel and updated over SSE.
The target is a Rust binary with a Slint UI, which removes the HTTP server, the
browser, and the .NET runtime from the picture.

Slint has first-class Rust bindings and no official C# binding, so the backend
has to be Rust before the UI can move. Starting with the UI would mean binding
Slint to the C# application, which is the riskier order.

Rewriting both halves at once would leave no working application at any
intermediate point. The dominant correctness risk is telemetry that is
structurally valid but numerically wrong: a shifted field or an off-by-one
offset produces plausible dashboards rather than visible failures.

## Decision

Migrate in eight steps, backend before migrating the production UI. First create
a Rust/Slint prototype beside the running app using synthetic data; this validates
the cross-platform UI toolchain without claiming telemetry parity. Capture and
pin the telemetry ground truth before porting converters or providers. Keep the
existing app as the default until the Rust implementation is proven; delete the
web stack only at the end.

The `DisplayUpdate` contract from ADR-0001 is frozen for the duration and is the
seam both migrations work against. The Rust workspace and a Slint prototype may
be built earlier with explicitly synthetic data. Before any Rust game converter
or provider is ported, a generated layout manifest and sampled frame corpus are
committed as ground truth, as described in ADR-0005.

## Consequences

- The existing application stays runnable and remains the default while the
  prototype is developed alongside it.
- Synthetic demo data proves the Linux UI path only; it is not evidence of
  telemetry or converter parity.
- The order is forced by Slint's binding availability, not chosen for
  convenience.
- Reversibility decreases with each step. After step 4 a revert is a flag, after
  step 6 it is restoring the Lit bundle, and step 7 is one-way, so it is last.
- Step 4 is a legitimate stopping point: Rust backend, unchanged UI, no .NET
  runtime.
- The existing converter tests stay as the executable specification the Rust
  converters have to satisfy.
- Slint has no charting primitive, so the race telemetry trace needs an explicit
  decision between its `Path` and `SharedPixelBuffer` approaches.
- Deleting the HTTP server removes the `--e2e` route that the Playwright suite
  uses, so that suite needs a new bridge before step 7.
- `SCSSdkClient` is large, so porting it is mechanical rather than quick.
