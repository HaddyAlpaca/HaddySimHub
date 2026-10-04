---
name: update-dependencies
description: Update the Rust crate dependencies on a branch and record any version held back. Use when asked to update, bump, or refresh dependencies.
---

# Updating dependencies

Dependencies are updated by hand; there is no scheduled workflow. One rule matters
most: **every version held back is written down next to the dependency it holds
back.**

## Branch

`deps/update-YYYY-MM-DD`.

## Compatible updates

Run from `rust/`.

```bash
cargo update            # newest versions Cargo.toml allows; changes Cargo.lock only
```

## Majors

`cargo update` never crosses a major (or a `0.x` minor). List what is on offer with
`cargo search <crate>` or crates.io, and take them one at a time, each with a reason
to believe it fits:

- `simetry` and `yaml-rust` are pinned together on purpose: `SimState::session_info`
  returns simetry's `Yaml`, so the two must resolve to the same crate.
- `slint` and `slint-build` move together.
- `windows` is pinned to the version already in the lockfile through other crates,
  so a bump there pulls a second copy into the build unless they move too.

## Recording what was held back

A hold is invisible otherwise: the range in `Cargo.toml` looks satisfied, so nothing
says the version was kept back deliberately. Put a comment directly above the
dependency with:

- the version it is held at and the date
- the reason
- **a removal condition someone can check** — an upstream release, a fixed issue,
  another crate that has to move first

Read those comments at the start of every update and drop the holds whose condition
has been met.

## Verifying

From `rust/`:

```bash
cargo fmt --all -- --check
cargo test --workspace --locked
cargo build --release --locked -p simhub-app
```

## The PR

State plainly which majors were taken and which were held back, with the reason for
each. "Held back out of caution" is not a reason; "simetry has not released against
the new yaml-rust" is.
