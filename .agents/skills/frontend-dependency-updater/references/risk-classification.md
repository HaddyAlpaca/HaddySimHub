# Risk Classification

## Low Risk (Batch Allowed)

- Patch updates for libraries with stable APIs.
- Minor updates for tooling without config/schema changes.

## Medium Risk (Review Recommended)

- Minor updates to core UI/runtime libraries (`@angular/*`, `rxjs`, `chart.js`, `ng2-charts`).
- Lint/test tooling updates that may change rule behavior.

## High Risk (Single-Step, Explicit Review)

- Any major version upgrade.
- Angular ecosystem major upgrades (`@angular/*`, `@angular/cli`, `@angular-eslint/*`, `@angular/build`).
- Build tooling majors (TypeScript, ESLint, Vitest, Playwright).

## Rollback Guidance

- Revert `package.json` and `package-lock.json` together when a batch introduces regressions.
- Prefer small commits per logical update group to keep rollback surgical.
