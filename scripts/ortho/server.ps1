# Runs your own maniaortho score server on this computer, for the game's "Send plays to my score server" setting.
#   powershell -ExecutionPolicy Bypass -File scripts\ortho\server.ps1
#   powershell -ExecutionPolicy Bypass -File scripts\ortho\server.ps1 -Port 7728 -NoLiveCheck
# Then open http://127.0.0.1:7727/ for your unofficial pp, best plays and latest plays. Ctrl+C stops it.
#
# It listens on this computer only (127.0.0.1) and rates plays with osu!'s published osu!mania calculators, at the version
# osu!'s live score processor uses (ortho-server\calculator-version.txt). It never contacts osu!'s servers: the only
# outside requests read which version osu! runs, from GitHub (ppy/osu-queue-score-statistics, public), at start and every
# 12 hours; -NoLiveCheck turns that off. When osu! moves to a new version, run server-update.ps1.
# Its data (plays.db, the map files and replays of your plays) is kept in ..\ortho-scores, next to the clone.
param(
    [string]$Data,
    [int]$Port = 7727,
    [switch]$NoLiveCheck,
    [switch]$NoBuild
)
$ErrorActionPreference = "Stop"
Set-Location (git rev-parse --show-toplevel)
$root = (Get-Location).Path
. (Join-Path $PSScriptRoot "windows-env.ps1")

if (-not $Data) { $Data = Join-Path (Split-Path $root -Parent) "ortho-scores" }
$Data = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($Data)

$arguments = @("run", "--project", "ortho-server\OrthoServer", "-c", "Release")
if ($NoBuild) { $arguments += "--no-build" }
$arguments += @("--", "serve", "--data", $Data, "--port", $Port)
if ($NoLiveCheck) { $arguments += "--no-live-check" }

& dotnet @arguments
exit $LASTEXITCODE
