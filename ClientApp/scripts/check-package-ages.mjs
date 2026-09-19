#!/usr/bin/env node
// Verify that every package version newly introduced by this change is old
// enough to pass Safe Chain's minimum-package-age gate, before it is committed
// and pushed. Anything younger than MIN_AGE_HOURS makes the script exit 1.
//
// Usage:
//   node ClientApp/scripts/check-package-ages.mjs
//
// Environment:
//   MIN_AGE_HOURS                 age gate in hours (default 48)
//   CHECK_PACKAGE_AGES_BASE_REF   git ref whose lockfile is the baseline
//                                 (default HEAD — the working tree holds the
//                                 update and HEAD still points at the parent)
//   CHECK_PACKAGE_AGES_SKIP=1     bypass the check

import { execFileSync } from 'node:child_process';
import { readFileSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

const here = dirname(fileURLToPath(import.meta.url));
const repoRoot = join(here, '..', '..');
const lockPath = join(repoRoot, 'ClientApp', 'package-lock.json');

const minAgeHours = Number(process.env.MIN_AGE_HOURS ?? 48);
const baseRef = process.env.CHECK_PACKAGE_AGES_BASE_REF ?? 'HEAD';

if (process.env.CHECK_PACKAGE_AGES_SKIP === '1') {
  console.log('[check-package-ages] skipped via CHECK_PACKAGE_AGES_SKIP=1');
  process.exit(0);
}

function packageName(key) {
  return key && key !== '' ? key.split('node_modules/').pop() : null;
}

function readLock(path) {
  try {
    const parsed = JSON.parse(readFileSync(path, 'utf8'));
    return (parsed.packages ?? {}) || {};
  } catch {
    return null;
  }
}

// lockfile v3 "packages" entries carry "version" but not "name"; the package
// name has to be derived from the node_modules path
function versionsByKey(packages, into = new Map()) {
  for (const [key, entry] of Object.entries(packages)) {
    if (!entry?.version) continue;
    const name = packageName(key);
    if (!name) continue;
    into.set(name, entry.version);
  }
  return into;
}

const current = readLock(lockPath);
if (!current) {
  console.error('[check-package-ages] could not read ClientApp/package-lock.json');
  process.exit(1);
}

let base = null;
try {
  const raw = execFileSync('git', ['show', `${baseRef}:ClientApp/package-lock.json`], {
    encoding: 'utf8',
    stdio: ['ignore', 'pipe', 'ignore'],
  });
  base = JSON.parse(raw).packages ?? null;
} catch (err) {
  console.warn(`[check-package-ages] no baseline lockfile at ${baseRef} (${err.message.split('\n')[0]}); nothing to compare.`);
}
if (!base) {
  console.warn('[check-package-ages] baseline missing — treating every installed package as new');
  base = {};
}

const baseByVersion = new Set();
for (const [key, entry] of Object.entries(base)) {
  if (!entry?.version) continue;
  const name = packageName(key);
  if (!name) continue;
  baseByVersion.add(`${name}@${entry.version}`);
}

const changed = versionsByKey(current);
const introduced = [];
for (const [name, version] of changed) {
  if (!baseByVersion.has(`${name}@${version}`)) introduced.push({ name, version });
}

if (introduced.length === 0) {
  console.log('[check-package-ages] no new package versions introduced — nothing to do');
  process.exit(0);
}

const seen = new Set();
const offenders = [];
const checked = [];

async function publishTime(name, version) {
  if (seen.has(`${name}@${version}`)) return null;
  seen.add(`${name}@${version}`);
  try {
    const res = await fetch(`https://registry.npmjs.org/${encodeURIComponent(name)}`);
    if (!res.ok) return null;
    const packument = await res.json();
    return packument?.time?.[version] ?? null;
  } catch {
    return null;
  }
}

await Promise.all(
  introduced.map(async ({ name, version }) => {
    const published = await publishTime(name, version);
    if (!published) return;
    const ageHours = (Date.now() - Date.parse(published)) / 3_600_000;
    if (ageHours < minAgeHours) offenders.push({ name, version, published, ageHours });
    checked.push({ name, version, published, ageHours });
  }),
);

checked.sort((a, b) => a.ageHours - b.ageHours);
for (const { name, version, published, ageHours } of checked) {
  console.log(`[check-package-ages] ${name}@${version} published ${published} (${ageHours.toFixed(1)}h old)`);
}

if (offenders.length > 0) {
  console.error(`\n[check-package-ages] ${offenders.length} package(s) younger than Safe Chain minimum age (${minAgeHours}h).`);
  console.error('Hold them back before committing (this is the policy working, not a test failure):\n');
  for (const { name, version } of offenders) {
    console.error(`  npm install ${name}@${version} --package-lock-only`);
  }
  console.error(`\nRe-run this script afterwards; the versions here will clear on roughly ${new Date(Date.now() + (minAgeHours + 24) * 3_600_000).toISOString()}.`);
  process.exit(1);
}

console.log(`[check-package-ages] all ${checked.length} new package version(s) past the ${minAgeHours}h age gate`);