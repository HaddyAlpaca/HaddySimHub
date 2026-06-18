# Frontend Dependency Updater Skill

This skill updates `ClientApp` npm packages safely with a patch/minor-first strategy, explicit handling for major upgrades, and validation through existing project scripts.

## Quick Start

1. Inventory outdated packages with `npm outdated`.
2. Apply low-risk updates (patch/minor), then run `build`, `test_ci`, and `lint`.
3. Evaluate and apply major updates package-by-package with changelog review.
4. Produce a concise update report with risk and rollback notes.

## Reference Files

| Topic | File |
|-------|------|
| Update Workflow | [references/workflow.md](references/workflow.md) |
| Commands & Validation | [references/commands.md](references/commands.md) |
| Risk Classification | [references/risk-classification.md](references/risk-classification.md) |
| Reporting Template | [references/report-template.md](references/report-template.md) |
