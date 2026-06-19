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

- .NET SDK version: 10.0.100 (specified in global.json)
- Backend target: .NET 10.0 with ImplicitUsings and Nullable enabled
- Frontend: Node.js 24.12.0+, npm 11.6.2+
- Backend Windows runtime identifiers: win-x64 (see HaddySimHub.csproj)

## Conventions

- The project is a monorepo: backend is ASP.NET Core (root), frontend is Angular (ClientApp/)
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
- **Node version mismatch**: The frontend requires Node 24.12.0+ and npm 11.6.2+; older versions will cause unexpected failures
