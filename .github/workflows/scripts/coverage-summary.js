const fs = require('fs');
const path = require('path');

const repoRoot = process.cwd();

function parseLcov(lcovPath) {
  const content = fs.readFileSync(lcovPath, 'utf8');
  const records = content.split('end_of_record');
  
  let totalLines = 0;
  let hitLines = 0;
  let totalFunctions = 0;
  let hitFunctions = 0;
  let totalBranches = 0;
  let hitBranches = 0;
  
  for (const record of records) {
    if (!record.trim()) continue;
    
    const lines = record.split('\n');
    
    for (const line of lines) {
      if (line.startsWith('LF:')) {
        totalLines += parseInt(line.substring(3), 10) || 0;
      } else if (line.startsWith('LH:')) {
        hitLines += parseInt(line.substring(3), 10) || 0;
      } else if (line.startsWith('FNF:')) {
        totalFunctions += parseInt(line.substring(4), 10) || 0;
      } else if (line.startsWith('FNH:')) {
        hitFunctions += parseInt(line.substring(4), 10) || 0;
      } else if (line.startsWith('BRF:')) {
        totalBranches += parseInt(line.substring(4), 10) || 0;
      } else if (line.startsWith('BRH:')) {
        hitBranches += parseInt(line.substring(4), 10) || 0;
      }
    }
  }
  
  return {
    lines: { total: totalLines, hit: hitLines },
    functions: { total: totalFunctions, hit: hitFunctions },
    branches: { total: totalBranches, hit: hitBranches }
  };
}

function formatPercentage(hit, total) {
  if (total === 0) return '0';
  return ((hit / total) * 100).toFixed(1);
}

function printCoverage(title, lcovPath) {
  if (fs.existsSync(lcovPath)) {
    const stats = parseLcov(lcovPath);
    console.log(`### ${title}`);
    console.log('');
    console.log('```');
    console.log('| Metric    | Coverage |');
    console.log('|-----------|----------|');
    console.log(`| Lines     | ${formatPercentage(stats.lines.hit, stats.lines.total)}% |`);
    console.log(`| Functions | ${formatPercentage(stats.functions.hit, stats.functions.total)}% |`);
    console.log(`| Branches  | ${formatPercentage(stats.branches.hit, stats.branches.total)}% |`);
    console.log('```');
    console.log('');
  } else {
    console.log(`### ${title}`);
    console.log('');
    console.log('No coverage file found');
    console.log('');
  }
}

// Paths are relative to repo root in CI
const backendPath = path.join(repoRoot, 'coverage', 'server', 'coverage.info');
const frontendPath = path.join(repoRoot, 'coverage', 'frontend', 'lcov.info');

printCoverage('Backend (.NET)', backendPath);
printCoverage('Frontend (TypeScript)', frontendPath);
