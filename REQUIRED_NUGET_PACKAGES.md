# Required NuGet Packages for Doodle Application

This document lists all the NuGet packages required to build and run the Doodle MAUI Blazor application.

## Core MAUI Packages

### 1. Microsoft.Maui.Controls
- **Version**: $(MauiVersion) - Automatically resolved by .NET MAUI SDK
- **Purpose**: Core MAUI controls and UI framework
- **Required**: Yes - Essential for MAUI application

### 2. Microsoft.AspNetCore.Components.WebView.Maui
- **Version**: $(MauiVersion) - Automatically resolved by .NET MAUI SDK
- **Purpose**: Enables Blazor WebView in MAUI applications
- **Required**: Yes - Essential for Blazor integration in MAUI

## Logging Package

### 3. Microsoft.Extensions.Logging.Debug
- **Version**: 9.0.8
- **Purpose**: Debug logging provider for development
- **Required**: Yes - Used for debugging and error tracking
- **Note**: Only active in DEBUG builds

## Database Package

### 4. sqlite-net-pcl
- **Version**: 1.9.172
- **Purpose**: SQLite database provider for .NET
- **Required**: Yes - Core database functionality
- **Dependencies**: 
  - SQLitePCLRaw.core (automatically included)
  - SQLitePCLRaw.bundle_green (automatically included)
  - Platform-specific SQLite libraries (automatically included)

## MVVM Framework

### 5. CommunityToolkit.Mvvm
- **Version**: 8.3.2
- **Purpose**: MVVM (Model-View-ViewModel) toolkit with commands, observable properties, etc.
- **Required**: Yes - Used by ViewModels for data binding and commands
- **Dependencies**:
  - CommunityToolkit.Common (automatically included)
  - Microsoft.Toolkit.Mvvm (automatically included)

## Markdown Processing

### 6. Markdig
- **Version**: 0.33.0
- **Purpose**: Markdown parser and renderer
- **Required**: Yes - Used for rendering markdown content in journal entries
- **Dependencies**: None

## Automatically Included Packages

The following packages are automatically included by the MAUI SDK and don't need to be explicitly referenced:

- **Microsoft.Maui.Essentials** - Platform APIs
- **Microsoft.Maui.Graphics** - Graphics rendering
- **Microsoft.Extensions.DependencyInjection** - Dependency injection
- **Microsoft.Extensions.Logging** - Logging abstractions
- **Microsoft.AspNetCore.Components** - Blazor components
- **Microsoft.AspNetCore.Components.Web** - Blazor web components

## Installation

All packages are already configured in `Doodle.csproj`. To restore packages, run:

```powershell
dotnet restore
```

## Package Summary

| Package | Version | Purpose | Required |
|---------|---------|---------|----------|
| Microsoft.Maui.Controls | $(MauiVersion) | MAUI UI Framework | ✅ Yes |
| Microsoft.AspNetCore.Components.WebView.Maui | $(MauiVersion) | Blazor WebView | ✅ Yes |
| Microsoft.Extensions.Logging.Debug | 9.0.8 | Debug Logging | ✅ Yes |
| sqlite-net-pcl | 1.9.172 | SQLite Database | ✅ Yes |
| CommunityToolkit.Mvvm | 8.3.2 | MVVM Framework | ✅ Yes |
| Markdig | 0.33.0 | Markdown Processing | ✅ Yes |

## Notes

- All MAUI packages use `$(MauiVersion)` which is automatically resolved by the .NET MAUI SDK
- The project targets .NET 9.0, so ensure you have .NET 9.0 SDK installed
- For Windows development, ensure Windows 10 SDK 10.0.19041.0 or later is installed
- Some packages may have transitive dependencies that are automatically resolved

## Troubleshooting

If you encounter package restore issues:

1. **Clear NuGet cache**: `dotnet nuget locals all --clear`
2. **Restore packages**: `dotnet restore`
3. **Rebuild solution**: `dotnet build`
4. **Check .NET version**: `dotnet --version` (should be 9.0.x)

