using SQLite;

namespace Doodle.Models;

/// <summary>
/// Represents a tag that can be associated with journal entries.
/// Tags can be predefined or user-created custom tags.
/// </summary>
[Table("Tags")]
public class Tag
{
    /// <summary>
    /// Primary key for the tag.
    /// </summary>
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    /// <summary>
    /// The name of the tag (e.g., "Work", "Family", "Travel").
    /// Must be unique across all tags.
    /// </summary>
    [Unique, MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional color code for UI display (hex format, e.g., "#FF5733").
    /// </summary>
    [MaxLength(7)]
    public string? Color { get; set; }

    /// <summary>
    /// Indicates whether this is a predefined tag (true) or a user-created custom tag (false).
    /// </summary>
    public bool IsPredefined { get; set; }

    /// <summary>
    /// Timestamp when the tag was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

