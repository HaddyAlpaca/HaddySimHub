#!/usr/bin/env bash
set -euo pipefail

REPO="$(gh repo view --json nameWithOwner -q .nameWithOwner 2>/dev/null || git config --get remote.origin.url | sed -E 's#.*[:/]([^/]+/[^/.]+)(\.git)?$#\1#')"
QUEUE_DIR="${QUEUE_DIR:-.queue}"
PENDING="$QUEUE_DIR/pending"
IN_PROGRESS="$QUEUE_DIR/in-progress"
DONE="$QUEUE_DIR/done"
FAILED="$QUEUE_DIR/failed"
ISSUE_STATE="${ISSUE_STATE:-open}"
LABEL_FILTER="${ISSUE_LABELS:-}"
LIMIT="${ISSUE_LIMIT:-100}"
DRY_RUN="${DRY_RUN:-0}"

mkdir -p "$PENDING" "$IN_PROGRESS" "$DONE" "$FAILED"

usage() {
  cat <<EOF
Sync GitHub issues into the local markdown queue.

Usage: $(basename "$0") [options]

Options:
  -r, --repo REPO    Sync issues from REPO (default: current repo)
  -s, --state S      Issue state to sync: open|closed|all (default: open)
  -l, --label L      Only sync issues with this label (repeatable, comma-separated)
  -n, --limit N      Maximum number of issues to fetch (default: 100)
  -d, --dry-run      Print what would be synced without writing files
  -h, --help         Show this help
EOF
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    -r|--repo) REPO="$2"; shift 2;;
    -s|--state) ISSUE_STATE="$2"; shift 2;;
    -l|--label) LABEL_FILTER="${LABEL_FILTER:+$LABEL_FILTER,}$2"; shift 2;;
    -n|--limit) LIMIT="$2"; shift 2;;
    -d|--dry-run) DRY_RUN=1; shift;;
    -h|--help) usage; exit 0;;
    *) echo "Unknown option: $1" >&2; usage; exit 1;;
  esac
done

flags=(--state "$ISSUE_STATE" --limit "$LIMIT")
[[ -n "$LABEL_FILTER" ]] && flags+=(--label "$LABEL_FILTER")

flag_output="$(gh issue list --repo "$REPO" "${flags[@]}" --json number,title,labels --jq '.[] | [.number, (.title|gsub("\t";" ")), ([.labels[].name]|join(","))] | @tsv' 2>&1)" || {
  echo "error: could not fetch issues from $REPO" >&2
  echo "$flag_output" >&2
  exit 1
}
mapfile -t issues < <(printf '%s\n' "$flag_output" | grep -v '^$')

if [[ ${#issues[@]} -eq 0 ]]; then
  echo "No issues to sync."
  exit 0
fi

synced=0
skipped=0

for entry in "${issues[@]}"; do
  IFS=$'\t' read -r num title labels <<< "$entry"

  slug="$(echo "$title" | tr '[:upper:]' '[:lower:]' | sed -E 's/[^a-z0-9]+/-/g; s/^-+|-+$//g')"
  slug="${slug:0:60}"
  [[ -n "$slug" ]] || slug="issue"

  filename="$(printf '%03d-%s.md' "$num" "$slug")"

  already=false
  for dir in "$PENDING" "$IN_PROGRESS" "$DONE" "$FAILED"; do
    if grep -rql "issue: $num$" "$dir" 2>/dev/null; then
      already=true
      break
    fi
  done

  if $already; then
    skipped=$((skipped + 1))
    echo "skip  #$num $title (already in queue)"
    continue
  fi

  if [[ "$DRY_RUN" -eq 1 ]]; then
    echo "would #$num $title -> $PENDING/$filename"
    continue
  fi

  body="$(gh issue view "$num" --repo "$REPO" --json body,url --jq '.body')"

  cat > "$PENDING/$filename" <<EOF
---
issue: $num
title: $title
branch: fix/issue-$num
labels: $labels
---

# $title

Source: $REPO#$num

---

$body
EOF

  echo "new   #$num $title -> $PENDING/$filename"
  synced=$((synced + 1))
done

if [[ "$DRY_RUN" -eq 1 ]]; then
  echo "Dry run: would sync $skipped skip (existing) + ${#issues[@]} total."
else
  echo "Done: $synced synced, $skipped already queued."
fi