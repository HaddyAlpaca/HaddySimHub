#!/usr/bin/env bash
set -euo pipefail

REPO="$(git config --get remote.origin.url | sed -E 's#.*[:/]([^/]+/[^/.]+)(\.git)?$#\1#')"
QUEUE_DIR="${QUEUE_DIR:-.queue}"
PENDING="$QUEUE_DIR/pending"
IN_PROGRESS="$QUEUE_DIR/in-progress"
DONE="$QUEUE_DIR/done"
FAILED="$QUEUE_DIR/failed"
BASE_BRANCH="${BASE_BRANCH:-main}"
MODEL="${OPENCODE_MODEL:-}"
DRY_RUN="${DRY_RUN:-0}"
OPENCODE_TIMEOUT="${OPENCODE_TIMEOUT:-1200000}"

mkdir -p "$PENDING" "$IN_PROGRESS" "$DONE" "$FAILED"

usage() {
  cat <<EOF
Process pending queue items: run opencode on each task, create a branch and a PR.

Usage: $(basename "$0") [options]

Options:
  -m, --model M      LLM model for opencode (default: opencode's default)
  -b, --base B       Base branch for PRs (default: main)
  -d, --dry-run      Show what would happen without running opencode
  -h, --help         Show this help
EOF
}

fail() {
  local task="$1" reason="$2"
  mv "$task" "$FAILED/"
  echo "failed: $(basename "$task") -> $FAILED/ ($reason)"
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    -m|--model) MODEL="$2"; shift 2;;
    -b|--base) BASE_BRANCH="$2"; shift 2;;
    -d|--dry-run) DRY_RUN=1; shift;;
    -h|--help) usage; exit 0;;
    *) echo "Unknown option: $1" >&2; usage; exit 1;;
  esac
done

read_frontmatter() {
  awk '
    /^---$/ { n++; next }
    n == 1 { print }
    n == 2 { exit }
  ' "$1"
}

process_task() {
  local task="$1"
  local task_name title branch labels

  task_name="$(basename "$task" .md)"
  title="$(read_frontmatter "$task" | awk '/^title:/ { sub(/^title: /, ""); print; exit }')"
  branch="$(read_frontmatter "$task" | awk '/^branch:/ { sub(/^branch: /, ""); print; exit }')"
  labels="$(read_frontmatter "$task" | awk '/^labels:/ { sub(/^labels: /, ""); print; exit }')"

  [[ -n "$title" ]] || title="$task_name"
  [[ -n "$branch" ]] || branch="bot/$task_name"

  echo "==> $task_name ($title)"
  echo "    branch: $branch"
  echo "    base:   $BASE_BRANCH"
  [[ -n "$MODEL" ]] && echo "    model:  $MODEL"

  mv "$task" "$IN_PROGRESS/"
  local working="$IN_PROGRESS/$task_name.md"

  if [[ "$DRY_RUN" -eq 1 ]]; then
    echo "    [dry-run] skipping opencode + git, task stays in $IN_PROGRESS/"
    return
  fi

  trap '[[ -f "$working" ]] && mv "$working" "$FAILED/"' ERR
  git fetch origin "$BASE_BRANCH" -q
  git checkout -B "$branch" "origin/$BASE_BRANCH"

  local prompt="$(awk '/^---$/{n++;next} n>=1{print}' "$working")"

  echo "    running opencode..."
  local args=()
  [[ -n "$MODEL" ]] && args+=(--model "$MODEL")
  if ! timeout "$OPENCODE_TIMEOUT" opencode run "${args[@]}" "$prompt"; then
    trap - ERR
    fail "$working" "opencode exited non-zero"
    git checkout "$BASE_BRANCH" 2>/dev/null || true
    return
  fi
  trap - ERR

  if git diff --quiet && git diff --cached --quiet; then
    fail "$working" "no changes produced"
    git checkout "$BASE_BRANCH" 2>/dev/null || true
    return
  fi

  git add -A
  git commit -m "feat: $title" -m "Closes via issue-driven queue task: $(basename "$working")"
  git push -u origin "$branch"

  local pr_args=(--base "$BASE_BRANCH" --head "$branch" --title "$title" --body "$(awk '/^---$/{n++;next} n>=1{print}' "$working")")
  [[ -n "$labels" ]] && pr_args+=(--label "$labels")

  gh pr create "${pr_args[@]}"
  mv "$working" "$DONE/"
  echo "    done: $(basename "$working") -> $DONE/"

  git checkout "$BASE_BRANCH"
}

shopt -s nullglob
files=("$PENDING"/*.md)

if [[ ${#files[@]} -eq 0 ]]; then
  echo "Queue is empty."
  exit 0
fi

for task in "${files[@]}"; do
  name="$(basename "$task")"
  process_task "$task"
done

echo "Finished. $(ls "$DONE" 2>/dev/null | wc -l) in done/, $(ls "$FAILED" 2>/dev/null | wc -l) in failed/."