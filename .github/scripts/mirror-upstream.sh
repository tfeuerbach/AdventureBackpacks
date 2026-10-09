#!/usr/bin/env bash
# Mirror the latest Vapok/AdventureBackpacks release into this fork.
# - Conflicts outside protected paths: take upstream automatically
# - Conflicts inside protected paths: stop for human review
# - Clean merge / auto-resolved: run tests; auto-finish only if green
set -euo pipefail

UPSTREAM_REPO="${UPSTREAM_REPO:-Vapok/AdventureBackpacks}"
PROTECTED_FILE="${PROTECTED_FILE:-.github/fork-protected-paths.txt}"
STATE_FILE="${STATE_FILE:-.github/upstream-sync-state.json}"
DEFAULT_BRANCH="${DEFAULT_BRANCH:-main}"

is_protected() {
  local path="$1"
  local line
  while IFS= read -r line || [[ -n "$line" ]]; do
    # strip comments / blanks
    line="${line%%#*}"
    line="$(echo "$line" | sed 's/[[:space:]]*$//;s/^[[:space:]]*//')"
    [[ -z "$line" ]] && continue

    if [[ "$line" == */ ]]; then
      [[ "$path" == "$line"* ]] && return 0
    else
      [[ "$path" == "$line" || "$path" == "$line"/* ]] && return 0
    fi
  done < "$PROTECTED_FILE"
  return 1
}

echo "::group::Detect upstream release"
if [[ -n "${UPSTREAM_TAG_OVERRIDE:-}" ]]; then
  LATEST_TAG="$UPSTREAM_TAG_OVERRIDE"
  LATEST_URL="https://github.com/${UPSTREAM_REPO}/releases/tag/${LATEST_TAG}"
  echo "Using override tag: ${LATEST_TAG}"
else
  LATEST_TAG="$(gh api "repos/${UPSTREAM_REPO}/releases/latest" --jq .tag_name)"
  LATEST_URL="$(gh api "repos/${UPSTREAM_REPO}/releases/latest" --jq .html_url)"
fi
echo "Latest upstream release: ${LATEST_TAG}"
echo "Release URL: ${LATEST_URL}"

LAST_TAG=""
if [[ -f "$STATE_FILE" ]]; then
  LAST_TAG="$(python -c "import json;print(json.load(open('${STATE_FILE}')).get('last_synced_tag',''))" 2>/dev/null || true)"
fi
echo "Last synced tag: ${LAST_TAG:-<none>}"
echo "::endgroup::"

{
  echo "latest_tag=${LATEST_TAG}"
  echo "latest_url=${LATEST_URL}"
  echo "last_tag=${LAST_TAG}"
} >> "$GITHUB_OUTPUT"

if [[ "${FORCE_SYNC:-}" != "true" && -n "$LAST_TAG" && "$LATEST_TAG" == "$LAST_TAG" ]]; then
  echo "already_synced=true" >> "$GITHUB_OUTPUT"
  echo "Already synced to ${LATEST_TAG}; nothing to do."
  exit 0
fi
echo "already_synced=false" >> "$GITHUB_OUTPUT"

SYNC_BRANCH="sync/upstream-${LATEST_TAG}"
echo "sync_branch=${SYNC_BRANCH}" >> "$GITHUB_OUTPUT"

echo "::group::Fetch & merge ${LATEST_TAG}"
git fetch origin "$DEFAULT_BRANCH"
git checkout -B "$SYNC_BRANCH" "origin/${DEFAULT_BRANCH}"

git remote remove upstream 2>/dev/null || true
git remote add upstream "https://github.com/${UPSTREAM_REPO}.git"
git fetch upstream "refs/tags/${LATEST_TAG}:refs/tags/upstream-${LATEST_TAG}" --force

MERGE_STATUS="clean"
PROTECTED_CONFLICTS=()
AUTO_RESOLVED=()

set +e
git merge "upstream-${LATEST_TAG}" --no-edit --no-ff -m "chore: merge upstream ${LATEST_TAG}"
MERGE_EXIT=$?
set -e

if [[ $MERGE_EXIT -ne 0 ]]; then
  mapfile -t CONFLICTS < <(git diff --name-only --diff-filter=U || true)
  echo "Merge reported conflicts in ${#CONFLICTS[@]} file(s)."

  for f in "${CONFLICTS[@]}"; do
    if is_protected "$f"; then
      echo "PROTECTED conflict: $f"
      PROTECTED_CONFLICTS+=("$f")
    else
      echo "Auto-taking upstream for: $f"
      # merge: ours = fork HEAD, theirs = upstream
      git checkout --theirs -- "$f"
      git add -- "$f"
      AUTO_RESOLVED+=("$f")
    fi
  done

  if ((${#PROTECTED_CONFLICTS[@]} > 0)); then
    MERGE_STATUS="protected_conflict"
    # Leave conflict markers for the PR; ensure branch is pushable.
    git add -u || true
  else
    git commit --no-edit -m "chore: merge upstream ${LATEST_TAG} (auto-resolved non-protected conflicts)"
    MERGE_STATUS="auto_resolved"
  fi
fi
echo "::endgroup::"

{
  echo "merge_status=${MERGE_STATUS}"
  echo "protected_conflicts<<EOF"
  printf '%s\n' "${PROTECTED_CONFLICTS[@]:-}"
  echo "EOF"
  echo "auto_resolved<<EOF"
  printf '%s\n' "${AUTO_RESOLVED[@]:-}"
  echo "EOF"
} >> "$GITHUB_OUTPUT"

if [[ "$MERGE_STATUS" == "protected_conflict" ]]; then
  echo "needs_human=true" >> "$GITHUB_OUTPUT"
  echo "reason=protected_conflict" >> "$GITHUB_OUTPUT"
  exit 0
fi

# Sync state is updated only after tests pass (see mirror-upstream.yml),
# so a failed test run does not mark the tag as done.
echo "needs_human=false" >> "$GITHUB_OUTPUT"
echo "reason=ready_for_tests" >> "$GITHUB_OUTPUT"
