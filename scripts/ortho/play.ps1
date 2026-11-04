# Runs the full osu! game (menus, song select, settings, replays) with maniaortho, built from this clone.
#   powershell -ExecutionPolicy Bypass -File scripts\ortho\play.ps1
#   powershell -ExecutionPolicy Bypass -File scripts\ortho\play.ps1 -NoBuild
#
# What it writes stays inside the folder that holds the clone (Documents\Projects\osu!):
#   - the game's data (database, settings, imported maps and skins, replays, logs) in ..\ortho-data, next to the clone;
#   - framework.ini and storage.ini next to the build: the first makes the game portable, so it never opens
#     %APPDATA%\osu (the release game's data) or %APPDATA%\osu-development; the second is osu!'s own "custom data
#     location" setting, pointing at ..\ortho-data;
#   - build caches and temp files in .ortho-cache (windows-env.ps1).
# osu!stable's files are used where they can be:
#   - maps, skins, scores and collections: osu!'s first-run "Import from osu!stable" (later: Settings > Maintenance).
#     On the same drive it creates hard links: no copies, no extra space, stable's files left as they are;
#   - screenshots: taken into ..\ortho-data\screenshots, then moved into stable's Screenshots folder when the game
#     closes (and at the next start, for any left over). A move never replaces a file that is already there.
# The Debug build talks only to osu!'s development server (dev.ppy.sh), never to osu.ppy.sh. Do not log in.
param(
    # Where the game keeps its data. Default: ortho-data next to the clone.
    [string]$Data,
    # osu!stable's install, where screenshots go.
    [string]$Stable = (Join-Path $env:LOCALAPPDATA "osu!"),
    [switch]$NoBuild,
    # Leave screenshots in the data folder.
    [switch]$KeepScreenshots,
    # For checks: ask the game to close after this many seconds.
    [int]$QuitAfter = 0
)
$ErrorActionPreference = "Stop"
# Windows PowerShell 5.1 turns a native command's redirected stderr into errors, which "Stop" makes fatal; the dotnet
# call below relaxes it and checks the exit code instead.
Set-Location (git rev-parse --show-toplevel)
$root = (Get-Location).Path
. (Join-Path $PSScriptRoot "windows-env.ps1")

# No update checks and no installer hooks (Velopack): this build updates with git pull. No crash reports.
$env:OSU_EXTERNAL_UPDATE_PROVIDER = "maniaortho development build"
$env:OSU_DISABLE_ERROR_REPORTING = "1"

if (-not $Data) { $Data = Join-Path (Split-Path $root -Parent) "ortho-data" }
$Data = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($Data)
$bin = Join-Path $root "osu.Desktop\bin\Debug\net10.0"
$exe = Join-Path $bin "osu!.exe"

if (-not $NoBuild) {
    Write-Host "== build"
    $ErrorActionPreference = "Continue"
    # No build servers: ones left running from an earlier build would keep that build's temp folder and settings.
    $buildLog = dotnet build osu.Desktop -c Debug -v q -nologo --disable-build-servers 2>&1
    $buildExit = $LASTEXITCODE
    $ErrorActionPreference = "Stop"
    if ($buildExit -ne 0) {
        $buildLog | Select-Object -Last 20 | ForEach-Object { "   $_" }
        Write-Host "build FAILED (run: dotnet build osu.Desktop -c Debug)"
        exit 1
    }
}
if (-not (Test-Path -LiteralPath $exe)) { Write-Host "No build at $exe (run without -NoBuild)"; exit 1 }

# framework.ini next to osu!.exe: osu-framework then treats the build folder as a portable install and never opens the
# profile's data folders. storage.ini moves the data from there to $Data.
$frameworkIni = Join-Path $bin "framework.ini"
if (-not (Test-Path -LiteralPath $frameworkIni)) { [IO.File]::WriteAllText($frameworkIni, "") }
[IO.File]::WriteAllText((Join-Path $bin "storage.ini"), "FullPath = $Data`r`n")

New-Item -ItemType Directory -Force -Path $Data | Out-Null
# osu! asks what to do with an empty custom data folder on its first start; a note in it skips the question.
$note = Join-Path $Data "ortho-data.txt"
if (-not (Test-Path -LiteralPath $note)) {
    [IO.File]::WriteAllText($note, "Data of the maniaortho development build (scripts\ortho\play.ps1 in the clone).`r`n" +
        "Imported maps and skins here are hard links to osu!stable's files: deleting this folder frees no space and leaves stable's files as they are.`r`n")
}

function Move-Screenshots {
    $from = Join-Path $Data "screenshots"
    $to = Join-Path $Stable "Screenshots"
    if ($KeepScreenshots -or -not (Test-Path -LiteralPath $from) -or -not (Test-Path -LiteralPath $to)) { return }
    $moved = 0
    foreach ($file in Get-ChildItem -LiteralPath $from -File) {
        $target = Join-Path $to $file.Name
        if (Test-Path -LiteralPath $target) { Write-Host "   kept $($file.Name): stable already has a file of that name"; continue }
        Move-Item -LiteralPath $file.FullName -Destination $target
        $moved++
    }
    if ($moved -gt 0) { Write-Host "== moved $moved screenshot(s) to $to" }
}

# The game must never touch these (only checked for existence and time, never opened).
$devData = Join-Path $env:APPDATA "osu-development"
$devDataBefore = if (Test-Path -LiteralPath $devData) { (Get-Item -LiteralPath $devData).LastWriteTimeUtc } else { $null }

Move-Screenshots
Write-Host "== osu! (data: $Data)"
$game = Start-Process -FilePath $exe -WorkingDirectory $bin -PassThru
if ($QuitAfter -gt 0) {
    if (-not $game.WaitForExit($QuitAfter * 1000)) {
        # At the main menu a close request opens osu!'s "are you sure you want to exit?" dialog, and a second one while
        # it is open confirms it (MainMenu.OnExiting); osu! then plays its outro and exits, saving everything.
        $game.CloseMainWindow() | Out-Null
        Start-Sleep -Seconds 3
        if (-not $game.HasExited) { $game.CloseMainWindow() | Out-Null }
        if (-not $game.WaitForExit(20000)) { $game.Kill(); Write-Host "   osu! did not close; stopped it" }
    }
}
$game.WaitForExit()
Move-Screenshots

$devDataAfter = if (Test-Path -LiteralPath $devData) { (Get-Item -LiteralPath $devData).LastWriteTimeUtc } else { $null }
if ($devDataAfter -ne $devDataBefore) { Write-Host "WARNING: $devData changed during the run; the build was not portable" ; exit 1 }
Write-Host "== closed; logs in $(Join-Path $Data 'logs')"
