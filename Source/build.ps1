param(
    [switch]$NoInstall
)

$ErrorActionPreference = "Stop"

$ProjectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectPath = Join-Path $ProjectRoot "BoneSmith.csproj"
$OutputPath = Join-Path $ProjectRoot "bin\Release"
$DevPluginPath = Join-Path $env:APPDATA "XIVLauncher\devPlugins\BoneSmith"

Write-Host "BoneSmith build starting..." -ForegroundColor Cyan
Write-Host "Project: $ProjectPath"

Push-Location $ProjectRoot
try {
    dotnet restore $ProjectPath --force
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet restore failed with exit code $LASTEXITCODE"
    }

    dotnet build $ProjectPath -c Release --no-restore
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet build failed with exit code $LASTEXITCODE"
    }

    $dllPath = Join-Path $OutputPath "BoneSmith.dll"
    if (!(Test-Path $dllPath)) {
        throw "Build completed, but BoneSmith.dll was not found in $OutputPath"
    }

    if (-not $NoInstall) {
        Write-Host "Installing dev plugin to: $DevPluginPath" -ForegroundColor Cyan
        Remove-Item -Recurse -Force $DevPluginPath -ErrorAction SilentlyContinue
        New-Item -ItemType Directory -Force $DevPluginPath | Out-Null
        Copy-Item (Join-Path $OutputPath "*") $DevPluginPath -Recurse -Force
    }

    Write-Host "BoneSmith build complete." -ForegroundColor Green
    Write-Host "In game command: /bonesmith" -ForegroundColor Green
}
finally {
    Pop-Location
}
