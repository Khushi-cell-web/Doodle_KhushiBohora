using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
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
            builder.Services.AddScoped<StreakService>();

            // Register ViewModels
            builder.Services.AddScoped<JournalEditorViewModel>();
            builder.Services.AddScoped<CalendarViewModel>();
            builder.Services.AddScoped<SearchViewModel>();
            builder.Services.AddScoped<StreakViewModel>();

            var app = builder.Build();

            // Initialize database on startup (non-blocking, fire-and-forget)
            // This ensures the UI loads immediately even if database initialization takes time
            Task.Run(async () =>
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
