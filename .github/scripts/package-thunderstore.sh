#!/usr/bin/env bash
# Build a Thunderstore / r2modman zip: {author}-{name}-{version}.zip
# Usage: package-thunderstore.sh <tag>   e.g. v2.2.11
set -euo pipefail

TAG="${1:?tag required (e.g. v2.2.11)}"
VERSION="${TAG#v}"
AUTHOR="${PACKAGE_AUTHOR:-tfeuerbach}"
NAME="${PACKAGE_NAME:-AdventureBackpacks}"
ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
DLL="${DLL_PATH:-$ROOT/AdventureBackpacks/bin/Release/AdventureBackpacks.dll}"
OUT_DIR="${OUT_DIR:-$ROOT/dist}"
STAGE="$OUT_DIR/ts-stage"
ZIP_NAME="${AUTHOR}-${NAME}-${VERSION}.zip"
ZIP_PATH="$OUT_DIR/$ZIP_NAME"

resolve_python() {
  if [[ -n "${PYTHON:-}" ]]; then
    echo "$PYTHON"
    return
  fi
  if command -v python3 >/dev/null 2>&1; then command -v python3; return; fi
  if command -v python >/dev/null 2>&1; then command -v python; return; fi
  if command -v py >/dev/null 2>&1; then echo "py -3"; return; fi
  for candidate in /c/Python3*/python.exe /c/Users/*/AppData/Local/Programs/Python/Python3*/python.exe; do
    if [[ -x "$candidate" ]]; then echo "$candidate"; return; fi
  done
  echo "error: python not found (set PYTHON=...)" >&2
  exit 1
}
PYTHON="$(resolve_python)"

if [[ ! -f "$DLL" ]]; then
  echo "error: DLL not found at $DLL" >&2
  echo "Build Release first, or set DLL_PATH." >&2
  exit 1
fi

rm -rf "$STAGE"
mkdir -p "$STAGE/plugins/Translations"

# shellcheck disable=SC2086
$PYTHON - <<PY
import json
import pathlib
import shutil

root = pathlib.Path(r"$ROOT")
stage = pathlib.Path(r"$STAGE")
data = json.loads((root / "manifest.json").read_text(encoding="utf-8"))
data["name"] = "$NAME"
data["author"] = "$AUTHOR"
data["version_number"] = "$VERSION"
data["website_url"] = "https://github.com/tfeuerbach/AdventureBackpacks"
(stage / "manifest.json").write_text(json.dumps(data, indent=2) + "\n", encoding="utf-8")

for name in ("icon.png", "README.md", "CHANGELOG.md", "LICENSE.md", "LICENSE"):
    src = root / name
    if src.is_file():
        shutil.copy2(src, stage / ("LICENSE.md" if name == "LICENSE" else name))

shutil.copy2(r"$DLL", stage / "plugins" / "AdventureBackpacks.dll")
translations = root / "AdventureBackpacks" / "Translations"
if translations.is_dir():
    for src in translations.glob("*.json"):
        shutil.copy2(src, stage / "plugins" / "Translations" / src.name)
PY

mkdir -p "$OUT_DIR"
rm -f "$ZIP_PATH"

# shellcheck disable=SC2086
$PYTHON - <<PY
import pathlib
import zipfile

stage = pathlib.Path(r"$STAGE")
zip_path = pathlib.Path(r"$ZIP_PATH")
with zipfile.ZipFile(zip_path, "w", compression=zipfile.ZIP_DEFLATED) as zf:
    for path in sorted(stage.rglob("*")):
        if path.is_file():
            zf.write(path, path.relative_to(stage).as_posix())
print(f"Wrote {zip_path} ({zip_path.stat().st_size} bytes)")
PY

echo "zip_path=${ZIP_PATH}" >> "${GITHUB_OUTPUT:-/dev/null}"
echo "zip_name=${ZIP_NAME}" >> "${GITHUB_OUTPUT:-/dev/null}"
echo "version=${VERSION}" >> "${GITHUB_OUTPUT:-/dev/null}"
echo "Packaged ${ZIP_PATH}"
