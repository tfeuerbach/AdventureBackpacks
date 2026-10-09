#!/usr/bin/env bash
# Create a fork GitHub release for an upstream tag, suffixed with lowercase "a".
# Example: upstream v2.2.11 → fork release tag v2.2.11a
set -euo pipefail

UPSTREAM_TAG="${1:?upstream tag required (e.g. v2.2.11)}"
UPSTREAM_REPO="${UPSTREAM_REPO:-Vapok/AdventureBackpacks}"
TARGET_COMMIT="${TARGET_COMMIT:-HEAD}"

# Avoid double-suffix if someone passes an already-forked tag.
if [[ "$UPSTREAM_TAG" == *a ]]; then
  FORK_TAG="$UPSTREAM_TAG"
  BASE_TAG="${UPSTREAM_TAG%a}"
else
  FORK_TAG="${UPSTREAM_TAG}a"
  BASE_TAG="$UPSTREAM_TAG"
fi

echo "Upstream tag: ${BASE_TAG}"
echo "Fork release tag: ${FORK_TAG}"
echo "Target commit: ${TARGET_COMMIT}"

if gh release view "$FORK_TAG" >/dev/null 2>&1; then
  echo "Release ${FORK_TAG} already exists; leaving it alone."
  echo "fork_tag=${FORK_TAG}" >> "${GITHUB_OUTPUT:-/dev/null}"
  exit 0
fi

UPSTREAM_URL="https://github.com/${UPSTREAM_REPO}/releases/tag/${BASE_TAG}"
NOTES="$(cat <<EOF
## Fork release \`${FORK_TAG}\`

Based on upstream **[${BASE_TAG}](${UPSTREAM_URL})** from \`${UPSTREAM_REPO}\`, plus this fork's **Open Alongside Container** side-panel feature.

- Upstream release notes: ${UPSTREAM_URL}
- Fork addition: backpack opens to the right of inventory; chests stay open when enabled
- Regression tests: \`dotnet test AdventureBackpacks.Tests/AdventureBackpacks.Tests.csproj\`

> Unofficial fork — not affiliated with the upstream maintainer. Prefer upstream unless you need dual-open.
EOF
)"

# Lightweight annotated tag on the synced main commit, then GitHub release.
if git rev-parse "$FORK_TAG" >/dev/null 2>&1; then
  echo "Git tag ${FORK_TAG} already exists locally."
else
  git tag -a "$FORK_TAG" "$TARGET_COMMIT" -m "Fork release ${FORK_TAG} (upstream ${BASE_TAG})"
fi

git push origin "$FORK_TAG" || true

gh release create "$FORK_TAG" \
  --title "AdventureBackpacks ${FORK_TAG}" \
  --notes "$NOTES" \
  --target "$TARGET_COMMIT"

echo "fork_tag=${FORK_TAG}" >> "${GITHUB_OUTPUT:-/dev/null}"
echo "Created release ${FORK_TAG}"
