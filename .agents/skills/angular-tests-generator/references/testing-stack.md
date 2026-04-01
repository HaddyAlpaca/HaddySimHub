# Testing Stack

**Vitest + Playwright + CDK Harness + vitest-mock-extended**

| Technology | Purpose |
|------------|---------|
| **Vitest** | Test runner and assertion library |
| **Playwright** | Browser provider for Angular browser tests |
| **CDK Testing** | Component harnesses for clean test APIs |
| **vitest-mock-extended** | Type-safe mocking |
| **Angular signals** | State management mocking |

## Running Tests

```bash
# Watch mode
npm test

# CI mode (single run)
npm run test_ci
```

## Test File Naming

- Pattern: `*.spec.ts` (co-located with source)
- Harness: `*.component.harness.ts`
