#!/bin/bash
# Installs what a Linux cloud container needs to build, test and playtest the ruleset:
# the .NET 10 SDK (from Ubuntu's archive), Mesa software OpenGL, Xvfb and ImageMagick.
# Idempotent: installs only what is missing, and skips the NuGet restore when no project file changed.
# Intended to be registered as a Claude Code SessionStart hook (see docs/ortho/AGENT.md).
set -euo pipefail

if [ "${CLAUDE_CODE_REMOTE:-}" != "true" ] && [ "${1:-}" != "--force" ]; then
  exit 0
fi

cd "${CLAUDE_PROJECT_DIR:-$(git rev-parse --show-toplevel)}"

missing=()
command -v dotnet >/dev/null 2>&1 || missing+=(dotnet-sdk-10.0)
command -v xvfb-run >/dev/null 2>&1 || missing+=(xvfb)
command -v montage >/dev/null 2>&1 || missing+=(imagemagick)
for pkg in libgl1-mesa-dri libegl1 libgles2 libx11-6 libxrandr2 libxcursor1 libxi6 libxkbcommon0; do
  dpkg -s "$pkg" >/dev/null 2>&1 || missing+=("$pkg")
done
if [ ${#missing[@]} -gt 0 ]; then
  apt-get update -qq
  DEBIAN_FRONTEND=noninteractive apt-get install -y -qq "${missing[@]}" >/dev/null
fi

nuget_dir="${NUGET_PACKAGES:-$HOME/.nuget/packages}"
mkdir -p "$nuget_dir"
hashes="$nuget_dir/.ortho-cloud-setup.sha256"
if ! sha256sum --check --status "$hashes" 2>/dev/null; then
  rm -f "$hashes"
  dotnet restore osu.Game.Rulesets.ManiaOrtho.Tests/osu.Game.Rulesets.ManiaOrtho.Tests.csproj
  sha256sum scripts/ortho/cloud-setup.sh global.json Directory.Build.props \
    osu.Game/osu.Game.csproj osu.Game.Rulesets.Mania/osu.Game.Rulesets.Mania.csproj \
    osu.Game.Rulesets.ManiaOrtho/osu.Game.Rulesets.ManiaOrtho.csproj \
    osu.Game.Rulesets.ManiaOrtho.Tests/osu.Game.Rulesets.ManiaOrtho.Tests.csproj > "$hashes" || rm -f "$hashes"
fi

if [ -n "${CLAUDE_ENV_FILE:-}" ]; then
  echo "export DOTNET_CLI_TELEMETRY_OPTOUT=1" >> "$CLAUDE_ENV_FILE"
fi
