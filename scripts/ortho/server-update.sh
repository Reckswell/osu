#!/bin/bash
# Moves the score server to the osu!mania calculator version osu! runs live, then runs its tests (see server-update.ps1).
set -euo pipefail
cd "$(git rev-parse --show-toplevel)"

file=ortho-server/calculator-version.txt
before="$(tr -d '[:space:]' < "$file")"
dotnet run --project ortho-server/OrthoServer -c Release -- check-live --write "$file"
after="$(tr -d '[:space:]' < "$file")"

if [ "$after" = "$before" ]; then
  echo "Already on osu!'s live version ($after)."
  exit 0
fi

echo "== $before -> $after: rebuilding the server and running its tests"
if ! dotnet test ortho-server/OrthoServer.Tests -c Release -nologo; then
  echo "Some tests failed. If only 'Parity:' tests failed, osu! now rates differently from this fork's copy of the calculators;"
  echo "the server is right, the game's own pp is behind."
fi
echo "Restart scripts/ortho/server.sh to rate your plays again with $after."
