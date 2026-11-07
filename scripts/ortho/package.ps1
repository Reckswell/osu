# Packages the full game with maniaortho for playing: double-click osu.exe.
#   powershell -ExecutionPolicy Bypass -File scripts\ortho\package.ps1
#
# Layout, next to the clone (Documents\Projects\osu!):
#   osu\osu.exe     the launcher (ManiaOrtho.Launcher): keeps the data in ortho-data and temp files in osu\temp, moves
#                   screenshots into osu!stable's Screenshots folder after each run
#   osu\game\       the game (osu.Desktop, Debug: it only knows osu!'s development server)
#   ortho-data\     the game's data, shared with scripts\ortho\play.ps1; kept when packaging again
# Run it again after pulling changes; the game must be closed. See docs/ortho/PLAYTESTING.md, "L4".
param(
    # The package folder. Default: osu next to the clone.
    [string]$Out
)
$ErrorActionPreference = "Stop"
Set-Location (git rev-parse --show-toplevel)
$root = (Get-Location).Path
. (Join-Path $PSScriptRoot "windows-env.ps1")

if (-not $Out) { $Out = Join-Path (Split-Path $root -Parent) "osu" }
$Out = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($Out)
$game = Join-Path $Out "game"
$staging = Join-Path $Out "game.new"

$running = Get-Process -Name "osu!" -ErrorAction SilentlyContinue |
    Where-Object { $_.Path -and $_.Path.StartsWith($game + "\", [StringComparison]::OrdinalIgnoreCase) }
if ($running) { Write-Host "The packaged game is running; close it and run this again."; exit 1 }

# Windows PowerShell 5.1 turns a native command's redirected stderr into errors, which "Stop" makes fatal: relax it
# around dotnet and check the exit code instead. No build servers: ones left from an earlier build would keep that
# build's temp folder and settings.
function Invoke-Dotnet([string]$What, [string[]]$Arguments) {
    Write-Host "== $What"
    $ErrorActionPreference = "Continue"
    $output = & dotnet @Arguments -v q -nologo --disable-build-servers 2>&1
    $code = $LASTEXITCODE
    $ErrorActionPreference = "Stop"
    if ($code -ne 0) {
        $output | Select-Object -Last 20 | ForEach-Object { "   $_" }
        Write-Host "$What FAILED"
        exit 1
    }
}

New-Item -ItemType Directory -Force -Path $Out | Out-Null
if (Test-Path -LiteralPath $staging) { Remove-Item -LiteralPath $staging -Recurse -Force }

# Built beside the old one and swapped in at the end, so a failed build leaves the last package playable.
Invoke-Dotnet "game (osu.Desktop, Debug)" @("publish", "osu.Desktop", "-c", "Debug", "-o", $staging)
Invoke-Dotnet "launcher (osu.exe)" @("publish", "ManiaOrtho.Launcher", "-c", "Release", "-r", "win-x64",
    "-p:SelfContained=false", "-p:PublishSingleFile=true", "-o", $Out)

# Only built files and the launcher's two ini files live in game\ (the data is in ortho-data), so it is replaced whole.
if (Test-Path -LiteralPath $game) { Remove-Item -LiteralPath $game -Recurse -Force }
Rename-Item -LiteralPath $staging -NewName "game"

$commit = git log -1 --format="%h %s"
Set-Content -LiteralPath (Join-Path $Out "version.txt") -Value "Packaged $(Get-Date -Format 'yyyy-MM-dd HH:mm') from $commit"
Write-Host ""
Write-Host "Done ($commit). Play: double-click $(Join-Path $Out 'osu.exe')"
