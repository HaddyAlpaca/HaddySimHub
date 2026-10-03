# Raw telemetry capture and layout manifests

The target is a raw-only, replayable source corpus for replacing all supported
C# game readers and converters with Rust. The current implementation captures
only ACC, ETS2, and iRacing; it is not yet sufficient to replace all C# game
integrations or to claim complete source coverage. See the
[capture specification and migration plan](rust-migration-spec-plan.md) for
the full game inventory and acceptance gates.

Capture records bytes from the game's shared-memory transport before a C#
decoder or converter interprets them. It does not serialize SDK objects,
normalized telemetry, or `DisplayUpdate` values.

Capture is opt-in with `--capture <directory>`. Without that option, no source
bytes are written.

## Current source coverage

| Game | Captured source | Capture point |
| --- | --- | --- |
| Dirt Rally 2 | Each UDP datagram received on port `20777` | `Dirt2GameDataProvider`, immediately after socket receive and before size validation/marshaling |
| ETS2 | `Local\SCSTelemetry` shared-memory map (32 KiB) | `SCSSdkClient.SharedMemory`, after copying the source map and before `SCSSdkConvert` |
| iRacing | `Local\IRSDKMemMapFileName` shared-memory map | `iRacingSDK.Net`, after the data-valid event and before `DataFeed` parses the map |
| Assetto Corsa | `Local\acpmf_physics`, `Local\acpmf_graphics`, `Local\acpmf_static` | `ACSharedMemoryReader`, from stable copies of each full mapped page before struct interpretation |
| ACC | `Local\acpmf_physics` and `Local\acpmf_graphics` | `ACCSharedMemoryReader`, from stable copies of each full mapped page before struct interpretation |
| Assetto Corsa Rally | `Local\acpmf_physics`, `Local\acpmf_graphics`, `Local\acpmf_static` | `ACRallySharedMemoryReader`, from stable copies of each full mapped page before struct interpretation |
| Microsoft Flight Simulator 2020 | Full native `SIMCONNECT_RECV_SIMOBJECT_DATA` dispatch block for the telemetry request | `SimConnectClient`, copied byte-for-byte before `Marshal.PtrToStructure<MsfsTelemetry>`; this is the SimConnect API payload, not the underlying SimConnect wire/shared-memory transport |
| Forza Horizon 5 | Each UDP datagram received on port `5300` | `ForzaGameDataProvider`, immediately after socket receive and before packet-size validation/marshaling |

ETS2 and iRacing capture stores the SDK input bytes, not the SDK's
`SCSTelemetry` or `IDataSample` object graphs. Keeping these source captures
allows a Rust reader to be developed without retaining those C# SDK projects.
All eight currently registered game providers now have an opt-in source-boundary
capture hook. For UDP games this records application-visible datagrams, not
Ethernet frames; for MSFS the available boundary is the native API dispatch
payload. No capture hook records data from an already-decoded SDK DTO.

The iRacing shared-memory snapshot is the complete mapped view for each
data-valid event. It may produce a large file. Raw frames are copied into a
bounded in-memory queue with a 64 MiB byte budget; JSON encoding and durable
file writes run on a background writer. If the queue fills, a frame exceeds
that budget, disk writing fails, a prior JSONL file has an invalid sequence,
or iRacing reports a raw shared-memory tick gap, capture stops accepting frames
and its status is `incomplete`. There are no frame-count or file-size caps that
silently truncate a run.

Each run has a `capture-status-<id>.json` file. Only a clean shutdown after all
queued frames have been flushed may mark a run `complete`; a run that is still
`capturing` was not finalized and must be treated as incomplete. This status
certifies that the writer persisted all frames it accepted, not that every
game-source update was captured or that shared-memory data was atomic. iRacing
checks the mapped header before and after each full-map copy, retries up to
three times when it changes, and marks the run incomplete when no stable header
is observed; tick gaps also invalidate the run. Header stability cannot prove
that a telemetry buffer was not modified while its tick marker stayed
unchanged. For ACC, AC, and AC Rally, each required page must be byte-identical
across adjacent reads (retried three times); ACC additionally checks matching
physics and graphics packet IDs and forward packet gaps. Dirt Rally 2, Forza,
and ETS2 do not currently have source sequence-gap accounting. These capture
hooks prove which bytes the app received, not that the game/OS delivered every
produced update. Live captures and real-game validation are still required.
Use a new, empty output directory for each capture session; the JSONL files
append if they already exist, so all corresponding status files must be checked
when intentionally combining runs.

## Capture format

Files are append-only JSON Lines named `<game>.jsonl`. Each line contains a
per-game sequence, capture ID and timestamp, raw source metadata/bytes, and
per-page byte lengths and SHA-256 checksums. Binary source data is
Base64-encoded for transport in JSON; decoding that field restores the exact
captured byte sequence. The checksum allows copied corpora to be checked for
truncation or corruption independently of the writer status.

ACC frames contain two raw pages:

```json
{"seq":0,"captureId":"<id>","capturedAtUtc":"<UTC timestamp>","raw":{"schemaVersion":1,"transport":"shared-memory","sourceName":"Local\\acpmf_physics + Local\\acpmf_graphics","pages":{"physics":"<base64 bytes>","graphics":"<base64 bytes>"}},"integrity":{"pages":{"physics":{"byteLength":800,"sha256":"<hex>"},"graphics":{"byteLength":1588,"sha256":"<hex>"}}}}
```

ETS2 and iRacing frames contain the raw shared-memory map as a single page:

```json
{"seq":0,"captureId":"<id>","capturedAtUtc":"<UTC timestamp>","raw":{"schemaVersion":1,"transport":"shared-memory","sourceName":"Local\\SCSTelemetry","pages":{"data":"<base64 bytes>"}},"integrity":{"pages":{"data":{"byteLength":32768,"sha256":"<hex>"}}}}
```

There are intentionally no C# telemetry or display-update goldens in this
format. The captured bytes are the input for an independent reader/converter
implementation. Comparing Rust with C# output, if needed, is a separate
verification step and must not change or enrich the raw capture.

## Capture on the game PC

On the Windows PC with the simulator installed, open PowerShell in the
repository root and run:

```powershell
dotnet run --project .\HaddySimHub -- --capture "$HOME\Documents\HaddySimHub-raw"
```

Start any supported simulator (Dirt Rally 2, ETS2, iRacing, Assetto Corsa,
ACC, Assetto Corsa Rally, Microsoft Flight Simulator, or Forza Horizon 5) and
let the application receive telemetry, then stop HaddySimHub cleanly. Confirm
that a status file reports
`"status":"complete"` before using the capture; `capturing` or `incomplete`
must not be used as a complete corpus. The raw files appear in
`%USERPROFILE%\Documents\HaddySimHub-raw\` as `<game>.jsonl` for each game
source that was active and published data. Copy the desired file(s) to a
development machine without editing the JSONL lines.

The ACC raw-page checker validates page lengths against the committed ACC
manifest, verifies SHA-256 checksums when present, and decodes the
converter-consumed fields. Run it from the repository's `rust` directory:

```powershell
cargo run --locked -p simhub-acc-parity -- "C:\path\to\acc.jsonl"
```

Linux/macOS example:

```bash
cargo run --locked -p simhub-acc-parity -- /path/to/acc.jsonl
```

For raw-only captures, the tool reports that it validated raw ACC data and
explicitly says no C# parity comparison was performed. A full reader/converter
parity check requires independent expected results; this raw capture alone does
not claim that both implementations agree. Rust replay support for the other
game sources is not yet integrated into this checker. Existing community Rust
reader candidates are surveyed in
[Rust telemetry implementation research](rust-telemetry-implementations.md);
each candidate still needs source-version, field-coverage, and license review
before reuse.

## Layout manifests

`fixtures/telemetry/manifest/<game>.json` records field names, types, sizes and
offsets for the C# structs used to marshal source memory images, including
nested structs and fixed-size arrays. `Pack` and `CharSet` are included because
they affect those offsets. These files pin the known ACC/AC-family/MSFS/Forza
layouts; they are not a replacement for capturing the source bytes.

Generate manifests after a layout change only once that change is understood
and the associated raw source data has been reviewed:

```bash
HADDYSIMHUB_UPDATE_MANIFESTS=1 dotnet test HaddySimHub.sln --filter "FullyQualifiedName~LayoutManifestTests"
```

Without the environment variable, the same test checks the committed
manifests. iRacing's shared-memory header/variable-buffer protocol and ETS2's
SDK shared-memory structure still need to be described from their raw captures
as the Rust readers are implemented.
