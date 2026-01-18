using Microsoft.Maui.Storage;
using Microsoft.Maui.ApplicationModel;

namespace Doodle.Services;

/// <summary>
/// Service for managing application theme preferences.
/// </summary>
public class ThemeService
{
    private const string ThemeKey = "app_theme";
    private const string DefaultTheme = "auto";
    private readonly PreferencesService? _preferencesService;

    public ThemeService(PreferencesService? preferencesService = null)
    {
        _preferencesService = preferencesService;
    }

    /// <summary>
    /// Gets the current theme preference.
    /// </summary>
    /// <returns>The theme name: "light", "dark", or "auto".</returns>
    public async Task<string> GetThemeAsync()
    {
        // Try database first, fallback to Preferences
        if (_preferencesService != null)
        {
            var dbTheme = await _preferencesService.GetPreferenceAsync(ThemeKey, "");
            if (!string.IsNullOrEmpty(dbTheme))
            {
                return dbTheme;
            }
        }

        return Preferences.Get(ThemeKey, DefaultTheme);
    }

    /// <summary>
    /// Gets the current theme preference (synchronous version for compatibility).
    /// </summary>
    public string GetTheme()
    {
        return Preferences.Get(ThemeKey, DefaultTheme);
    }

    /// <summary>
    /// Sets the theme preference.
    /// </summary>
    /// <param name="theme">The theme name: "light", "dark", "auto", or "custom".</param>
    public async Task SetThemeAsync(string theme)
    {
        if (theme != "light" && theme != "dark" && theme != "auto" && theme != "custom")
        {
            theme = DefaultTheme;
        }

        System.Diagnostics.Debug.WriteLine($"SetThemeAsync called with theme: {theme}");

        // Store in both Preferences (for immediate access) and database (for persistence)
        Preferences.Set(ThemeKey, theme);
        
        if (_preferencesService != null)
        {
            await _preferencesService.SetPreferenceAsync(ThemeKey, theme);
            System.Diagnostics.Debug.WriteLine("Theme saved to database");
        }

        // Apply theme immediately on main thread
        if (Application.Current != null)
        {
            if (MainThread.IsMainThread)
            {
                ApplyTheme(theme);
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ApplyTheme(theme);
                });
            }
        }
    }

    /// <summary>
    /// Sets the theme preference (synchronous version for compatibility).
    /// </summary>
    public void SetTheme(string theme)
    {
        if (theme != "light" && theme != "dark" && theme != "auto" && theme != "custom")
        {
            theme = DefaultTheme;
        }

        Preferences.Set(ThemeKey, theme);
        ApplyTheme(theme);
    }

    /// <summary>
    /// Applies the theme to the application.
    /// </summary>
    /// <param name="theme">The theme to apply.</param>
    private void ApplyTheme(string theme)
    {
        try
        {
            if (Application.Current == null)
            {
                System.Diagnostics.Debug.WriteLine("Application.Current is null, cannot apply theme");
                return;
            }

            AppTheme appTheme = theme switch
            {
                "light" => AppTheme.Light,
                "dark" => AppTheme.Dark,
                "auto" => AppTheme.Unspecified,
                "custom" => AppTheme.Unspecified, // Custom uses system preference for base
                _ => AppTheme.Unspecified
            };

            System.Diagnostics.Debug.WriteLine($"Applying theme: {theme} -> {appTheme}");
            
            // Ensure we're on the main thread
            if (MainThread.IsMainThread)
            {
                Application.Current.UserAppTheme = appTheme;
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Application.Current.UserAppTheme = appTheme;
                });
            }
            
            System.Diagnostics.Debug.WriteLine($"Theme applied successfully. Current theme: {Application.Current.UserAppTheme}");
            
            // Trigger theme color update in Blazor
            TriggerThemeColorUpdate(theme);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error applying theme: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    /// <summary>
    /// Triggers a theme color update event that Blazor can listen to.
    /// </summary>
    private void TriggerThemeColorUpdate(string theme)
    {
        try
        {
            // Use a static event or store the theme for Blazor to read
            // For now, we'll rely on CSS media queries and data attributes
            System.Diagnostics.Debug.WriteLine($"Theme color update triggered for: {theme}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error triggering theme color update: {ex.Message}");
        }
    }

    /// <summary>
    /// Loads and applies the saved theme on application startup.
    /// </summary>
    public void LoadTheme()
    {
        var theme = GetTheme();
        ApplyTheme(theme);
    }

    /// <summary>
    /// Gets the custom theme colors.
    /// </summary>
    /// <returns>A tuple with (DeepGreen, Olive, ClayBeige) colors, or default colors if not set.</returns>
    public async Task<(string DeepGreen, string Olive, string ClayBeige)> GetCustomColorsAsync()
    {
        const string DefaultDeepGreen = "#30360E";
        const string DefaultOlive = "#787F56";
        const string DefaultClayBeige = "#E2D4B9";

        if (_preferencesService != null)
        {
            var deepGreen = await _preferencesService.GetPreferenceAsync("custom_theme_deep_green", DefaultDeepGreen);
            var olive = await _preferencesService.GetPreferenceAsync("custom_theme_olive", DefaultOlive);
            var clayBeige = await _preferencesService.GetPreferenceAsync("custom_theme_clay_beige", DefaultClayBeige);
            return (deepGreen, olive, clayBeige);
        }

        return (
            Preferences.Get("custom_theme_deep_green", DefaultDeepGreen),
            Preferences.Get("custom_theme_olive", DefaultOlive),
            Preferences.Get("custom_theme_clay_beige", DefaultClayBeige)
        );
    }

    /// <summary>
    /// Sets the custom theme colors.
    /// </summary>
    public async Task SetCustomColorsAsync(string deepGreen, string olive, string clayBeige)
    {
        if (_preferencesService != null)
        {
            await _preferencesService.SetPreferenceAsync("custom_theme_deep_green", deepGreen);
            await _preferencesService.SetPreferenceAsync("custom_theme_olive", olive);
            await _preferencesService.SetPreferenceAsync("custom_theme_clay_beige", clayBeige);
        }

        Preferences.Set("custom_theme_deep_green", deepGreen);
        Preferences.Set("custom_theme_olive", olive);
        Preferences.Set("custom_theme_clay_beige", clayBeige);
    }
}
