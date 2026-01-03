# Doodle Application Structure

## Application Overview

Doodle is a .NET MAUI Blazor application for journaling with the following features:
- Journal entry creation and editing
- Calendar view for browsing entries
- Search and filter functionality
- Mood tracking
- Tag management
- Streak tracking

## Page Routes and Navigation

### 1. Home Page
- **Route**: `/`
- **File**: `Components/Pages/Home.razor`
- **Description**: Main landing page showing streak statistics, quick actions, and recent entries
- **Navigation**: Accessible via "Home" link in NavMenu

### 2. Journal Editor
- **Routes**: 
  - `/editor` - Create/edit today's entry
  - `/editor/{SelectedDate:datetime}` - Create/edit entry for specific date
- **File**: `Components/Pages/JournalEditor.razor`
- **Description**: Rich text editor for creating and editing journal entries with mood and tag support
- **Navigation**: Accessible via "Write Entry" link in NavMenu

### 3. Calendar View
- **Route**: `/calendar`
- **File**: `Components/Pages/CalendarView.razor`
- **Description**: Calendar interface showing entries by date with navigation between months
- **Navigation**: Accessible via "Calendar" link in NavMenu

### 4. Search & Filter
- **Route**: `/search`
- **File**: `Components/Pages/SearchView.razor`
- **Description**: Search and filter interface for finding entries by text, date range, mood, or tags
- **Navigation**: Accessible via "Search & Filter" link in NavMenu

## Layout Structure

### MainLayout.razor
- **Location**: `Components/Layout/MainLayout.razor`
- **Purpose**: Main application layout containing sidebar navigation and content area
- **Components**:
  - Sidebar with NavMenu
  - Top row with app branding
  - Main content area for page content

### NavMenu.razor
- **Location**: `Components/Layout/NavMenu.razor`
- **Purpose**: Navigation menu with links to all main pages
- **Links**:
  - Home (`/`)
  - Write Entry (`/editor`)
  - Calendar (`/calendar`)
  - Search & Filter (`/search`)

## Component Structure

```
Components/
├── Layout/
│   ├── MainLayout.razor      # Main application layout
│   └── NavMenu.razor          # Navigation menu
├── Pages/
│   ├── Home.razor             # Home page (route: /)
│   ├── JournalEditor.razor    # Editor page (routes: /editor, /editor/{date})
│   ├── CalendarView.razor     # Calendar page (route: /calendar)
│   └── SearchView.razor       # Search page (route: /search)
├── Shared/
│   └── MarkdownView.razor      # Markdown rendering component
├── Routes.razor               # Router configuration
└── _Imports.razor             # Global using statements
```

## Required NuGet Packages

### Core Packages
1. **Microsoft.Maui.Controls** - MAUI UI Framework
2. **Microsoft.AspNetCore.Components.WebView.Maui** - Blazor WebView support

### Additional Packages
3. **Microsoft.Extensions.Logging.Debug** (v9.0.8) - Debug logging
4. **sqlite-net-pcl** (v1.9.172) - SQLite database
5. **CommunityToolkit.Mvvm** (v8.3.2) - MVVM framework
6. **Markdig** (v0.33.0) - Markdown processing

See `REQUIRED_NUGET_PACKAGES.md` for detailed package information.

## Service Architecture

### Data Layer
- **DatabaseService** - SQLite database management
- **Location**: `Data/DatabaseService.cs`

### Business Services
- **JournalService** - Journal entry operations
- **MoodService** - Mood management
- **TagService** - Tag management
- **StreakService** - Streak calculation
- **Location**: `Services/`

### ViewModels
- **JournalEditorViewModel** - Editor page logic
- **CalendarViewModel** - Calendar page logic
- **SearchViewModel** - Search page logic
- **StreakViewModel** - Streak statistics
- **Location**: `ViewModels/`

## Models

- **JournalEntry** - Journal entry data model
- **Mood** - Mood data model
- **Tag** - Tag data model
- **EntryMood** - Journal entry-mood relationship
- **EntryTag** - Journal entry-tag relationship
- **Location**: `Models/`

## Application Entry Points

### Windows
- **Entry Point**: `Platforms/Windows/App.xaml.cs`
- **Type**: `MauiWinUIApplication`
- **Framework**: `net9.0-windows10.0.19041.0`

### Main Application
- **App.xaml.cs** - Main application class
- **MauiProgram.cs** - Application initialization and service registration
- **MainPage.xaml** - Main page with BlazorWebView

## Running the Application

### Windows
```powershell
.\run-windows.ps1
```

Or directly:
```powershell
dotnet run --framework net9.0-windows10.0.19041.0
```

## Navigation Flow

```
Home (/)
  ├── Write Entry → /editor
  ├── View Calendar → /calendar
  └── Search Entries → /search

Editor (/editor)
  ├── Can navigate to specific date: /editor/2024-01-15
  └── Returns to Home after save

Calendar (/calendar)
  ├── Click date → /editor/{date}
  └── Navigation between months

Search (/search)
  └── Click result → /editor/{date}
```

## Key Features

1. **Blazor Integration**: Full Blazor WebView integration for web-based UI
2. **SQLite Database**: Local data storage with SQLite
3. **MVVM Pattern**: Clean separation using CommunityToolkit.Mvvm
4. **Markdown Support**: Markdown rendering for journal entries
5. **Responsive Design**: Bootstrap-based responsive UI
6. **Error Handling**: Comprehensive error handling throughout

