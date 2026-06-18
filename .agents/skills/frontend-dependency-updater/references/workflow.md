# Update Workflow

## Scope

- Work only in `ClientApp/`.
- Respect engine constraints from `package.json` (`node ^24.12.0`, `npm >=11.6.2`).

## Standard Flow

1. **Baseline inventory**
   - Run `npm outdated` and capture current, wanted, and latest versions.
2. **Patch/minor batch**
   - Update low-risk packages first (`npm update`).
   - Re-run inventory to confirm what remains.
3. **Validation gate**
   - Run build, tests, and lint scripts from `package.json`.
   - Stop and resolve issues before continuing.
4. **Major upgrades (one by one or by family)**
   - Prioritize tooling/framework families together (for example Angular and Angular ESLint).
   - Review release notes/changelogs before applying.
5. **Final validation gate**
   - Re-run build, tests, and lint.
6. **Report**
   - Summarize changed packages, risk level, notable breaking changes, and rollback guidance.

## Security-First Variant

1. Run `npm audit --omit=dev` for runtime risk.
2. Apply non-breaking fixes first (`npm audit fix` when safe).
3. Continue with standard flow.
