# HaddySimHub Client (Angular)

## Fast dev loop with hot-reload (recommended)

Instead of `ng build` → copying files to `wwwroot` → restarting the backend,
run the backend once and the frontend using the Angular dev-server with HMR.

1. **Start the backend** (from the repository root) and optionally set a test mode directly:

   ```bash
   dotnet run --project HaddySimHub --test race    # or: rally / truck
   ```

   The backend listens on `http://localhost:3333` (including the SignalR hub `/display-data`).

2. **Start the frontend dev server** (from `ClientApp/`):

   ```bash
   npm start
   ```

   Open `http://localhost:4200`. Changes in `src/` are hot-reloaded — no rebuild, copy or backend restart required.

The dev server proxies `/display-data` (including the WebSocket upgrade) to the backend on port 3333; see [`proxy.conf.json`](./proxy.conf.json).

## Selecting a test mode

- Via CLI when starting the backend: `--test race`, `--test rally` or `--test truck` (also `--test=race` works). An unknown value is ignored with a warning.
- At runtime in the backend console: press `Ctrl+T` to cycle through none → race → rally → truck.

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
