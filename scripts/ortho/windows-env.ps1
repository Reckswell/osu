# Dot-sourced by the Windows scripts (playtest.ps1, play.ps1) from the clone's root: keeps what the .NET CLI and the
# game write inside the clone, away from the user's profile. NuGet packages, the .NET CLI's home and temp files go to
# .ortho-cache (gitignored).
$cache = Join-Path (Get-Location) ".ortho-cache"
$env:NUGET_PACKAGES = Join-Path $cache "nuget-packages"
$env:NUGET_HTTP_CACHE_PATH = Join-Path $cache "nuget-http"
$env:NUGET_PLUGINS_CACHE_PATH = Join-Path $cache "nuget-plugins"
$env:DOTNET_CLI_HOME = Join-Path $cache "dotnet-home"
$env:TEMP = Join-Path $cache "temp"
$env:TMP = $env:TEMP
$env:DOTNET_CLI_TELEMETRY_OPTOUT = "1"
$env:DOTNET_NOLOGO = "1"
# The .NET CLI's first run would otherwise add its tools folder to the user's PATH and a development certificate to the
# user's certificate store.
$env:DOTNET_ADD_GLOBAL_TOOLS_TO_PATH = "0"
$env:DOTNET_GENERATE_ASPNET_CERTIFICATE = "0"
New-Item -ItemType Directory -Force -Path $env:TEMP, $env:DOTNET_CLI_HOME | Out-Null
