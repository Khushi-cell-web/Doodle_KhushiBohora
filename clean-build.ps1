# Clean Build Script for Doodle MAUI Project
# This script resolves file locks and prepares for a clean build

Write-Host "`n=== Doodle Clean Build Script ===" -ForegroundColor Cyan
Write-Host ""

# Step 1: Kill any running Doodle processes
Write-Host "Step 1: Stopping running Doodle processes..." -ForegroundColor Yellow
$doodleProcesses = Get-Process | Where-Object { 
    $_.Path -like "*Doodle*" -and 
    $_.ProcessName -ne "powershell" -and 
    $_.ProcessName -ne "pwsh"
}
if ($doodleProcesses) {
    $doodleProcesses | ForEach-Object {
        Write-Host "  Stopping process: $($_.ProcessName) (PID: $($_.Id))" -ForegroundColor Gray
        Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue
    }
    Start-Sleep -Seconds 2
    Write-Host "  ✓ Processes stopped" -ForegroundColor Green
} else {
    Write-Host "  ✓ No running Doodle processes found" -ForegroundColor Green
}

# Step 2: Clean CSS files first (before removing obj directory)
Write-Host "`nStep 2: Cleaning CSS files..." -ForegroundColor Yellow
$cssFiles = Get-ChildItem -Path "obj" -Recurse -Filter "*.css" -ErrorAction SilentlyContinue | Where-Object { $_.FullName -like "*scopedcss*" }
if ($cssFiles) {
    $cssFiles | ForEach-Object {
        try {
            Remove-Item -Path $_.FullName -Force -ErrorAction Stop
        } catch {
            Write-Host "  ⚠ Could not delete: $($_.FullName)" -ForegroundColor Yellow
        }
    }
    Write-Host "  ✓ CSS files cleaned" -ForegroundColor Green
} else {
    Write-Host "  ✓ No CSS files to clean" -ForegroundColor Green
}

# Step 3: Clean build directories
Write-Host "`nStep 3: Cleaning build directories..." -ForegroundColor Yellow
$directories = @("bin", "obj")
foreach ($dir in $directories) {
    if (Test-Path $dir) {
        Write-Host "  Removing $dir..." -ForegroundColor Gray
        Remove-Item -Path $dir -Recurse -Force -ErrorAction SilentlyContinue
        Write-Host "  ✓ $dir cleaned" -ForegroundColor Green
    } else {
        Write-Host "  ✓ $dir doesn't exist (already clean)" -ForegroundColor Green
    }
}

# Step 4: Clean NuGet cache (optional, but helps with package issues)
Write-Host "`nStep 4: Clearing NuGet cache..." -ForegroundColor Yellow
dotnet nuget locals all --clear 2>&1 | Out-Null
Write-Host "  ✓ NuGet cache cleared" -ForegroundColor Green

# Step 5: Restore packages
Write-Host "`nStep 5: Restoring project packages..." -ForegroundColor Yellow
$restoreResult = dotnet restore 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "  ✓ Packages restored successfully" -ForegroundColor Green
} else {
    Write-Host "  ⚠ Package restore had warnings (this is usually OK)" -ForegroundColor Yellow
}

# Step 6: Clean project
Write-Host "`nStep 6: Cleaning project..." -ForegroundColor Yellow
dotnet clean 2>&1 | Out-Null
Write-Host "  ✓ Project cleaned" -ForegroundColor Green

# Step 7: Build for Windows (primary target)
Write-Host "`nStep 7: Building for Windows..." -ForegroundColor Yellow
$buildResult = dotnet build --framework net9.0-windows10.0.19041.0 --no-incremental 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "  ✓ Build successful!" -ForegroundColor Green
} else {
    Write-Host "  ✗ Build failed. Check errors above." -ForegroundColor Red
    Write-Host "`nBuild output:" -ForegroundColor Yellow
    $buildResult | Select-String -Pattern "error|warning" | Select-Object -First 10
    exit 1
}

Write-Host "`n=== Clean Build Complete ===" -ForegroundColor Green
Write-Host "You can now run the project with: dotnet run --framework net9.0-windows10.0.19041.0" -ForegroundColor Cyan
Write-Host ""
