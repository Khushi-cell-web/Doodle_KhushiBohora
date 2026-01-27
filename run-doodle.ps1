# PowerShell script to run Doodle application on Windows
# This script builds and runs only the Windows framework, skipping Android

$ErrorActionPreference = "Stop"

Write-Host "`n=== Running Doodle Application ===" -ForegroundColor Cyan
Write-Host ""

# Build for Windows only (skips Android/MacCatalyst)
Write-Host "Building for Windows..." -ForegroundColor Yellow

# Try to restore workloads first (this may install MacCatalyst, but it won't be used)
Write-Host "Checking workloads..." -ForegroundColor Yellow
dotnet workload restore --verbosity quiet 2>$null

dotnet msbuild Doodle.csproj `
    /p:TargetFramework=net9.0-windows10.0.19041.0 `
    /p:SkipInvalidConfigurations=true `
    /p:BuildProjectReferences=false `
    /p:_IsMacCatalystEnabled=false `
    /p:_IsIOSEnabled=false `
    /t:Restore,Build `
    /nologo `
    /v:minimal

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "Build successful! Starting application..." -ForegroundColor Green
Write-Host ""

# Run the executable directly
$exePath = ".\bin\Debug\net9.0-windows10.0.19041.0\win10-x64\Doodle.exe"
if (Test-Path $exePath) {
    & $exePath
} else {
    Write-Host "Error: Executable not found at $exePath" -ForegroundColor Red
    exit 1
}
