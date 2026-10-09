#!/usr/bin/env bash
# Download and extract Valheim/BepInEx/APIManager reference assemblies into .refs/
# Requires secret/env VALHEIM_REFS_URL pointing at a zip whose root is either:
#   BepInEx/ Valheim/ APIManager/
# or a single top-level folder containing those directories.
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
REFS_DIR="${REFS_DIR:-$ROOT/.refs}"
URL="${VALHEIM_REFS_URL:-}"

if [[ -z "$URL" ]]; then
  echo "error: VALHEIM_REFS_URL is not set." >&2
  echo "Add a repository secret with a URL to a zip of the .refs tree (BepInEx, Valheim, APIManager)." >&2
  exit 1
fi

TMP="$(mktemp -d)"
trap 'rm -rf "$TMP"' EXIT

resolve_python() {
  if [[ -n "${PYTHON:-}" ]]; then echo "$PYTHON"; return; fi
  if command -v python3 >/dev/null 2>&1; then command -v python3; return; fi
  if command -v python >/dev/null 2>&1; then command -v python; return; fi
  if command -v py >/dev/null 2>&1; then echo "py -3"; return; fi
  for candidate in /c/Python3*/python.exe; do
    if [[ -x "$candidate" ]]; then echo "$candidate"; return; fi
  done
  echo "error: python not found (set PYTHON=...)" >&2
  exit 1
}
PYTHON="$(resolve_python)"

echo "Downloading Valheim refs..."
curl -fsSL "$URL" -o "$TMP/refs.zip"
EXTRACT="$TMP/extract"
mkdir -p "$EXTRACT"
# shellcheck disable=SC2086
$PYTHON - <<PY
import zipfile
zipfile.ZipFile(r"$TMP/refs.zip").extractall(r"$EXTRACT")
PY

# Normalize so REFS_DIR contains BepInEx/, Valheim/, APIManager/
SRC="$EXTRACT"
if [[ ! -d "$SRC/BepInEx" && ! -d "$SRC/Valheim" ]]; then
  # one top-level directory
  children=("$SRC"/*)
  if [[ ${#children[@]} -eq 1 && -d "${children[0]}" ]]; then
    SRC="${children[0]}"
  fi
fi

if [[ ! -d "$SRC/BepInEx" || ! -d "$SRC/Valheim" ]]; then
  echo "error: zip must contain BepInEx/ and Valheim/ directories." >&2
  ls -la "$SRC" >&2 || true
  exit 1
fi

rm -rf "$REFS_DIR"
mkdir -p "$REFS_DIR"
cp -a "$SRC"/. "$REFS_DIR"/
echo "Restored refs to ${REFS_DIR}"
ls -la "$REFS_DIR"
