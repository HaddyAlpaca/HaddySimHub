# Issue-queue als cronjob (met ntfy.sh-notificaties)

Deze scripts runnen de issue-gedreven queue onbeheerd via cron:

| Script | Wat het doet |
| --- | --- |
| `sync-issues.sh` | Haalt GitHub-issues op en schrijft één markdown-taak per issue naar `.queue/pending/` |
| `process-pending.sh` | Draait `opencode` op elke taak, maakt een branch + PR en verplaatst taken naar `in-progress`/`done`/`failed` |
| `cron-run.sh` | Wrapper voor cron: draait `sync-issues.sh` (en optioneel `process-pending.sh`) en stuurt een **ntfy.sh-notificatie** bij fouten of een samenvatting bij succes |

Logs staan in `.queue/logs/<stap>-<timestamp>.log` (`.queue/` staat in `.gitignore`).

## Vereisten

- `gh` geauthenticeerd (bv. `gh auth login`)
- `curl` (voor ntfy.sh-notificaties)
- `opencode` op `$PATH` (alleen voor `process-pending.sh`)
- Uitvoerbare scripts (`chmod +x scripts/queue/*.sh`)

## ntfy.sh instellen

1. Installeer de ntfy-app op je telefoon ([ntfy.sh/docs/subscribe/phone/](https://docs.ntfy.sh/subscribe/phone/)) en abonneer je op een topic, bijv. `haddy-queue`:

   ```
   termux-notifications install
   ```

   Of vul in de app: `haddy-queue` (of `https://ntfy.sh/haddy-queue`).

2. Kies een eigen topicnaam zodat anderen hem niet kunnen raden — je kunt een toegangstoken koppelen ([securing](https://docs.ntfy.sh/publish/#authentication)).

## Cronjob configureren

`cron-run.sh` wordt volledig via omgevingsvariabelen geconfigureerd:

| Variabele | Standaard | Betekenis |
| --- | --- | --- |
| `NTFY_TOPIC` | — (uit) | ntfy.de topic; zonder dit wordt er niets verzonden |
| `NTFY_SERVER` | `https://ntfy.sh` | ntfy-server |
| `NTFY_FAILURE_PRIORITY` | `high` | Prioriteit bij fouten |
| `NTFY_SUCCESS_PRIORITY` | `default` | Prioriteit bij succes |
| `NTFY_NOTIFY_FAILURE` | `1` | `0` = geen notificatie bij fouten |
| `NTFY_NOTIFY_SUCCESS` | `1` | `0` = geen notificatie bij succes |
| `SYNC_ISSUES` | `1` | `0` = sync-stap overslaan |
| `PROCESS_PENDING` | `0` | `1` = ook `process-pending.sh` draaien |
| `QUEUE_DIR` | `.queue` | Alternatieve queuedirectory |

> Let op: `gh issue list` kan stil falen (verlopen token, rate limit). `sync-issues.sh` eindigt dan met een fout in plaats van `No issues to sync.`, zodat `cron-run.sh` via ntfy meldt dat er iets mis is.

### Voorbeeld: elk uur syncen

```bash
crontab -e
```

```cron
# elke minuut 0 van elk uur, dagelijks
0 * * * * cd /home/joost/code/HaddySimHub && NTFY_TOPIC=haddy-queue ./scripts/queue/cron-run.sh >> .queue/cron.log 2>&1
```

### Voorbeeld: elk uur syncen + 's avonds taken verwerken

```cron
0 * * * * cd /home/joost/code/HaddySimHub && NTFY_TOPIC=haddy-queue ./scripts/queue/cron-run.sh >> .queue/cron.log 2>&1
0 22 * * * cd /home/joost/code/HaddySimHub && NTFY_TOPIC=haddy-queue PROCESS_PENDING=1 ./scripts/queue/cron-run.sh >> .queue/cron.log 2>&1
```

### Crontab-tips

- Zet bovenin je crontab een `PATH` zodat `git`, `gh`, `opencode` en `curl` gevonden worden:
  ```cron
  PATH=/home/joost/.local/bin:/usr/local/bin:/usr/bin:/bin
  ```
- `cron` e-mailt output alleen naar het systeempercentage-account; door het loggen naar `.queue/cron.log` zie je alle output op één plek.
- Laat cron de output `>> file 2>&1` wegschrijven; **niet** direct doorsturen naar `ntfy` — `cron-run.sh` stuurt al geaggregeerde notificaties.

## Testen

Draai eerst handmatig om te controleren dat alles werkt (zonder notificatie):

```bash
NTFY_TOPIC= ./scripts/queue/cron-run.sh
```

Voor een echte notificatietest naar je eigen topic:

```bash
NTFY_TOPIC=haddy-queue ./scripts/queue/cron-run.sh
```

Flow-test: simuleer een mislukkende sync (slechte token), zodat je de fout-notificatie ziet:

```bash
GH_TOKEN=bogus NTFY_TOPIC=haddy-queue ./scripts/queue/cron-run.sh
```

## Alternatief: systemd-timer

Gebruik je liever een systemd-timer dan cron:

`~/.config/systemd/user/issue-queue.service`:

```ini
[Unit]
Description=HaddySimHub issue queue sync

[Service]
Environment=NTFY_TOPIC=haddy-queue
WorkingDirectory=/home/joost/code/HaddySimHub
ExecStart=/home/joost/code/HaddySimHub/scripts/queue/cron-run.sh
StandardOutput=append:/home/joost/code/HaddySimHub/.queue/cron.log
StandardError=append:/home/joost/code/HaddySimHub/.queue/cron.log
```

`~/.config/systemd/user/issue-queue.timer`:

```ini
[Unit]
Description=Run HaddySimHub issue queue sync hourly

[Timer]
OnCalendar=hourly
Persistent=true

[Install]
WantedBy=timers.target
```

Activeer:

```bash
systemctl --user daemon-reload
systemctl --user enable --now issue-queue.timer
systemctl --user list-timers issue-queue.timer
```

## Notificaties bij "belangrijke mededelingen"

`cron-run.sh` stuurt standaard een notificatie bij elke mislukte stap (fout + laatste logregels) en een korte samenvatting bij succes. Wil je ook gewaarschuwd worden bij taken die in de queue blijven liggen of herhaaldelijk falen, zet dan `PROCESS_PENDING=1` aan: falende taken komen in `.queue/failed/` en `process-pending.sh` telt die in zijn samenvatting.