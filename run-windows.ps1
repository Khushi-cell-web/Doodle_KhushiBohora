# PowerShell script to run the Doodle application on Windows
# Usage: .\run-windows.ps1
#
# IMPORTANT: This script MUST be used because the project targets multiple frameworks.
# Running 'dotnet run' without --framework will fail with an error.

$ErrorActionPreference = "Stop"

# Check if we're in the correct directory
if (-not (Test-Path "Doodle.csproj")) {
    Write-Host "ERROR: Doodle.csproj not found!" -ForegroundColor Red
    Write-Host "Please run this script from the project root directory." -ForegroundColor Yellow
    exit 1
}

Write-Host "Stopping any running Doodle processes..." -ForegroundColor Yellow
Get-Process | Where-Object {$_.ProcessName -like "*Doodle*"} | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 1

Write-Host "Building and running Doodle for Windows..." -ForegroundColor Green
Write-Host "Framework: net9.0-windows10.0.19041.0" -ForegroundColor Cyan
Write-Host ""

try {
    # Clean first to remove any cached build artifacts
    Write-Host "Cleaning previous build..." -ForegroundColor Cyan
    dotnet clean Doodle.csproj /p:TargetFramework=net9.0-windows10.0.19041.0 /nologo 2>&1 | Out-Null
    
    # Build only Windows framework - use MSBuild to skip other frameworks
    Write-Host "Building Windows framework only..." -ForegroundColor Cyan
    Write-Host "(Ignoring errors for Android/iOS/MacCatalyst - they require workloads)" -ForegroundColor Gray
    Write-Host ""
    
    # Use MSBuild with explicit target framework and skip invalid configurations
    $buildResult = dotnet msbuild Doodle.csproj `
        /p:TargetFramework=net9.0-windows10.0.19041.0 `
        /p:SkipInvalidConfigurations=true `
        /p:BuildProjectReferences=false `
        /t:Restore,Build `
        /nologo `
        /v:minimal 2>&1
    
    # Filter out workload errors for non-Windows platforms
    $actualErrors = $buildResult | Where-Object { 
        $_ -match "error" -and 
        $_ -notmatch "NETSDK1147" -and
        $_ -notmatch "maccatalyst" -and
        $_ -notmatch "android" -and
        $_ -notmatch "ios"
    }
    
    if ($actualErrors) {
        Write-Host "Build errors detected:" -ForegroundColor Red
        $actualErrors | ForEach-Object { Write-Host $_ -ForegroundColor Red }
        throw "Build failed with errors"
    }
    
    Write-Host "Build completed successfully!" -ForegroundColor Green
    Write-Host "Starting application..." -ForegroundColor Green
    Write-Host ""
    dotnet run --framework net9.0-windows10.0.19041.0 --project Doodle.csproj --no-build
}
catch {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Red
    Write-Host "ERROR: Failed to run the project" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
    Write-Host "Error details: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "Troubleshooting:" -ForegroundColor Yellow
    Write-Host "1. Make sure you're in the project root directory" -ForegroundColor White
    Write-Host "2. Ensure .NET 9.0 SDK is installed" -ForegroundColor White
    Write-Host "3. Try running: dotnet restore" -ForegroundColor White
    Write-Host "4. Try running: dotnet build --framework net9.0-windows10.0.19041.0" -ForegroundColor White
    Write-Host ""
    exit 1
}
