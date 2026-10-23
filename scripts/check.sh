#!/usr/bin/env bash
# Every check a PR must pass, as CI runs them: Release build (warnings as errors), all tests, Polish-text placement.
set -euo pipefail
cd "$(dirname "$0")/.."

dotnet build --configuration Release --verbosity quiet
dotnet test --no-build --configuration Release
python3 scripts/check-polish-text.py
echo "All checks passed."
