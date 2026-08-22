# Dependency pins

Versions held back on purpose, and what has to be true before each one can go.

A pin is a cost: it keeps the project on an older version than the ecosystem is on,
and it is invisible in a normal `npm outdated` run because the range in
`package.json` looks satisfied. So every entry here needs a removal condition that
someone can actually check, and the list is reviewed whenever dependencies are
updated. An entry with no removal condition does not belong here — it belongs in a
comment on the dependency itself.

## How pins are expressed

- **Direct dependencies** — the range in `package.json` is lowered, and the exact
  version lands in `package-lock.json`. `npm ci` installs from the lockfile, so the
  lockfile is what CI actually gets.
- **Transitive dependencies** — an entry in the `overrides` block of
  `package.json`. Nothing else can reach a dependency of a dependency, and the
  scheduled dependency workflow deliberately preserves `overrides`, so an entry put
  there survives until somebody removes it by hand. That is exactly why it has to
  be written down here.

## Active pins

### `jose` → `6.2.9` (override)

- **Reached through:** `@angular/cli` → `@modelcontextprotocol/sdk`
- **Held since:** 2026-08-22
- **Why:** `6.2.10` was published the same day. Safe Chain blocks packages below its
  minimum age (see `scripts/safe-chain-wrapper.sh`), so `npm ci` fails in CI on a
  release that new. The version itself is not suspect — it is simply too young to
  have been observed.
- **Remove when:** the version `@modelcontextprotocol/sdk` resolves to is more than
  two days old, which is any time after roughly 2026-08-25. Drop the `overrides`
  entry, run `npm install`, and check that CI's install step passes.

### `eslint` → `^10.8.1`

- **Held since:** 2026-08-22
- **Why:** same minimum-age block; `10.9.0` was one day old.
- **Remove when:** `10.9.0` or later is past the age gate. Raise the range and run
  `npm install`.

### `baseline-browser-mapping` → `^2.11.15`

- **Held since:** 2026-08-22
- **Why:** same minimum-age block; `2.11.18` was published the same day. This
  package releases most days, so expect a newer version to be available rather than
  the exact one that was blocked.
- **Remove when:** the latest release is past the age gate.

## Pins that predate this file

`esbuild`, `@babel/core` and `piscina` are pinned in `overrides` without a recorded
reason. They are left alone until someone establishes why they are there; if the
reason turns out to be gone, remove the entry rather than documenting it here.

## Checking ages yourself

Safe Chain reports which packages it blocked and why, so a failing CI install names
them directly. To check before pushing:

```bash
npm view <package>@<version> time --json
```

The scheduled `deps-update.yml` workflow runs `ncu -u` with no target filter, so it
will keep proposing the versions pinned here. That is expected: the pins live in
`package.json`, and the workflow's PR is where a fresh attempt shows up.
