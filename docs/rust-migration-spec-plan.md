# Raw-capture foundation and complete Rust migration

## Specification

### Overview

The goal is to replace the simulator-facing C# backend and its C#-only runtime
dependencies with a Rust application, while preserving the currently supported
games, display behavior, and operational features. Before replacing each game
reader, HaddySimHub must be able to capture and replay the exact unmodified
bytes visible at that reader's source boundary; captures are evidence, not
normalized telemetry or converter output.

“100% Rust” means no HaddySimHub-owned C# application, telemetry, conversion,
web-host, or updater code is required to build or run the delivered
application. It does not mean Rust can guarantee that a simulator emitted
every update or that Windows, a game, or a vendor-provided native interface
never dropped data.

### Requirements

#### REQ-1: Inventory and capture every supported source

For each currently registered simulator, the capture system records data at
the earliest stable source boundary available to the application, before any
C# or Rust game-specific decoder, DTO, converter, or display mapping.

**Acceptance criteria:**
- Given any currently registered game, when its source adapter is enabled,
  then its source transport, payload boundary, page/datagram shape, and
  continuity signals are listed in a versioned source inventory.
- Given a capture-enabled game source, when bytes arrive, then the saved
  payload decodes byte-for-byte to those source bytes without SDK objects,
  normalized telemetry, or `DisplayUpdate` fields.
- Given a game for which only an API-provided data block is available, when
  the source inventory is reviewed, then that boundary is explicitly marked
  as API payload rather than raw network/shared-memory transport.

#### REQ-2: Preserve framing and provenance

Each raw record retains all framing needed for deterministic replay without
interpreting or rewriting its payload.

**Acceptance criteria:**
- Given a UDP source, when a datagram is captured, then it is stored as one
  exact payload with datagram boundary, source/destination metadata where
  available, timestamp, and monotonic capture sequence.
- Given a shared-memory source, when a snapshot is captured, then every
  mapped page/range is stored byte-for-byte with source name, byte length,
  timestamp, sequence, and integrity checksum.
- Given any record, when its source cannot provide a protocol version or
  timestamp, then that field is marked unavailable rather than inferred from
  decoded values.
- Given any record, when it is inspected, then it contains no game DTO,
  normalized telemetry, or display/converter output.

#### REQ-3: Make completeness claims fail-visible and bounded

Capture distinguishes persisted accepted input from source-level continuity;
it never reports verified completeness when continuity is unknown or a loss
condition has been observed.

**Acceptance criteria:**
- Given queue overflow, disk/write failure, invalid framing, or detected
  source discontinuity, when capture finalizes, then its status is
  `incomplete` with a machine-readable reason and the affected game/source.
- Given an unclean shutdown, when a corpus is inspected, then its run remains
  `capturing`/unfinalized and cannot be accepted as complete.
- Given successful shutdown, when every accepted record is durably flushed,
  then the run reports writer completion and separately reports whether
  source continuity is verified, unverified, or failed for each source.
- Given a source that overwrites data without a recoverable history, when
  updates may have been missed, then the capture explicitly reports that
  limitation; it does not claim lossless capture of all game-produced updates.

#### REQ-4: Replay the corpus independently in Rust

The captured raw corpus is a stable input to Rust source decoders and
converters without requiring a running simulator or C# SDK.

**Acceptance criteria:**
- Given a finalized corpus and its format version, when the Rust replay tool
  reads it, then it validates sequence, framing, byte lengths, checksums, and
  source-specific structural invariants before decoding.
- Given valid captured frames, when Rust decodes them, then the result can be
  compared to the existing C# behavior in a separate test/report path without
  adding C# output to the raw capture files.
- Given malformed, truncated, or modified payload data, when Rust validates
  it, then replay fails with the file, record, and source-specific reason.

#### REQ-5: Port all supported game behavior

The Rust application replaces every registered C# game acquisition and
conversion path and preserves the user-visible outputs.

**Acceptance criteria:**
- Given each currently registered game, when its Rust implementation is
  exercised against a representative approved raw corpus, then it produces
  the same public display model/values as the legacy implementation for the
  supported fields and defined edge cases.
- Given an absent game, a disconnect, reconnect, malformed source input, or
  simultaneous game processes, when the Rust app runs, then source lifecycle
  and single-active-display behavior match the existing contract.
- Given the full application is built and published, then no HaddySimHub C#
  runtime project or .NET runtime is needed to run it.

#### REQ-6: Retire the legacy runtime only after parity gates pass

The existing C# application remains available during migration and is removed
only after all replacement gates pass.

**Acceptance criteria:**
- Given any supported game or application workflow, when migration is
  proposed for completion, then corpus validation, Rust replay/parity tests,
  platform builds, and user-facing feature checks pass for that workflow.
- Given the final migration, when source and build manifests are inspected,
  then C# app, SDK, and updater projects are no longer required or registered
  in the delivered build.
- Given a Rust release candidate, when CI runs, then Rust formatting,
  workspace tests, Windows-target build/package checks, and frontend/UI
  interaction tests pass.

### Constraints

- Raw captures must not contain C# DTOs, Rust DTOs, converter output, or
  `DisplayUpdate` goldens.
- The Rust migration must preserve the current supported game set and
  user-visible dashboards, lifecycle, and update/install behavior unless a
  separately accepted change says otherwise.
- The application targets Windows for simulator integration; the Rust
  prototype may continue to build/test on Linux where dependencies allow.
- Capture must use bounded memory and non-blocking source callbacks; overload
  must invalidate the capture instead of silently dropping accepted records.
- “Complete” is scoped to records observed and durably saved at a documented
  source boundary. It cannot certify that the simulator, OS, transport, or
  shared-memory producer did not lose/overwrite updates before observation.
- Corpus format changes must be versioned and old fixtures must remain
  readable or have an explicit migration tool.

### Non-Requirements

- Capturing Ethernet/IP packets with a privileged packet-sniffing driver is not
  required; the target is the exact application-visible UDP datagram or mapped
  shared-memory/API payload.
- Raw captures will not contain DTO snapshots, `DisplayUpdate` values, or
  expected converter results. Any legacy-vs-Rust comparison is performed by a
  separate replay test/report.
- The capture system does not recover samples that a game overwrote before
  HaddySimHub could observe them.
- This migration does not add simulator support beyond the currently
  registered set.

### Assumptions

- The target set is the eight games registered in
  `HaddySimHub/Displays/DisplayDefinitions.cs`: Dirt Rally 2, ETS2, iRacing,
  Assetto Corsa, ACC, Assetto Corsa Rally, Microsoft Flight Simulator 2020,
  and Forza Horizon 5.
- The goal includes replacing the C# runtime host/updater and current web
  frontend with the already-decided Rust/Slint application, not merely
  translating converters.
- Existing C# tests and behavior are temporary migration oracles, but only for
  the four sources validated against a running game: ACC, iRacing, ETS2 and
  Dirt Rally 2. Assetto Corsa, AC Rally, Forza Horizon 5 and MSFS 2020 have
  never been run against the game, so for those a passing parity test shows
  only that two implementations agree, not that either is right. The final
  shipped test/runtime path is Rust.
- For MSFS, the first feasible raw boundary may be the unmodified SimConnect
  response data block returned by the native API, rather than the simulator's
  underlying IPC bytes. This distinction must be recorded and validated.

### Open Questions

- No blocking product choice is needed to begin source inventory. If any
  source cannot expose stable replayable bytes without a vendor SDK, document
  the boundary and evidence first; then decide whether an API-level byte
  payload is acceptable or whether that title requires a different capture
  method.

### Initial source inventory

| Game | Current C# ingress | Capture status | Main source-level risk |
| --- | --- | --- | --- |
| Dirt Rally 2 | UDP on port 20777, then immediate `Packet` marshal and an unbounded `ConcurrentQueue<Packet>` | Captured at `UdpClient.EndReceive` before validation/marshal | Existing receiver queue and OS drops are not measured; no sequence counter checked |
| ETS2 | `Local\SCSTelemetry` shared-memory map through `SCSSdkClient` | Raw map captured before `SCSSdkConvert` | No source sequence-gap/consistency check; 10 ms polling can miss overwritten updates |
| iRacing | `Local\IRSDKMemMapFileName` + data-valid event, via `iRacingSDK.Net` | Raw full map captured before `DataFeed` | Header stability does not prove ring-buffer/session payload atomicity |
| Assetto Corsa | Physics, graphics, static shared-memory pages | All mapped pages captured before struct interpretation | Sampled on a timer; producer can update between pages; static page must be present |
| ACC | Physics and graphics shared-memory pages | All mapped pages captured before struct interpretation | Polling can miss overwritten updates; independent pages may update between reads |
| Assetto Corsa Rally | Physics, graphics, static shared-memory pages | All mapped pages captured before struct interpretation | Sampled on a timer; producer can update between pages; static page must be present |
| Microsoft Flight Simulator 2020 | SimConnect native API dispatch response block | Exact telemetry dispatch copied before `MsfsTelemetry` decode | API payload, not raw underlying SimConnect transport; definition ordering is required |
| Forza Horizon 5 | UDP on port 5300, then packet bytes marshalled into `ForzaTelemetry` | Captured at `UdpClient.ReceiveAsync` before validation/marshal | OS/socket loss unmeasured; short packets are captured but still rejected by existing parser |

Four of the eight implementations have been validated against a running game
(ACC, iRacing, ETS2, Dirt Rally 2); the other four (Assetto Corsa, AC Rally,
Forza Horizon 5, MSFS 2020) have not. Capture priority follows that split
rather than port order: for an unvalidated source the first capture is also the
first real test of the existing implementation, so it must happen before a Rust
reader is built on top of it. AC Rally is the weakest case, with an inferred
layout, no validation and no published specification.

Readers are a mix of dependencies and in-house implementations, decided per
source on protocol churn and dependency health. ACC takes
`acc_shared_memory_rs`; Dirt Rally 2 and Assetto Corsa have viable permissive
candidates for their frozen formats; ETS2, iRacing, Forza Horizon 5, MSFS 2020
and AC Rally are written here, in three cases because every candidate is
unlicensed or noncommercial rather than because none exists. The REQ-3
continuity layer is ours regardless of who decodes the bytes. See
[`rust-telemetry-implementations.md`](rust-telemetry-implementations.md) for
the measured project health behind each decision.

## Execution plan

### Scope

**Goal:** Establish verified raw-input and Rust replay coverage for all eight
currently supported simulators, then replace the complete HaddySimHub-owned
C# application/runtime with the Rust/Slint application without losing
supported behavior.

**In scope:**
- Source-protocol inventory and feasibility tests for all eight integrations.
- Versioned raw-only capture records for UDP datagrams, shared-memory ranges,
  and the documented MSFS native API payload boundary.
- Per-source continuity/completeness evidence, bounded asynchronous durable
  capture, corpus validation, and simulator-free Rust replay tests.
- Rust ports of all source readers, converters, display selection/lifecycle,
  application hosting, dashboards, and updater/install workflow.
- Parallel operation and parity gates before deleting C# projects.

**Out of scope:**
- Claiming physically lossless observation of every game-produced update
  where the game publishes only an overwriteable latest-value mapping.
- Network-interface packet sniffing, packet injection, or modifying games.
- New games or dashboard features not present in the current application.
- Storing transformed telemetry or expected outputs in the raw capture corpus.

### Approach

Keep the current C# application operational as a temporary oracle while
building a separate Rust implementation. First resolve the highest-risk
question—what exact source bytes each integration can expose, and which
protocols can prove continuity—then make raw capture/replay a versioned
contract. Port each game only after a validated corpus exists, compare C# and
Rust behavior in separate tests, and remove C# only after all game, UI,
deployment, and updater gates pass.

### Steps

1. **Prove source boundaries and loss observability**
   - Files: `HaddySimHub/Displays/*/*GameDataProvider.cs`,
     `HaddySimHub/Displays/*/*SharedMemoryReader.cs`,
     `HaddySimHub/Displays/Msfs/SimConnectClient.cs`,
     `SCSSdkClient/`, `iRacingSDK.Net/`,
     `HaddySimHub/Displays/README.md`.
   - Change: For each source, document exact bytes available before decode,
     frame/page size, update trigger/rate, sequence/generation fields, and
     whether overwritten or dropped records can be detected. Capture hooks
     now exist for all eight registered games, but live source behavior and
     continuity remain to be proven. Add small
     source-boundary probes/tests before changing any decoder. For MSFS,
     establish whether the raw SimConnect response block is sufficient to
     rebuild the current decoder independently.
   - Why: If any source does not expose a replayable byte boundary, later
     capture and conversion parity assumptions are invalid.
   - Verify: Source inventory covers all eight registrations; tests prove raw
     byte capture hooks precede all marshaling/conversion; each source receives
     a `verified`, `detectable-loss`, or `unobservable-loss` classification.

2. **Freeze the raw-corpus contract**
   - Files: `HaddySimHub/Capture/TelemetryCapture.cs`,
     `docs/telemetry-corpus.md`, `fixtures/telemetry/`.
   - Change: Define a format version with per-run identity, source identity,
     per-source sequence, monotonic and wall-clock capture times, exact page or
     datagram boundaries, length, SHA-256, and explicit source-continuity
     evidence. Keep records raw-only. Provide a validator that rejects
     sequence gaps, invalid checksums, malformed records, missing pages, and
     unfinished/incomplete runs.
   - Why: A corpus is a long-lived Rust migration input and must not rely on
     undocumented C# internals.
   - Verify: Round-trip property tests for arbitrary bytes; corruption,
     truncation, sequence-gap, and incomplete-run tests; schema version tests.

3. **Capture UDP datagrams at socket ingress**
   - Files: `HaddySimHub/Displays/Dirt2/Dirt2GameDataProvider.cs`,
     `HaddySimHub/Displays/Forza/ForzaGameDataProvider.cs`,
     capture tests and source documentation.
   - Change: Enqueue exact `UdpReceiveResult` payloads and datagram metadata
     before size checks or `Marshal.PtrToStructure`; use bounded packet queues
     with explicit overflow and shutdown-drain status. Keep the existing
     parser separate and do not save its DTO.
   - Verify: Inject boundary-sized and malformed datagrams; assert exact
     payload round-trip and preservation of datagram boundaries; force queue
     overflow and confirm run status is incomplete.

4. **Complete shared-memory capture**
   - Files: `HaddySimHub/Displays/SharedMemoryPage.cs`,
     `HaddySimHub/Displays/AC/ACSharedMemoryReader.cs`,
     `HaddySimHub/Displays/ACC/ACCSharedMemoryReader.cs`,
     `HaddySimHub/Displays/ACRally/ACRallySharedMemoryReader.cs`,
     `HaddySimHub/Displays/ETS/EtsGameDataProvider.cs`,
     `iRacingSDK.Net/iRacingMemory.cs` and source hooks.
   - Change: Capture every required page, including each Assetto Corsa family
     static page and protocol/version metadata as raw fields; ensure snapshots
     are source-consistent where possible. Use producer packet/tick markers
     for gaps, and mark unobservable continuity honestly. For iRacing, validate
     selected ring-buffer metadata and stable copied ranges rather than treating
     equal full-map headers as proof that all data was atomic.
   - Verify: Synthetic concurrent-writer tests for changing pages/ring buffers;
     page-version/layout tests; disconnection/reconnection and counter-wrap
     tests; source status is never `verified` without defined evidence.

5. **Capture MSFS at the documented pre-decode boundary**
   - Files: `HaddySimHub/Displays/Msfs/SimConnectClient.cs`,
     `HaddySimHub/Displays/Msfs/SimVarDefinitions.cs`,
     `HaddySimHub.Tests/SimVarDefinitionsTests.cs`.
   - Change: Capture the exact SimConnect object-data response bytes and
     definition metadata before `Marshal.PtrToStructure<MsfsTelemetry>`. Do not
     serialize `MsfsTelemetry`. Record this as an API-payload source, not as
     raw underlying transport.
   - Verify: Fake native-dispatch payload tests confirm byte-exact capture,
     response framing, definition ordering, and independent Rust decoding. If
     the data block omits data needed by Rust, block MSFS C# retirement until
     an adequate byte source is found.

6. **Build Rust corpus validators and deterministic replay harness**
   - Files: `rust/crates/simhub-acc-parity/` (generalize/rename as appropriate),
     `rust/crates/` new per-source decoder crates, `fixtures/telemetry/`.
   - Change: Replace the ACC-only checker with a generic corpus validator and
     source-specific Rust replay modules; keep game decoders separate from
     capture framing. Create approved representative corpora on Windows for
     every game, with no derived DTO data embedded.
   - Verify: `cargo test --workspace --locked`; every fixture validates and
     decodes offline; intentionally corrupted fixture variants fail with
     record-level diagnostics.

7. **Port game readers and converters, one source at a time**
   - Files: new `rust/crates/simhub-telemetry-*`, `rust/crates/simhub-core/`,
     existing C# converter tests as temporary oracle.
   - Change: Port the sources with a working oracle first — ACC, then iRacing,
     ETS2 and Dirt Rally 2 — so early ports are checkable against known-good
     behavior. Then Assetto Corsa, Forza Horizon 5 and MSFS 2020, each verified
     against its published format rather than against our untested C#. Port
     AC Rally last: inferred layout, no validation, no specification. Implement
     each reader/converter against its raw corpus. Preserve units,
     null/default semantics, enum mapping, packet version behavior, and
     converter state. Run legacy-vs-Rust comparisons in tests/reports separate
     from the raw capture files.
   - Verify: For every game, replay each corpus through both implementations
     and compare the supported display outputs plus boundary/error cases;
     run existing `.NET` tests until the corresponding Rust parity suite is
     complete.

8. **Complete Rust application and feature parity**
   - Files: `rust/crates/simhub-app/`, `simhub-ui/`, `simhub-core/`,
     launch/update packaging and CI workflows.
   - Change: Replace C# display discovery/selection, lifecycle/reconnect,
     logging/configuration, web/SSE runtime, frontend dashboards, and
     `HaddySimHubUpdater` with the Rust/Slint runtime and Rust release/update
     path. Keep the public dashboard behavior and accessibility/interaction
     expectations.
   - Verify: Windows simulator-free integration tests for app lifecycle and
     updater; UI screenshot/interaction parity for every display type; Windows
     packaged build starts and runs without .NET.

9. **Run a parallel soak and retire C#**
   - Files: `.github/workflows/ci.yml`, solution/project files, C# projects,
     `docs/architecture.md`, `docs/README.md`.
   - Change: Run legacy and Rust readers in parallel against the same live
     game/source where feasible, compare source sequence and outputs, resolve
     all mismatches, then make Rust the only runtime. Delete C# projects and
     obsolete SDK integrations only after gates pass.
   - Verify: Per-game soak reports show zero unexplained observed-input gaps
     and parity mismatches; Rust CI/build/package tests pass; `dotnet` is no
     longer required by documented build/run/release commands.

### Verification

- [ ] The source inventory covers all eight current registrations and labels
  exact raw transport versus API-payload boundaries.
- [ ] Capture tests prove exact byte/datagram/page round-trip, checksums,
  sequence continuity, disk-failure/overflow behavior, and clean shutdown.
- [ ] Every corpus validator rejects modified, truncated, missing, or
  incomplete input and never upgrades unverified sources to verified.
- [ ] Rust can replay each committed raw corpus offline without the simulator,
  SDK DTOs, or .NET runtime.
- [ ] Separate tests compare legacy C# and Rust display behavior for all
  supported games before C# retirement. For ACC, iRacing, ETS2 and Dirt Rally 2
  that comparison is evidence of correctness; for Assetto Corsa, AC Rally,
  Forza Horizon 5 and MSFS 2020 it only shows the two agree, so each of those
  also needs its decoded values checked against the published format and
  against a live session.
- [ ] The final Windows package preserves current game, UI, updater, and
  lifecycle behavior and runs without HaddySimHub-owned C# code or .NET.
- [ ] `dotnet test HaddySimHub.sln --no-restore` passes during the transition;
  final gate is `cargo test --workspace --locked`, Rust formatting/lints,
  Windows package tests, and UI acceptance tests.

### Risks

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| Overwrite-only shared memory loses producer updates before polling can read them | Likely | Blocks literal zero-loss claims | Establish source semantics/rates first; capture high-rate where useful; expose sequence gaps; scope completeness to observed snapshots |
| iRacing map changes while buffers/session data are copied | Likely | Produces mixed, misleading replay input | Validate header, selected buffer tick, session-info generation/ranges; retry and fail capture if consistency cannot be established |
| UDP packets are dropped by OS/socket/queue before or during capture | Possible | Gaps in Rust rebuild evidence | Capture immediately on receive, bound queues, count socket/queue drops, use game sequence fields when available, report unobservable network loss |
| SimConnect only exposes API-shaped data, not raw transport | Likely | Could prevent strict raw-transport requirement | Capture exact native API response block; document boundary; prove Rust can decode it before C# retirement |
| Real game corpora are unavailable on Linux CI | Likely | CI cannot exercise source protocols | Commit sanitized/raw representative fixtures with provenance; keep live Windows capture/soak as release gate |
| An unvalidated C# reader is treated as an oracle and its layout error is inherited by the Rust port | Likely | Silent wrong dashboards that no gate catches | Capture the four unvalidated sources early; verify decoded values against the published format and a live session, not against our own implementation |
| Full UI/runtime/updater migration is much larger than telemetry conversion | Likely | Delays full C# removal | Keep stages independently shippable; leave legacy app default until end-to-end gates pass |

### Assumptions

- Existing `ADR-0004` remains the migration order: establish Rust UI/runtime
  foundation and telemetry ground truth, port backend/game behavior, migrate
  production UI, then remove .NET last.
- All eight games in the current C# composition root remain supported.
- “100% Rust” excludes OS/vendor binaries and simulator software, but excludes
  HaddySimHub-owned C# projects and .NET runtime requirements.
