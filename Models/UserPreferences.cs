using SQLite;

namespace Doodle.Models;

/// <summary>
/// Represents user preferences stored in the database.
/// </summary>
[Table("UserPreferences")]
public class UserPreferences
{
    /// <summary>
    /// Primary key for the preferences record.
    /// </summary>
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    /// <summary>
    /// The preference key (e.g., "theme", "default_entry_time", etc.).
    /// </summary>
    [MaxLength(100)]
    [Indexed(Unique = true)]
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// The preference value.
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the preference was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
