# Update Report Template

Use this structure in PR description or handoff notes.

```text
Frontend Dependency Update Report

Scope:
- Directory: ClientApp
- Strategy: patch/minor first, majors reviewed explicitly

Updated Packages:
- package-a: 1.2.3 -> 1.2.5 (patch, low risk)
- package-b: 4.7.0 -> 4.8.0 (minor, medium risk)
- package-c: 2.9.0 -> 3.0.0 (major, high risk; changelog reviewed)

Validation:
- build: pass/fail
- test_ci: pass/fail
- lint: pass/fail

Breaking Changes / Follow-ups:
- [none] or short bullet list

Rollback Plan:
- Revert package.json + package-lock.json from this change set.
```
