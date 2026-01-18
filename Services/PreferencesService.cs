using Doodle.Models;
using Doodle.Data;

namespace Doodle.Services;

/// <summary>
/// Service for managing user preferences stored in the database.
/// </summary>
public class PreferencesService
{
    private readonly DatabaseService _databaseService;

    public PreferencesService(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    /// <summary>
    /// Gets a preference value by key.
    /// </summary>
    /// <param name="key">The preference key.</param>
    /// <param name="defaultValue">The default value if not found.</param>
    /// <returns>The preference value or default.</returns>
    public async Task<string> GetPreferenceAsync(string key, string defaultValue = "")
    {
        try
        {
            var preference = await _databaseService.Database.Table<UserPreferences>()
                .Where(p => p.Key == key)
                .FirstOrDefaultAsync();

            return preference?.Value ?? defaultValue;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting preference {key}: {ex.Message}");
            return defaultValue;
        }
    }

    /// <summary>
    /// Sets a preference value.
    /// </summary>
    /// <param name="key">The preference key.</param>
    /// <param name="value">The preference value.</param>
    public async Task SetPreferenceAsync(string key, string value)
    {
        try
        {
            var existing = await _databaseService.Database.Table<UserPreferences>()
                .Where(p => p.Key == key)
                .FirstOrDefaultAsync();

            if (existing != null)
            {
                existing.Value = value;
                existing.UpdatedAt = DateTime.UtcNow;
                await _databaseService.Database.UpdateAsync(existing);
            }
            else
            {
                var preference = new UserPreferences
                {
                    Key = key,
                    Value = value,
                    UpdatedAt = DateTime.UtcNow
                };
                await _databaseService.Database.InsertAsync(preference);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error setting preference {key}: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets a boolean preference.
    /// </summary>
    public async Task<bool> GetBoolPreferenceAsync(string key, bool defaultValue = false)
    {
        var value = await GetPreferenceAsync(key, defaultValue.ToString());
        return bool.TryParse(value, out var result) ? result : defaultValue;
    }

    /// <summary>
    /// Sets a boolean preference.
    /// </summary>
    public async Task SetBoolPreferenceAsync(string key, bool value)
    {
        await SetPreferenceAsync(key, value.ToString());
    }
}
