# PowerShell script to find and open the Doodle database

Write-Host "`n=== Finding Doodle Database ===" -ForegroundColor Cyan

# Search for the database file
$dbFile = Get-ChildItem $env:LOCALAPPDATA -Recurse -Filter "doodle.db3" -ErrorAction SilentlyContinue

if ($dbFile) {
    Write-Host "`nDatabase Found!" -ForegroundColor Green
    Write-Host "Location: $($dbFile.FullName)" -ForegroundColor Yellow
    Write-Host "Size: $([math]::Round($dbFile.Length / 1KB, 2)) KB" -ForegroundColor Yellow
    Write-Host "Last Modified: $($dbFile.LastWriteTime)" -ForegroundColor Yellow
    
    # Open in File Explorer
    Write-Host "`nOpening database location in File Explorer..." -ForegroundColor Cyan
    Start-Process explorer.exe -ArgumentList "/select,`"$($dbFile.FullName)`""
    
    Write-Host "`nTo open the database with a SQLite browser, use:" -ForegroundColor Yellow
    Write-Host "  DB Browser for SQLite: https://sqlitebrowser.org/" -ForegroundColor White
    Write-Host "  Or use: sqlite3 `"$($dbFile.FullName)`"" -ForegroundColor White
} else {
    Write-Host "`nDatabase not found!" -ForegroundColor Red
    Write-Host "The database will be created when you first run the application." -ForegroundColor Yellow
}

Write-Host ""
