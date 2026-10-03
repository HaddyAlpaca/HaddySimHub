# Handover to Windows

Development moved to Windows after the telemetry layer was built. This records
what changes on that machine, and what can only be answered there.

## Build changes

`rust/crates/simhub-telemetry/Cargo.toml` declares `simetry` under
`[target.'cfg(windows)'.dependencies]`. That gating exists only so the crate
still builds on Linux, where `simetry` pulls `windows` unconditionally and would
fail. On Windows it can become a plain `[dependencies]` entry.

Keep the feature limitation:

```toml
simetry = { version = "0.2.3", default-features = false, features = ["with_truck_simulator"] }
```

`with_r3e` is on by default and runs `bindgen` over a RaceRoom header we do not
use. It needs libclang and the target's C headers, and it was what blocked
cross-compiling; there is no reason to carry it.

Cross-checking with `cargo check --target x86_64-pc-windows-msvc` becomes the
native build. `simhub-ui` should compile there — its failure on Linux is a
missing fontconfig, not a code problem.

## What only a running game can settle

Every item below is a concrete discrepancy found while porting, in a game that
has already been played. Each is one session away from being resolved.

| Game | Question | What a wrong answer looks like |
| --- | --- | --- |
| Forza Horizon 5 | Is the datagram 323 or 324 bytes? | If the trailing byte belongs to a field, every offset after it is wrong and the dashboard still looks plausible |
| ACC | Is the rev limit zero or nonsense? | `current_max_rpm` on the physics page is documented as never written by the sim; the real limit is on the static page, which the C# reader does not map |
| ACC | Is session time remaining a thousand times too large? | The ACC reader scales the page value by 1000 where the Assetto Corsa reader divides the same field at the same offset by 1000 |
| DiRT Rally 2 | Does stage progress sit at 100% immediately? | The field at offset 12 is read as a 0-1 fraction, but the published layout and `simetry` both call it total distance driven |
| ETS2 | Does the dashboard always report ten trailers, with damage ten times too low? | The map carries ten blocks regardless; the C# reader never checked the per-block `attached` flag |
| ETS2 | What does a 14-speed gearbox show in second gear? | The crawler labels were off by one; now fixed to `C1`, `C2`, then 1-12 |
| AC Rally | Do the values look sane at all? | Its page layout was copied from ACC, never validated, and has no published specification |
| MSFS 2020 | Do the deviation needles point the right way? | Positive is assumed to mean right of course and above the glidepath; mirrored needles are one negation |

## State of the port

Converters are complete for all eight games and tested: 108 tests over models
and converters, all pure and platform-independent.

Telemetry sources split in two. Decoding — bytes to a telemetry struct — is
pure and tested, 61 tests covering Dirt Rally 2, Forza, AC, ACC, AC Rally and
ETS2, plus the UDP receive path against a real socket. Acquisition for the five
games `simetry` covers is adapted behind `simetry_source` modules, which are
type-checked but not unit-tested, since those types exist only on Windows.

Two of those adapters are lossier than the byte decoders they sit beside:
`simetry` does not expose ACC's session lap total, nor Dirt Rally 2's sector
split times. Where both paths exist, the byte decoder is the fuller one.

## Still to do

The readers for Forza, MSFS 2020 and AC Rally have no `simetry` equivalent and
use the decoders in this crate. Beyond that: process detection, the
single-active-display lifecycle, wiring the converters to the Slint UI, and the
updater.
