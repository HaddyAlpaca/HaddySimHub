import { test, expect } from '@playwright/test';

test('Initial page is shown', async ({ page }) => {
  await page.goto('https://localhost:3333/');

  // Expect a title "to contain" a substring.
  await expect(page).toHaveTitle(/Playwright/);
});
