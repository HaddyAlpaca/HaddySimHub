import { readFile } from 'node:fs/promises';
import { glob } from 'node:fs';
import { promisify } from 'node:util';
import { PurgeCSS } from 'purgecss';

const globAsync = promisify(glob);
const cssFiles = await globAsync('dist/haddy-sim-hub-client/assets/*.css');

if (cssFiles.length === 0) {
  throw new Error('No built CSS found. Run `npm run build` before analyzing styles.');
}

const results = await new PurgeCSS().purge({
  content: ['src/**/*.{ts,html}'],
  css: cssFiles,
  rejected: true,
  safelist: {
    standard: [/^haddy-/],
    deep: [/^haddy-/],
    greedy: [/^haddy-/],
  },
});

for (const result of results) {
  const original = (await readFile(result.file, 'utf8')).length;
  const remaining = result.css.length;
  const rejected = result.rejected ?? [];
  const reduction = original === 0 ? 0 : ((original - remaining) / original) * 100;

  console.log(`${result.file}: ${original} -> ${remaining} bytes (${reduction.toFixed(1)}% removable)`);
  if (rejected.length > 0) {
    console.log('Potentially unused selectors:');
    rejected.sort().forEach((selector) => console.log(`  ${selector}`));
  }
}
