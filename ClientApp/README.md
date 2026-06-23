# HaddySimHub Client (Angular)

## Snelle dev-loop met hot-reload (aanbevolen)

In plaats van `ng build` → bestanden naar `wwwroot` kopiëren → backend herstarten,
draai je de backend één keer en de frontend via de Angular dev-server met HMR.

1. **Start de backend** (vanuit de repo-root) en zet desgewenst direct een test mode:

   ```bash
   dotnet run --project HaddySimHub --test race    # of: rally / truck
   ```

   De backend luistert op `http://localhost:3333` (incl. de SignalR-hub `/display-data`).

2. **Start de frontend dev-server** (vanuit `ClientApp/`):

   ```bash
   npm start
   ```

   Open `http://localhost:4200`. Wijzigingen in `src/` worden direct hot-reloaded —
   geen rebuild, copy of backend-herstart meer nodig.

De dev-server proxyt `/display-data` (inclusief de WebSocket-upgrade) naar de backend
op poort 3333; zie [`proxy.conf.json`](./proxy.conf.json).

## Test mode kiezen

- Via CLI bij het starten van de backend: `--test race`, `--test rally` of `--test truck`
  (ook `--test=race` werkt). Een onbekende waarde wordt genegeerd met een waarschuwing.
- Tijdens runtime in de backend-console: druk op `Ctrl+T` om te cyclen door
  geen test mode → race → rally → truck.

## Productie-build naar wwwroot

De frontend wordt automatisch meegebouwd bij `dotnet publish` — de
`PublishClientApp`-target in `HaddySimHub/HaddySimHub.csproj` draait
`npm run build` en plaatst de output in de `wwwroot` van de publish-output.
Dit is dezelfde stap die de CD-workflow gebruikt; je hoeft dus niets handmatig
te kopiëren:

```bash
dotnet publish ./HaddySimHub -r win-x64 --output ./dist
# ./dist/wwwroot bevat nu de productie-frontend
```

Een gewone `dotnet build` of `dotnet test` triggert dit bewust niet (blijft snel
en vereist geen Node). Skip de frontend-build expliciet met
`-p:BuildClientAppOnPublish=false` (bv. publishen op een machine zonder Node).
