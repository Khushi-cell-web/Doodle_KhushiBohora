# PowerShell script to find and open the Doodle database

Write-Host "`n=== Searching for Doodle Database ===" -ForegroundColor Cyan
Write-Host ""

# Search for the database file
$dbFile = Get-ChildItem $env:LOCALAPPDATA -Recurse -Filter "doodle.db3" -ErrorAction SilentlyContinue

if ($dbFile) {
    Write-Host "Database found!" -ForegroundColor Green
    Write-Host "Location: $($dbFile.FullName)" -ForegroundColor Yellow
    Write-Host "Size: $([math]::Round($dbFile.Length / 1KB, 2)) KB" -ForegroundColor Yellow
    Write-Host "Last Modified: $($dbFile.LastWriteTime)" -ForegroundColor Yellow
    Write-Host ""
    
    # Ask if user wants to open the database location
    $openLocation = Read-Host "Open database location in File Explorer? (Y/N)"
    if ($openLocation -eq 'Y' -or $openLocation -eq 'y') {
        Start-Process explorer.exe -ArgumentList "/select,`"$($dbFile.FullName)`""
    }
    
    # Ask if user wants to open with SQLite browser (if available)
    $openDb = Read-Host "Open database with default application? (Y/N)"
    if ($openDb -eq 'Y' -or $openDb -eq 'y') {
        Start-Process $dbFile.FullName
    }
} else {
    Write-Host "Database not found in $env:LOCALAPPDATA" -ForegroundColor Red
    Write-Host ""
    Write-Host "Trying alternative locations..." -ForegroundColor Yellow
    
    # Try common MAUI app data locations
    $alternativePaths = @(
        "$env:LOCALAPPDATA\Doodle",
        "$env:APPDATA\Doodle",
        "$env:USERPROFILE\AppData\Local\Doodle"
    )
    
    foreach ($path in $alternativePaths) {
        if (Test-Path $path) {
            $dbFile = Get-ChildItem $path -Filter "doodle.db3" -ErrorAction SilentlyContinue
            if ($dbFile) {
                Write-Host "Found at: $($dbFile.FullName)" -ForegroundColor Green
                break
            }
        }
    }
    
    if (-not $dbFile) {
        Write-Host "Database file not found. It may not have been created yet." -ForegroundColor Red
        Write-Host "Run the application first to create the database." -ForegroundColor Yellow
    }
}

Write-Host ""
