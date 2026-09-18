# Running the issue queue as a cronjob (with ntfy.sh notifications)

These scripts run the issue-driven queue unattended via cron:

| Script | What it does |
| --- | --- |
| `sync-issues.sh` | Fetches GitHub issues and writes one markdown task per issue to `.queue/pending/` |
| `process-pending.sh` | Runs `opencode` on each task, creates a branch + PR, and moves tasks through `in-progress`/`done`/`failed` |
| `cron-run.sh` | Cron wrapper: runs `sync-issues.sh` (and optionally `process-pending.sh`) and sends an **ntfy.sh notification** on failures or a summary on success |

Logs live in `.queue/logs/<step>-<timestamp>.log` (`.queue/` is in `.gitignore`).

## Requirements

- `gh` authenticated (e.g. `gh auth login`)
- `curl` (for ntfy.sh notifications)
- `opencode` on `$PATH` (only for `process-pending.sh`)
- Executable scripts (`chmod +x scripts/queue/*.sh`)

## Setting up ntfy.sh

1. Install the ntfy app on your phone ([ntfy.sh/docs/subscribe/phone/](https://docs.ntfy.sh/subscribe/phone/)) and subscribe to a topic, e.g. `haddy-queue`:

   ```
   termux-notifications install
   ```

   Or in the app, enter: `haddy-queue` (or `https://ntfy.sh/haddy-queue`).

2. Pick a unique topic name so others can't guess it — you can attach an access token ([securing](https://docs.ntfy.sh/publish/#authentication)).

## Configuring the cronjob

`cron-run.sh` is configured entirely via environment variables:

| Variable | Default | Meaning |
| --- | --- | --- |
| `NTFY_TOPIC` | — (off) | The ntfy.sh topic; without it no notifications are sent |
| `NTFY_SERVER` | `https://ntfy.sh` | The ntfy server |
| `NTFY_FAILURE_PRIORITY` | `high` | Priority for failures |
| `NTFY_SUCCESS_PRIORITY` | `default` | Priority for success summaries |
| `NTFY_NOTIFY_FAILURE` | `1` | `0` = disable failure notifications |
| `NTFY_NOTIFY_SUCCESS` | `1` | `0` = disable success notifications |
| `SYNC_ISSUES` | `1` | `0` = skip the sync step |
| `PROCESS_PENDING` | `0` | `1` = also run `process-pending.sh` |
| `QUEUE_DIR` | `.queue` | Alternative queue directory |

> Note: `gh issue list` can fail silently (expired token, rate limit). `sync-issues.sh` then exits with an error instead of printing `No issues to sync.`, so `cron-run.sh` reports the problem via ntfy.

### Example: sync every hour

```bash
crontab -e
```

```cron
# at minute 0 of every hour, every day
0 * * * * cd /home/joost/code/HaddySimHub && NTFY_TOPIC=haddy-queue ./scripts/queue/cron-run.sh >> .queue/cron.log 2>&1
```

### Example: sync hourly + process tasks in the evening

```cron
0 * * * * cd /home/joost/code/HaddySimHub && NTFY_TOPIC=haddy-queue ./scripts/queue/cron-run.sh >> .queue/cron.log 2>&1
0 22 * * * cd /home/joost/code/HaddySimHub && NTFY_TOPIC=haddy-queue PROCESS_PENDING=1 ./scripts/queue/cron-run.sh >> .queue/cron.log 2>&1
```

### Crontab tips

- Set a `PATH` at the top of your crontab so `git`, `gh`, `opencode` and `curl` are found:
  ```cron
  PATH=/home/joost/.local/bin:/usr/local/bin:/usr/bin:/bin
  ```
- `cron` only emails output to the system account; logging to `.queue/cron.log` keeps all output in one place.
- Have cron write output to `>> file 2>&1`; **do not** pipe it straight to `ntfy` — `cron-run.sh` already sends aggregated notifications.

## Testing

First run manually to check everything works (no notification):

```bash
NTFY_TOPIC= ./scripts/queue/cron-run.sh
```

For a real notification to your own topic:

```bash
NTFY_TOPIC=haddy-queue ./scripts/queue/cron-run.sh
```

Flow test: simulate a failing sync (bad token) so you can see the failure notification:

```bash
GH_TOKEN=bogus NTFY_TOPIC=haddy-queue ./scripts/queue/cron-run.sh
```

## Alternative: systemd timer

Prefer a systemd timer over cron:

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

Enable it:

```bash
systemctl --user daemon-reload
systemctl --user enable --now issue-queue.timer
systemctl --user list-timers issue-queue.timer
```

## Notifications for "important announcements"

`cron-run.sh` sends a notification on every failed step (error + last log lines) and a short summary on success, by default. If you also want to be alerted when tasks sit in the queue or keep failing, enable `PROCESS_PENDING=1`: failed tasks end up in `.queue/failed/` and `process-pending.sh` counts them in its summary.