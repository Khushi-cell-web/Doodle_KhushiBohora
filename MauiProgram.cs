using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Doodle.Data;
using Doodle.Services;
using Doodle.ViewModels;

namespace Doodle
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
    		builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            // Register Data Services
            builder.Services.AddSingleton<DatabaseService>();

            // Register Business Services
            builder.Services.AddScoped<JournalService>();
            builder.Services.AddScoped<MoodService>();
            builder.Services.AddScoped<TagService>();
            builder.Services.AddScoped<CategoryService>();
            builder.Services.AddScoped<StreakService>();
            builder.Services.AddScoped<AnalyticsService>(sp =>
            {
                var databaseService = sp.GetRequiredService<DatabaseService>();
                var journalService = sp.GetRequiredService<JournalService>();
                var moodService = sp.GetRequiredService<MoodService>();
                return new AnalyticsService(databaseService, journalService, moodService);
            });
            builder.Services.AddSingleton<PreferencesService>();
            builder.Services.AddSingleton<SecurityService>();
            builder.Services.AddSingleton<SessionService>();
            
            // ThemeService needs PreferencesService, so register it after
            builder.Services.AddSingleton<ThemeService>(sp =>
            {
                var preferencesService = sp.GetService<PreferencesService>();
                return new ThemeService(preferencesService);
            });

            // Register ViewModels
            builder.Services.AddScoped<JournalEditorViewModel>(sp =>
            {
                var journalService = sp.GetRequiredService<JournalService>();
                var moodService = sp.GetRequiredService<MoodService>();
                var tagService = sp.GetRequiredService<TagService>();
                var categoryService = sp.GetRequiredService<CategoryService>();
                return new JournalEditorViewModel(journalService, moodService, tagService, categoryService);
            });
            builder.Services.AddScoped<CalendarViewModel>();
            builder.Services.AddScoped<SearchViewModel>(sp =>
            {
                var journalService = sp.GetRequiredService<JournalService>();
                var moodService = sp.GetRequiredService<MoodService>();
                var tagService = sp.GetRequiredService<TagService>();
                var categoryService = sp.GetRequiredService<CategoryService>();
                return new SearchViewModel(journalService, moodService, tagService, categoryService);
            });
            builder.Services.AddScoped<StreakViewModel>(sp =>
            {
                var streakService = sp.GetRequiredService<StreakService>();
                var journalService = sp.GetRequiredService<JournalService>();
                return new StreakViewModel(streakService, journalService);
            });
            builder.Services.AddScoped<HomeViewModel>(sp =>
            {
                var journalService = sp.GetRequiredService<JournalService>();
                return new HomeViewModel(journalService);
            });
            builder.Services.AddScoped<ViewJournalEntryViewModel>();
            builder.Services.AddScoped<ExportJournalViewModel>();
            builder.Services.AddScoped<DashboardViewModel>(sp =>
            {
                var journalService = sp.GetRequiredService<JournalService>();
                var moodService = sp.GetRequiredService<MoodService>();
                var tagService = sp.GetRequiredService<TagService>();
                var categoryService = sp.GetRequiredService<CategoryService>();
                var streakService = sp.GetRequiredService<StreakService>();
                var analyticsService = sp.GetRequiredService<AnalyticsService>();
                return new DashboardViewModel(journalService, moodService, tagService, categoryService, streakService, analyticsService);
            });
            builder.Services.AddScoped<SettingsViewModel>(sp =>
            {
                var themeService = sp.GetRequiredService<ThemeService>();
                var securityService = sp.GetRequiredService<SecurityService>();
                var journalService = sp.GetRequiredService<JournalService>();
                var moodService = sp.GetService<MoodService>();
                var tagService = sp.GetService<TagService>();
                return new SettingsViewModel(themeService, securityService, journalService, moodService, tagService);
            });

            var app = builder.Build();

            // Ensure session is locked on app startup if security is enabled
            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(100); // Small delay to ensure services are ready
                    var securityService = app.Services.GetRequiredService<SecurityService>();
                    var sessionService = app.Services.GetRequiredService<SessionService>();
                    
                    // If security is enabled, ensure session is locked on startup
                    if (securityService.IsSecurityEnabled())
                    {
                        System.Diagnostics.Debug.WriteLine("Security is enabled - locking session on startup");
                        sessionService.SetLocked();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error locking session on startup: {ex.Message}");
                }
            });

            // Load theme on startup
            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(1000); // Wait for app and database to initialize
                    var themeService = app.Services.GetRequiredService<ThemeService>();
                    var appInstance = app.Services.GetRequiredService<App>();
                    appInstance.SetThemeService(themeService);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading theme: {ex.Message}");
                }
            });

            // Initialize database on startup (non-blocking, fire-and-forget)
            // This ensures the UI loads immediately even if database initialization takes time
            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(100); // Small delay to let UI render first
                    await InitializeDatabaseAsync(app);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Database initialization error: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                }
            }).ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    System.Diagnostics.Debug.WriteLine($"Database initialization task faulted: {t.Exception?.GetBaseException()?.Message}");
                }
            }, TaskContinuationOptions.OnlyOnFaulted);

            return app;
        }

        /// <summary>
        /// Initializes the SQLite database on application startup (async, non-blocking).
        /// </summary>
        private static async Task InitializeDatabaseAsync(MauiApp app)
        {
            try
            {
                var databaseService = app.Services.GetRequiredService<DatabaseService>();
                await databaseService.InitializeAsync();
                System.Diagnostics.Debug.WriteLine("Database initialized successfully");
            }
            catch (Exception ex)
            {
                // Log error (in production, use proper logging)
                System.Diagnostics.Debug.WriteLine($"Database initialization error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}
