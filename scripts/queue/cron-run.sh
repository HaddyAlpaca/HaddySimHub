#!/usr/bin/env bash
set -euo pipefail

# Run the issue queue as a cronjob and report status/failures via ntfy.sh.
#
# Configuration (environment variables):
#   NTFY_SERVER          Base URL of the ntfy server (default: https://ntfy.sh)
#   NTFY_TOPIC           Topic to publish notifications to (default: none, disables ntfy)
#   NTFY_FAILURE_PRIORITY ntfy priority for failures (default: high)
#   NTFY_SUCCESS_PRIORITY ntfy priority for success summaries (default: default)
#   NTFY_NOTIFY_FAILURE  1 = notify on failures (default: 1), 0 = off
#   NTFY_NOTIFY_SUCCESS  1 = notify on success  (default: 1), 0 = off
#   SYNC_ISSUES          1 = run sync-issues.sh (default: 1), 0 = skip
#   PROCESS_PENDING      1 = run process-pending.sh (default: 0), 0 = skip
#   QUEUE_DIR            Alternative queue directory (default: .queue)
#
# Logs are written to "$QUEUE_DIR/logs/<step>-<timestamp>.log".

ROOT="$(git rev-parse --show-toplevel)"
QUEUE_DIR="${QUEUE_DIR:-.queue}"
SYNC_ISSUES="${SYNC_ISSUES:-1}"
PROCESS_PENDING="${PROCESS_PENDING:-0}"
NTFY_SERVER="${NTFY_SERVER:-https://ntfy.sh}"
NTFY_TOPIC="${NTFY_TOPIC:-}"
NTFY_FAILURE_PRIORITY="${NTFY_FAILURE_PRIORITY:-high}"
NTFY_SUCCESS_PRIORITY="${NTFY_SUCCESS_PRIORITY:-default}"
NTFY_NOTIFY_FAILURE="${NTFY_NOTIFY_FAILURE:-1}"
NTFY_NOTIFY_SUCCESS="${NTFY_NOTIFY_SUCCESS:-1}"

LOG_DIR="$ROOT/$QUEUE_DIR/logs"
mkdir -p "$LOG_DIR"
STAMP="$(date +%Y%m%d-%H%M%S)"

usage() {
  cat <<EOF
Run the issue queue from a cronjob and report via ntfy.sh.

Usage: $(basename "$0") [options]

Options:
  -h, --help  Show this help

Configuration is done via environment variables (see header of this script
or README.md in the same directory).
EOF
}

[[ $# -eq 0 ]] || { usage; exit 1; }

notify() {
  [[ -n "$NTFY_TOPIC" ]] || return 0
  local title="$1" priority="$2" tag="$3" message="$4"
  curl -sf \
    -H "Title: $title" \
    -H "Priority: $priority" \
    -H "Tags: $tag" \
    -d "$message" \
    "$NTFY_SERVER/$NTFY_TOPIC" 2>/dev/null \
    || echo "warning: ntfy notification failed" >&2
}

summary() {
  local sync_log="${1:-}" proc_log="${2:-}"
  local lines=("issue-queue $(date '+%Y-%m-%d %H:%M')")
  if [[ -n "$sync_log" && -f "$sync_log" ]]; then
    local new_lines synced
    new_lines="$(grep -c '^new ' "$sync_log" || true)"
    synced="$(grep -o 'Done: [0-9]* synced' "$sync_log" | head -n1 || true)"
    lines+=("sync: $new_lines new${synced:+ ($synced)}")
  fi
  if [[ -n "$proc_log" && -f "$proc_log" ]]; then
    local churned
    churned="$(grep -c '^==>' "$proc_log" || true)"
    lines+=("process: $churned task(s) handled")
  fi
  printf '%s\n' "${lines[@]}"
}

ok=1
sync_log=""
proc_log=""

if [[ "$SYNC_ISSUES" == "1" ]]; then
  sync_log="$LOG_DIR/sync-$STAMP.log"
  echo "==> sync-issues.sh"
  if "$ROOT/scripts/queue/sync-issues.sh" > "$sync_log" 2>&1; then
    echo "    sync OK"
  else
    echo "    sync FAILED (see $sync_log)"
    ok=0
  fi
fi

if [[ "$PROCESS_PENDING" == "1" ]]; then
  proc_log="$LOG_DIR/process-$STAMP.log"
  echo "==> process-pending.sh"
  if "$ROOT/scripts/queue/process-pending.sh" > "$proc_log" 2>&1; then
    echo "    process OK"
  else
    echo "    process FAILED (see $proc_log)"
    ok=0
  fi
fi

if [[ "$ok" == "0" ]]; then
  if [[ "$NTFY_NOTIFY_FAILURE" == "1" ]]; then
    body="$(summary "$sync_log" "$proc_log")"
    for f in "${sync_log:-}" "${proc_log:-}"; do
      if [[ -n "$f" && -f "$f" ]] && ! grep -Eq 'Done:|Finished:' "$f"; then
        body+=$'\n\n'"--- $f ---"$'\n'"$(tail -n 15 "$f")"
      fi
    done
    notify "HaddySimHub issue-queue: step failed" "$NTFY_FAILURE_PRIORITY" "error" "$body"
  fi
  exit 1
fi

if [[ "$NTFY_NOTIFY_SUCCESS" == "1" ]] && [[ -n "$NTFY_TOPIC" ]]; then
  notify "HaddySimHub issue-queue: OK" "$NTFY_SUCCESS_PRIORITY" "white_check_mark" "$(summary "$sync_log" "$proc_log")"
fi