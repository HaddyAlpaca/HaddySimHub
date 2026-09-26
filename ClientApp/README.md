# HaddySimHub Client (Lit + Vite)

## Fast dev loop with hot-reload (recommended)

Run the backend once and the frontend using the Vite dev server with HMR.

1. **Start the backend** (from the repository root):

   ```bash
   dotnet run --project HaddySimHub -- --no-update
   ```

   The backend listens on `http://localhost:3333` (including the SSE endpoint `/display-data/stream`).

2. **Start the frontend dev server** (from `ClientApp/`):

   ```bash
   npm start
   ```

   Open `http://localhost:5173`. Changes in `src/` are hot-reloaded — no rebuild, copy or backend restart required.

The dev server proxies `/display-data` (SSE endpoint) to the backend on port 3333; see [`proxy.conf.json`](./proxy.conf.json).

## Keeping SCSS clean

Stylelint checks every SCSS file in CI and can automatically fix the formatting
rules it supports:

```bash
npm run lint:styles       # check only
npm run lint:styles:fix   # apply safe automatic fixes
```

To inspect selectors that do not appear to be used by the built application,
run:

```bash
npm run analyze:styles
```

This is a report-only heuristic. Lit templates can contain dynamic class names,
so review the output before removing any selector.

## End-to-end tests

Run the real frontend and backend together in Chromium without a simulator:

```bash
npm run test:e2e
```

Playwright starts the backend with `--e2e` and the Vite development server.
That backend mode disables game display polling and exposes
`POST /__e2e/display-update` on loopback only; the tests post a `DisplayUpdate`
and assert that the corresponding dashboard renders the supplied values. The
injection route is not registered during normal application runs.

## Production build to wwwroot

The frontend is automatically built as part of `dotnet publish` — the
`PublishClientApp` MSBuild target in `HaddySimHub/HaddySimHub.csproj` runs
`npm run build` and places the output into the publish output's `wwwroot`.
This is the same step used by the CD workflow, so no manual copy is required:

```bash
dotnet publish ./HaddySimHub -r win-x64 --output ./dist
# ./dist/wwwroot now contains the production frontend
```

A plain `dotnet build` or `dotnet test` will intentionally not trigger this (keeps them fast and Node-free). Skip the frontend build explicitly with `-p:BuildClientAppOnPublish=false` (for example when publishing on a machine without Node).
