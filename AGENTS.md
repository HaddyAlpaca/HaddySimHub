# AGENTS.md

## Build & Test

### Backend (.NET)
- Build: `dotnet build HaddySimHub.sln`
- Test: `dotnet test HaddySimHub.sln --no-restore`
- Test with coverage: `dotnet test HaddySimHub.sln --no-restore --collect:"XPlat Code Coverage"`
- Restore dependencies: `dotnet restore HaddySimHub.sln`

### Frontend (Angular)
- Install: `npm install` (from ClientApp directory)
- Build: `npm run build` (from ClientApp directory)
- Test: `npm run test_ci` (from ClientApp directory, non-interactive CI mode)
- Test with coverage: `npm run coverage` (from ClientApp directory)
- Lint (all): `npm run lint` (from ClientApp directory)
- Lint (TypeScript): `npm run lint:ts` (from ClientApp directory)
- Lint (SCSS): `npm run lint:styles` (from ClientApp directory)
- Start dev server: `npm start` (from ClientApp directory)

## Environment & Setup

- .NET SDK version: 10.0.201 (specified in global.json with `"rollForward": "latestFeature"`)
  - This means minimum SDK 10.0.201; rollForward will only accept 10.0.2xx patch versions
  - Verify your installed SDK: `dotnet --list-sdks`
- Backend target: .NET 10.0 with ImplicitUsings and Nullable enabled
- Frontend: Node.js 24.15.0+, npm 11.6.2+
- Backend Windows runtime identifiers: win-x64 (see HaddySimHub.csproj)

### .NET SDK Setup (Snap vs Native Install)

If your `dotnet` is snap-installed, it may not include SDK 10.0.201 by default:

```bash
# Check where dotnet comes from
which dotnet  # If /snap/bin/dotnet, you're using snap

# Check available SDKs
dotnet --list-sdks  # Look for 10.0.201
```

If you only see 10.0.107 or earlier:

```bash
# Install SDK 10.0.201 to ~/.dotnet (independent of snap)
curl -sSL https://dot.net/v1/dotnet-install.sh | bash -s -- --channel 10.0 --version 10.0.201

# Add ~/.dotnet to PATH (do this in your shell or ~/.bashrc for permanence)
export PATH="$HOME/.dotnet:$PATH"

# Verify it worked
dotnet --list-sdks  # Should now show 10.0.201
```

Then `dotnet build` will use the correct SDK version.

## Conventions

- The project is a monorepo: backend is ASP.NET Core (root), frontend is Angular (ClientApp/)
  - **Backend structure**: `HaddySimHub/` (main app), `HaddySimHub.Tests/` (tests), `HaddySimHub.Shared/` (shared models)
  - **Frontend structure**: `ClientApp/src/app/` (components, services, displays), `ClientApp/src/assets/` (static files)
  - **Backend entry point**: `HaddySimHub/Program.cs`
  - **Frontend entry point**: `ClientApp/src/index.html` and `ClientApp/src/main.ts`
- Backend settings: `TreatWarningsAsErrors` is enabled — fix all compiler warnings
- Frontend uses ESLint with `eslint.config.mjs` and Stylelint for SCSS validation
- Logging is excluded from HaddySimHub project (see Logging/** excludes in csproj)

## Do Not

- Do not use `npm install` from the repository root — always run from ClientApp/
- Do not use `dotnet build --watch` without understanding it doesn't auto-rebuild changed files reliably
- Do not ignore compiler warnings in .NET code — TreatWarningsAsErrors is enabled and tests will fail
- Do not modify generated files in dist/ (frontend output)
- Do not commit node_modules or bin/obj directories

## Common Pitfalls

- **Multiple directories**: Frontend commands must be run from ClientApp/; backend commands from repo root
- **Linting failures block CI**: Frontend linting (ESLint + Stylelint) runs in CI; address all linting warnings
- **Compiler warnings fail builds**: The backend has `TreatWarningsAsErrors` enabled, so unused variables or type issues will break the build
- **Node version mismatch**: The frontend requires Node 24.15.0+ and npm 11.6.2+; older versions will cause unexpected failures
- **Snap dotnet SDK mismatch**: Snap-installed dotnet may not include SDK 10.0.201 by default. See "Environment & Setup" section for how to install it to ~/.dotnet and update PATH.

## Security & Deployment

- **Deployment architecture**: iRacingSDK integration for race data (Windows-only runtime)
- **Authentication model**: Uses SignalR hubs for real-time game data updates (GameDataHub.cs)
- **Data handling**: Game telemetry data from iRacing is piped through DisplayBase implementations; no external API calls
- **Trust boundaries**: Backend is trusted; frontend displays are renderer processes that trust GameDataHub updates
- **CI/CD**: GitHub Actions workflows in .github/workflows/ handle build, test, and deployment automation
