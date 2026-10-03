# Handover to Windows

Development moved to Windows after the telemetry layer was built. This records
what changes on that machine, and what can only be answered there.

## Picking it up

```
git fetch origin
git checkout rust-migration
```

Two things the toolchain needs beyond a normal Rust install. An MSVC target,
which `rustup target list --installed` will confirm. And libclang, because
`simetry` runs `bindgen` in its build script to generate the SCS layout from
the plugin's own header — if `cargo build` stops on a missing `libclang.dll`,
that is the cause and not the code.

## Build changes

Done. CI and CD now run on Windows runners only, so `simetry` is a plain
dependency and the `simetry_source` modules are no longer gated on
`cfg(windows)`. The feature limitation stays: `with_r3e` runs `bindgen` over a
RaceRoom header we do not use.

Moving the other jobs to Windows turned up three things that Linux CI had hidden:

- `MsfsGameDataProvider` did not compile: a `var` initialised from a method
  group or `null` has no type. The branch never ran through CI.
- The layout manifests failed byte-for-byte on a CRLF checkout. The serializer
  now writes `\n` regardless of platform, and `.gitattributes` keeps the
  committed files LF.
- Under Git Bash the Safe Chain wrapper ran npm without Safe Chain, because its
  Windows shims are `.cmd` files that bash never resolves. The wrapper now calls
  `safe-chain` directly, and its own test package is blocked again.

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

## Where to start

Before touching code, run the four games that have been played before — ACC,
iRacing, ETS2 and Dirt Rally 2 — for a few minutes each with `--capture`. That
produces the fixtures this repository still lacks, and the recordings answer
the ACC and Dirt Rally 2 questions above on the way past.

Then start Forza Horizon 5 and Assetto Corsa Rally once each. Those are the two
whose layouts rest on an assumption rather than on a specification, and they
are where the difference between correct and merely plausible actually shows.

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
