# Rust reader strategy: dependencies versus in-house

Which Rust implementations we take as dependencies, which we write ourselves,
and why. Dependency health figures were measured on 2026-10-03 and should be
re-checked before any of these decisions is acted on.

## The two axes

A dependency is worth taking when it is healthy *and* when the protocol it
decodes still moves. Those are separate questions and they pull in different
directions.

**Protocol churn.** Five of our eight sources are frozen: the game is
superseded or no longer updated, so its wire format cannot drift. For those, a
decoder whose last commit is years old is finished, not abandoned, and the
staleness of a candidate says little. Three sources still move and need an
implementation that moves with them.

| Frozen | Still moving |
| --- | --- |
| Dirt Rally 2.0, Forza Horizon 5, MSFS 2020, Assetto Corsa, ACC | iRacing (season SDK updates), ETS2 (active title plus a community plugin ABI), AC Rally (early access) |

**Dependency health.** Maintenance and license, together. A license problem is
disqualifying regardless of how active a project is, and in this ecosystem the
two correlate badly: the most active projects are the ones with no license at
all.

| Candidate | Last push | License | Verdict |
| --- | --- | --- | --- |
| `acc_shared_memory_rs` | 2025-06 | MIT OR Apache-2.0 | Usable |
| `cm-telemetry` | 2023-05 | MIT | Usable for a frozen format |
| `simetry` | 2024-02 | MIT | Usable for a frozen format; covers 5 of our 8 |
| `iracing-telem` | 2024-08 | BSD-3-Clause | Only licensed iRacing option, 2 years quiet |
| `scs-sdk-telemetry` | 2025-02 | MPL-2.0 | File-level copyleft; we would modify it |
| `flybywireless-simconnect` | 2026-09 | MIT (crates.io only) | Fresh but 0.2.0, one maintainer, no LICENSE file |
| `racedirector/iracing.rs` | 2026-10 | **none** | Unusable: unpublished, all rights reserved |
| `0x20F/forza-telemetry` | 2026-05 | **none** | Unusable: same |
| `EffortlessMetrics/OpenRacing` | 2026-09 | NOASSERTION | Unusable without clarification |
| `simconnect-sdk` | 2026-02 | MIT | **Archived** |
| `acr_telemetry` | — | PolyForm Noncommercial | Excluded; do not read while writing our AC Rally reader |

## Decisions

| Source | Decision | Reasoning |
| --- | --- | --- |
| ACC | **Dependency:** `acc_shared_memory_rs` | Clean dual permissive license, published, frozen low-churn layout. The only candidate passing both filters outright, and our first port, so it tests the approach early |
| Dirt Rally 2.0 | Dependency candidate: `cm-telemetry` | MIT, frozen Codemasters EGO format on port 20777. Staleness is not a risk for a format that stopped changing |
| Assetto Corsa | Dependency candidate: `simetry` AC path | MIT, frozen 2014-era layout, documented at the assettocorsamods reference |
| ETS2 | **In-house** | `scs-sdk-telemetry` is MPL-2.0 and we would modify its files. The real contract is the RenCloud plugin ABI, which we already depend on in any language; pin its revision |
| iRacing | **In-house** | The one actively maintained project with the right shape, `racedirector/iracing.rs`, has no license and is unpublished. `iracing-telem` is BSD-3 but two years quiet against a protocol that actually drifts |
| Forza Horizon 5 | **In-house** | Not a maintenance question: every Forza candidate is unlicensed or NOASSERTION. License is a hard blocker regardless of freeze |
| MSFS 2020 | **In-house FFI to `SimConnect.dll`** | See below |
| AC Rally | **In-house** | No published layout, and the only project is noncommercial |

## MSFS 2020: vendor DLL, not the wire protocol

`flybywireless-simconnect` implements the SimConnect wire protocol in pure Rust
over a named pipe or TCP with no `SimConnect.dll`, and supports MSFS 2020
deliberately: the sim is "KittyHawk" internally and negotiation can be capped
at exactly its opcode set with `--no-default-features --features kittyhawk`.

We are not taking it. We stay on the vendor DLL with our own FFI bindings:

- MSFS 2020 is superseded by MSFS 2024, so its ABI is frozen. The reason to
  take a dependency — someone else tracks the protocol — buys nothing when
  there is no protocol movement left to track.
- It preserves the capture boundary we already have. Moving to the wire
  protocol makes the existing MSFS corpus the wrong shape and forces a
  re-capture at pipe level, which the C# application cannot produce without a
  pipe shim.
- `SimConnect.dll` ships with the simulator, so the FFI route adds no install
  burden.
- The crate's risk is immaturity rather than staleness: 0.2.0, published one
  month ago, three stars, one maintainer, 51% documented, and MIT declared only
  in crates.io metadata with no LICENSE file in the repository.

The wire-protocol route buys freedom from vendor binaries. Against a frozen sim
that is purity, not future-proofing, and it costs a re-capture.

## AC Rally and the ACC layout

Our manifests show AC Rally's physics (800 B, 85 fields) and graphics
(1588 B, 84 fields) are offset-for-offset identical to ACC's; every difference
is a type or field name. AC classic is unrelated at 580/252/420, so AC Rally is
ACC-derived rather than AC-derived.

This is the weakest position of the eight, because three things stack:

- The identical manifests show our two C# structs are copies of each other,
  not that the two games publish the same bytes.
- The AC Rally implementation has never been validated against the running
  game, so nothing has ever tested the copy.
- No published layout exists to check it against, and the one community
  project is noncommercial and therefore off limits.

Offset 1404 is already divergent — `LastSectorTime2` in ACC, `ISplit` in
AC Rally — which suggests the two do drift where it matters. AC Rally also
reads a third page, `acpmf_static` (820 B, 45 fields), that our ACC reader does
not map at all, so no ACC reference covers it.

`HaddySimHub/Displays/README.md` states the three titles share no page layout,
which contradicts the manifests for this pair. A capture of both games settles
it, and should happen before any AC Rally Rust work starts.

## Oracle status

The migration plan treats the existing C# behavior as a temporary oracle. That
holds for only half the set.

| Validated against the running game | Never run against the game |
| --- | --- |
| ACC, iRacing, ETS2, Dirt Rally 2.0 | Assetto Corsa, AC Rally, Forza Horizon 5, MSFS 2020 |

For the right-hand column a passing Rust-versus-C# parity test proves only that
two implementations decode identically, not correctly: a layout error is
inherited by the port and the gate still goes green. Those four need validation
against the published format and against plausible live values, not against our
own implementation. AC, FH5 and MSFS 2020 have documentation to fall back on.
AC Rally has none.

## Status

This lists discoverable material and measured project health, not correctness
against live simulators. Corpus capture and replay parity remain the gate for
every reader, bought in or written here.
