#!/usr/bin/env bash
# Create a fork GitHub release matching the upstream tag (same name, no suffix)
# and attach the Thunderstore zip when PACKAGE_ZIP is set.
# Example: upstream v2.2.11 → fork release v2.2.11 + tfeuerbach-AdventureBackpacks-2.2.11.zip
set -euo pipefail

UPSTREAM_TAG="${1:?upstream tag required (e.g. v2.2.11)}"
UPSTREAM_REPO="${UPSTREAM_REPO:-Vapok/AdventureBackpacks}"
TARGET_COMMIT="${TARGET_COMMIT:-HEAD}"
PACKAGE_ZIP="${PACKAGE_ZIP:-}"

# Same tag as upstream. Normalize accidental old "a" suffix (v2.2.11a → v2.2.11).
FORK_TAG="$UPSTREAM_TAG"
if [[ "$FORK_TAG" =~ ^v[0-9]+\.[0-9]+\.[0-9]+a$ ]]; then
  FORK_TAG="${FORK_TAG%a}"
fi
BASE_TAG="$FORK_TAG"

echo "Upstream / fork release tag: ${FORK_TAG}"
echo "Target commit: ${TARGET_COMMIT}"

ASSET_ARGS=()
if [[ -n "$PACKAGE_ZIP" ]]; then
  if [[ ! -f "$PACKAGE_ZIP" ]]; then
    echo "error: PACKAGE_ZIP not found: $PACKAGE_ZIP" >&2
    exit 1
  fi
  ASSET_ARGS=("$PACKAGE_ZIP")
  echo "Release asset: ${PACKAGE_ZIP}"
fi

if gh release view "$FORK_TAG" >/dev/null 2>&1; then
  echo "Release ${FORK_TAG} already exists."
  if [[ ${#ASSET_ARGS[@]} -gt 0 ]]; then
    ASSET_NAME="$(basename "${ASSET_ARGS[0]}")"
    if gh release view "$FORK_TAG" --json assets --jq '.assets[].name' | grep -Fxq "$ASSET_NAME"; then
      echo "Asset ${ASSET_NAME} already on release; leaving it alone."
    else
      echo "Uploading missing asset ${ASSET_NAME}..."
      gh release upload "$FORK_TAG" "${ASSET_ARGS[0]}" --clobber
    fi
  fi
  echo "fork_tag=${FORK_TAG}" >> "${GITHUB_OUTPUT:-/dev/null}"
  exit 0
fi

UPSTREAM_URL="https://github.com/${UPSTREAM_REPO}/releases/tag/${BASE_TAG}"
NOTES="$(cat <<EOF
## Fork release \`${FORK_TAG}\`

Based on upstream **[${BASE_TAG}](${UPSTREAM_URL})** from \`${UPSTREAM_REPO}\`, plus this fork's **Open Alongside Container** side-panel feature.

- Upstream release notes: ${UPSTREAM_URL}
- Fork addition: backpack opens to the right of inventory; chests stay open when enabled
- Install: download \`tfeuerbach-AdventureBackpacks-*.zip\` and import in r2modman / Thunderstore Mod Manager (or extract under BepInEx/plugins)
- Regression tests: \`dotnet test AdventureBackpacks.Tests/AdventureBackpacks.Tests.csproj\`

> Unofficial fork — not affiliated with the upstream maintainer. Prefer upstream unless you need dual-open.
EOF
)"

if git rev-parse "$FORK_TAG" >/dev/null 2>&1; then
  echo "Git tag ${FORK_TAG} already exists locally."
else
  git tag -a "$FORK_TAG" "$TARGET_COMMIT" -m "Fork release ${FORK_TAG} (upstream ${BASE_TAG})"
fi

git push origin "$FORK_TAG" || true

gh release create "$FORK_TAG" \
  "${ASSET_ARGS[@]}" \
  --title "AdventureBackpacks ${FORK_TAG}" \
  --notes "$NOTES" \
  --target "$TARGET_COMMIT"

echo "fork_tag=${FORK_TAG}" >> "${GITHUB_OUTPUT:-/dev/null}"
echo "Created release ${FORK_TAG}"
