---
name: update-dependencies
description: Update NuGet and npm dependencies on a branch, respecting the Safe Chain minimum-age policy and recording any version held back. Use when asked to update, bump, or refresh dependencies, or to prepare the dependency-update PR by hand instead of waiting for the scheduled workflow.
---

# Updating dependencies

The scheduled `.github/workflows/deps-update.yml` does this on the 1st and 15th.
Doing it by hand follows the same rules, plus one this file exists for: **every
version held back gets written down in `ClientApp/DEPENDENCY-PINS.md`.**

## Branch

`deps/update-YYYY-MM-DD`, matching the workflow's naming.

## NuGet

```bash
dotnet list HaddySimHub.sln package --outdated
dotnet add <project> package <name> --version <version>
```

Two things to check afterwards:

- `dotnet add package` reformats the `.csproj` it touches, and has been seen
  collapsing a multi-line `<Target>` onto one line. Read the diff and restore any
  formatting change that is not a version bump.
- `iRacingSDK.Net/` holds two project files. Only `iRacingSDK.Net.csproj` is in the
  solution; `iRacingSDK.csproj` is not referenced and should be left alone.

## npm

Run from `ClientApp/`.

```bash
npx npm-check-updates@latest                  # see what is on offer
npx npm-check-updates@latest -u --target minor
npm install
```

**Leave `engines` and `overrides` alone** when taking bulk updates — the scheduled
workflow saves and restores `engines` for exactly this reason. Change `overrides`
only as a deliberate pin, and then record it (see below).

Take majors one at a time, and only with a reason to believe they fit:

- Check the peer range before bumping anything Angular type-checks against.
  `@angular/compiler-cli` declares a narrow `typescript` range, and a TypeScript
  major outside it is an unmet peer dependency, not a risk to weigh.
- `@types/node` tracks the major in `engines.node`. A newer major type-checks
  against APIs the runtime does not have.

After a `playwright` bump, `npx playwright install chromium` is needed once locally
before the tests will run. CI installs its own.

## The Safe Chain minimum-age policy

CI installs through `scripts/safe-chain-wrapper.sh`, which blocks packages below a
minimum age — roughly two days. This catches the case where a compromised release
is published and pulled shortly after, so **do not reach for
`--safe-chain-skip-minimum-package-age`.** A blocked install is the policy working.

The failure surfaces as a red "Frontend tests" check whose log ends in
`403 Forbidden - blocked by safe-chain direct download minimum package age`,
followed by the list of packages. It reads like a test failure and is not one.

Catch it before pushing by ageing the versions the update actually introduces:

```bash
git show main:ClientApp/package-lock.json > /tmp/lock-main.json
# diff the two lockfiles for changed versions, then for each:
npm view <package>@<version> time --json
```

Anything under two days old will be blocked. Hold it back:

- **Direct dependency** — `npm install --save-dev <pkg>@<older-version>`. Lowering
  the range in `package.json` alone is not enough: `npm install` will not downgrade
  a version the lockfile already satisfies, and `npm ci` installs from the lockfile.
- **Transitive dependency** — an `overrides` entry in `package.json`, then
  `npm install`. Confirm it landed by reading the version out of the lockfile.

## Recording what was held back

Every pin goes in `ClientApp/DEPENDENCY-PINS.md` before the PR opens, with:

- what is pinned and to which version
- for a transitive pin, the path that reaches it
- the date and the reason
- **a removal condition someone can check** — a date the age gate clears, an
  upstream release, a peer range that has to widen

Pins are otherwise invisible: the range in `package.json` looks satisfied, so
`npm outdated` says nothing and the next person has no way to know the hold was
deliberate or whether it still applies. `overrides` entries are worse, because the
scheduled workflow preserves them, so they outlive the reason unless someone
removes them by hand.

Read that file at the start of every dependency update and drop the entries whose
condition has been met. That is the point of writing them down.

## Verifying

Backend, from the repo root:

```bash
dotnet build HaddySimHub.sln     # TreatWarningsAsErrors is on: 0 warnings expected
dotnet test HaddySimHub.sln
```

Frontend, from `ClientApp/`:

```bash
npm run build
npm run test_ci
npm run lint
npm audit --omit=dev
```

The `truck-display` and `rally-display` SCSS budget warnings are long-standing. Do
not report them as new unless a stylesheet or budget setting actually changed.

## The PR

State plainly which majors were taken and which were held back, with the reason for
each. "Held back out of caution" is not a reason; "outside the peer range Angular
declares" is. Link anything recorded in `DEPENDENCY-PINS.md` so a reviewer can see
the hold is tracked rather than forgotten.
