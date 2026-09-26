import { configDefaults, defineConfig } from 'vitest/config';
import { playwright } from '@vitest/browser-playwright';

export default defineConfig({
  test: {
    exclude: [...configDefaults.exclude, 'e2e/**'],
    browser: {
      provider: playwright(),
      enabled: true,
      instances: [
        { browser: 'chromium' },
      ],
    },
    coverage: {
      provider: 'v8',
      reporter: ['text', 'lcov', 'html'],
      reportsDirectory: './coverage',
      exclude: [
        '**/*.spec.ts',
        '**/*.harness.ts',
        '**/testing/**',
        '**/index.ts',
      ],
    },
  },
});
