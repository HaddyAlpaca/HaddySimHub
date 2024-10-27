import { test, expect } from '@playwright/test';

test('Smoke when server cannot be reached', async ({ page }) => {
  await page.goto('https://localhost:3333/');

  // Expect a title "to contain" a substring.
  await expect(page).toHaveTitle(/Playwright/);
});
