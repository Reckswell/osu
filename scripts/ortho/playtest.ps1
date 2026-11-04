# Windows version of playtest.sh: runs the playtest harness on the real GPU.
#   pwsh scripts/ortho/playtest.ps1
#   pwsh scripts/ortho/playtest.ps1 -Beatmap "C:\path\to\map.osu" -Times "5000,20000" -ScrollSpeed 24 -PlayToEnd
#   pwsh scripts/ortho/playtest.ps1 -Preset Voltex -Wall 0.3
param(
    [string[]]$Beatmap = @("synthetic-7k", "real-4k"),
    [string]$Times,
    [string]$ScrollSpeed,
    [string]$Preset,
    [string]$Wall,
    [string]$Size = "1920x1080",
    [switch]$PlayToEnd,
    [switch]$NoBaseline,
    [string]$Out
)
$ErrorActionPreference = "Stop"
# Windows PowerShell 5.1 turns a native command's redirected stderr into errors, which "Stop" makes fatal; the dotnet
# calls below relax it and check exit codes instead.
Set-Location (git rev-parse --show-toplevel)

# Keep what a run writes inside the clone (NuGet caches, the .NET CLI home, temp files: .ortho-cache). The harness keeps
# its own settings next to its build output (portable).
. (Join-Path $PSScriptRoot "windows-env.ps1")

if (-not $Out) {
    $Out = "playtest-output\$(Get-Date -Format yyyyMMdd-HHmmss)"
    New-Item -ItemType Directory -Force -Path "playtest-output" | Out-Null
    if (-not (Test-Path "playtest-output\.gitignore")) { Set-Content -Path "playtest-output\.gitignore" -Value "*" }
}
New-Item -ItemType Directory -Force -Path $Out | Out-Null

Write-Host "== build"
$ErrorActionPreference = "Continue"
# No build servers: ones left running from an earlier build would keep that build's temp folder and settings.
dotnet build osu.Game.Rulesets.ManiaOrtho.Tests -c Release -v q -nologo --disable-build-servers 2>&1 | Out-Null
$buildExit = $LASTEXITCODE
$ErrorActionPreference = "Stop"
if ($buildExit -ne 0) { Write-Host "build FAILED (run: dotnet build osu.Game.Rulesets.ManiaOrtho.Tests -c Release)"; exit 1 }
$dll = "osu.Game.Rulesets.ManiaOrtho.Tests\bin\Release\net10.0\osu.Game.Rulesets.ManiaOrtho.Tests.dll"

$rulesets = if ($NoBaseline) { @("maniaortho") } else { @("mania", "maniaortho") }
$failed = $false

foreach ($map in $Beatmap) {
    $name = ([IO.Path]::GetFileNameWithoutExtension($map)) -replace '[^A-Za-z0-9._-]', '_'
    $dir = Join-Path $Out $name
    foreach ($ruleset in $rulesets) {
        Write-Host "== $map ($ruleset)"
        $runArgs = @($dll, "playtest", "--beatmap", $map, "--ruleset", $ruleset, "--out", $dir, "--size", $Size)
        if ($Times) { $runArgs += @("--times", $Times) }
        if ($ScrollSpeed) { $runArgs += @("--scroll-speed", $ScrollSpeed) }
        if ($Preset) { $runArgs += @("--preset", $Preset) }
        if ($Wall) { $runArgs += @("--wall", $Wall) }
        if ($PlayToEnd) { $runArgs += "--play-to-end" }
        $log = "$dir-$ruleset.log"
        $ErrorActionPreference = "Continue"
        & dotnet @runArgs *> $log
        $ErrorActionPreference = "Stop"
        if ($LASTEXITCODE -ne 0) { $failed = $true; Write-Host "   FAILED, see $log"; Get-Content $log -Tail 5 }
        Select-String -Path $log -Pattern "PLAYTEST (PASSED|FAILED)" | ForEach-Object { "   " + $_.Line }
    }
}

Write-Host ""
Write-Host "Output: $Out (screenshots per map folder; summary-*.json has fps on this GPU)"
if ($failed) { exit 1 }
