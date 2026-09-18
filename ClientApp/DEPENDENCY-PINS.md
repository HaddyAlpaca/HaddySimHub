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

### `jsdom` → `^30.0.1`

- **Held since:** 2026-09-18
- **Why:** `30.1.0` was published 2026-09-17 (one day old), so Safe Chain blocked
  `npm ci` in CI. It also drags in `@asamuzakjp/dom-selector@9.2.0`, which is
  younger than the gate too.
- **Remove when:** `30.1.0` is past the age gate (roughly after 2026-09-19). Raise
  the range and run `npm install`.

### `baseline-browser-mapping` → `^2.11.24`

- **Held since:** 2026-08-22 (re-held 2026-09-18)
- **Why:** same minimum-age block; `2.11.25` was published 2026-09-17. This package
  releases most days, so expect a newer version to be available rather than the
  exact one that was blocked.
- **Remove when:** the latest release is past the age gate.

### `eslint-plugin-jsdoc` → `64.2.1`

- **Held since:** 2026-09-18
- **Why:** `64.5.3` was published 2026-09-18 (the same day the dependency PR ran).
  It drags in `jsdoc-type-pratt-parser@9.2.2` and `@es-joy/jsdoccomment@0.98.0`,
  both younger than the gate too.
- **Remove when:** `64.5.3` and those two transitives are past the age gate
  (roughly after 2026-09-20). Raise the version and run `npm install`.

### `typescript` → `6.0.3`

- **Held since:** 2026-09-18
- **Why:** `7.0.2` is supported by neither `typescript-eslint` (ESLint fails with
  "typescript-eslint does not support TS 7.0") nor the Angular toolchain yet. This
  is a compatibility hold, not a Safe Chain one.
- **Remove when:** `typescript-eslint` (and `@angular/compiler-cli`) support a
  7.x release.

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

Or run the whole check against the updated lockfile — it fails loudly on anything
younger than the Safe Chain gate:

```bash
node ClientApp/scripts/check-package-ages.mjs
```

It compares `ClientApp/package-lock.json` in the working tree with the lockfile at
`HEAD` (default) or any `CHECK_PACKAGE_AGES_BASE_REF` you pass, and exits 1 with
the offending versions if a newly introduced one is younger than `MIN_AGE_HOURS`
(default 48).

The scheduled `deps-update.yml` workflow runs `ncu -u` with no target filter, so it
will keep proposing the versions pinned here. That is expected: the pins live in
`package.json`, and the workflow's PR is where a fresh attempt shows up.
