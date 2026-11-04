#!/bin/bash
# Runs your own maniaortho score server on this computer (see server.ps1 for what it does and does not contact).
#   scripts/ortho/server.sh [--data <folder>] [--port <port>] [--no-live-check]
# Data defaults to ../ortho-scores, next to the clone. Open http://127.0.0.1:7727/ for the score page.
set -euo pipefail
cd "$(git rev-parse --show-toplevel)"

data="$(cd .. && pwd)/ortho-scores"
args=()
while [ $# -gt 0 ]; do
  case "$1" in
    --data) data="$2"; shift ;;
    *) args+=("$1") ;;
  esac
  shift
done

exec dotnet run --project ortho-server/OrthoServer -c Release -- serve --data "$data" ${args[@]+"${args[@]}"}
