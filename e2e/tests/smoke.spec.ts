import { test, expect } from '@playwright/test';

test('Smoke when server cannot be reached', async ({ page }) => {
  await page.goto('http://localhost:3333/');
});

test('Rally display is shown', async ({ page }) => {

});
