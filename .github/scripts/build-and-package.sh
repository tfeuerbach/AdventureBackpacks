#!/usr/bin/env bash
# Restore NuGet packages, build Release DLL, produce Thunderstore zip.
# Expects .refs/ already present (see restore-valheim-refs.sh).
# Usage: build-and-package.sh <tag>
set -euo pipefail

TAG="${1:?tag required (e.g. v2.2.11)}"
ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
cd "$ROOT"

REFS_DIR="${REFS_DIR:-$ROOT/.refs}"
if [[ ! -d "$REFS_DIR/BepInEx" || ! -d "$REFS_DIR/Valheim" ]]; then
  echo "error: missing ${REFS_DIR}/BepInEx or Valheim — run restore-valheim-refs.sh first." >&2
  exit 1
fi

if command -v nuget >/dev/null 2>&1; then
  nuget restore AdventureBackpacks/packages.config -PackagesDirectory packages
elif command -v mono >/dev/null 2>&1 && [[ -f /usr/local/bin/nuget.exe || -f "$HOME/.local/bin/nuget.exe" ]]; then
  NUGET_EXE="${NUGET_EXE:-$(command -v nuget.exe || true)}"
  mono "$NUGET_EXE" restore AdventureBackpacks/packages.config -PackagesDirectory packages
else
  # nuget CLI from setup-nuget on Windows lands on PATH as nuget
  echo "error: nuget not found on PATH" >&2
  exit 1
fi

MSBUILD="${MSBUILD:-msbuild}"
if ! command -v "$MSBUILD" >/dev/null 2>&1; then
  if command -v xbuild >/dev/null 2>&1; then
    MSBUILD=xbuild
  else
    echo "error: msbuild not found on PATH" >&2
    exit 1
  fi
fi

# Normalize Windows paths for msbuild when running under Git Bash
REFS_PROP="$REFS_DIR"
DIST_PROP="$ROOT/dist"
MOD_PROP="$ROOT"
if command -v cygpath >/dev/null 2>&1; then
  REFS_PROP="$(cygpath -w "$REFS_DIR")"
  DIST_PROP="$(cygpath -w "$ROOT/dist")"
  MOD_PROP="$(cygpath -w "$ROOT")"
fi

"$MSBUILD" AdventureBackpacks/AdventureBackpacks.csproj \
  /restore:false \
  /p:Configuration=Release \
  /p:Platform=AnyCPU \
  /p:ReferencesRoot="$REFS_PROP" \
  /p:ReleaseRoot="$DIST_PROP" \
  /p:ModRoot="$MOD_PROP" \
  /p:SkipReleaseCopy=true \
  /v:m

DLL="$ROOT/AdventureBackpacks/bin/Release/AdventureBackpacks.dll"
if [[ ! -f "$DLL" ]]; then
  echo "error: build succeeded but DLL missing at $DLL" >&2
  exit 1
fi

chmod +x "$ROOT/.github/scripts/package-thunderstore.sh"
DLL_PATH="$DLL" OUT_DIR="$ROOT/dist" \
  "$ROOT/.github/scripts/package-thunderstore.sh" "$TAG"
