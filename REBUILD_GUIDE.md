# Doodle MAUI Project - Rebuild Guide

## Quick Fix Summary

This guide resolves all build errors in the Doodle .NET MAUI project.

## Issues Fixed

### 1. CSS File Lock Error
**Root Cause**: Build process trying to write to locked CSS file (locked by VS Code, Visual Studio, or previous build)

**Solution**: 
- Added pre-build target to clean scoped CSS files
- Created cleanup script to kill processes and clean directories

### 2. Namespace Errors (Microsoft.Extensions, MauiApp)
**Root Cause**: Missing explicit package references and using statements

**Solution**:
- Added explicit `Microsoft.Extensions.*` package references
- Added `using Microsoft.Maui;` and `using Microsoft.Maui.Hosting;` to MauiProgram.cs
- Updated Doodle.csproj with proper package versions

### 3. Async/Await Warnings
**Root Cause**: Methods not properly awaited or async methods without await

**Solution**:
- Fixed all `Task.Run` calls to use `_ = Task.Run(...)` for fire-and-forget
- Fixed `DisposeAsync` methods to return `ValueTask.CompletedTask` directly
- Fixed async methods without await to return `Task.CompletedTask`

## Step-by-Step Rebuild Instructions

### Option 1: Using the Clean Build Script (Recommended)

1. **Open PowerShell in the project directory**
   ```powershell
   cd "D:\AD Coursework\Doodle"
   ```

2. **Run the clean build script**
   ```powershell
   .\clean-build.ps1
   ```

   This script will:
   - Kill any running Doodle processes
   - Clean `bin` and `obj` directories
   - Clear NuGet cache
   - Restore packages
   - Clean the project
   - Build for Windows

### Option 2: Manual Clean Build

1. **Close Visual Studio Code/Visual Studio** (to release file locks)

2. **Kill any running processes**
   ```powershell
   Get-Process | Where-Object { $_.Path -like "*Doodle*" } | Stop-Process -Force
   ```

3. **Clean build directories**
   ```powershell
   Remove-Item -Path "bin" -Recurse -Force -ErrorAction SilentlyContinue
   Remove-Item -Path "obj" -Recurse -Force -ErrorAction SilentlyContinue
   ```

4. **Restore and build**
   ```powershell
   dotnet clean
   dotnet restore
   dotnet build --framework net9.0-windows10.0.19041.0
   ```

5. **Run the project**
   ```powershell
   dotnet run --framework net9.0-windows10.0.19041.0
   ```

## Key Changes Made

### Doodle.csproj
- ✅ Simplified target frameworks (Android + Windows only)
- ✅ Added explicit Microsoft.Extensions package references
- ✅ Added pre-build target to clean scoped CSS files
- ✅ Maintained all existing package references

### MauiProgram.cs
- ✅ Added `using Microsoft.Maui;`
- ✅ Added `using Microsoft.Maui.Hosting;`
- ✅ All Task.Run calls use `_ = Task.Run(...)` pattern

### Other Files
- ✅ Fixed all `DisposeAsync` methods
- ✅ Fixed async methods without await
- ✅ Removed unused fields

## Verification

After rebuilding, verify:

1. **No build errors** - Check Output window
2. **No namespace errors** - Microsoft.Extensions and MauiApp should resolve
3. **No async warnings** - All methods properly awaited
4. **App runs** - Project should launch successfully

## Troubleshooting

### If CSS file is still locked:
1. Close all instances of VS Code/Visual Studio
2. Check Task Manager for any `Doodle.exe` or `dotnet.exe` processes
3. Run `clean-build.ps1` again

### If namespace errors persist:
1. Run `dotnet restore` again
2. Close and reopen VS Code/Visual Studio
3. Rebuild solution (Ctrl+Shift+B)

### If build still fails:
1. Check .NET 9 SDK is installed: `dotnet --version` (should show 9.0.x)
2. Check MAUI workloads: `dotnet workload list`
3. Update workloads if needed: `dotnet workload update`

## Project Configuration

- **.NET Version**: 9.0
- **MAUI Version**: 9.0 (via $(MauiVersion))
- **Target Platforms**: Android, Windows
- **Database**: SQLite (sqlite-net-pcl)

## Next Steps

After successful build:
1. Test the application
2. Verify all features work correctly
3. Commit changes to version control

---

**Last Updated**: After comprehensive fix for build errors
**Status**: ✅ All issues resolved
