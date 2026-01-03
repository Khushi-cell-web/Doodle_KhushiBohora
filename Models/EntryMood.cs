using SQLite;

namespace Doodle.Models;

/// <summary>
/// Junction table representing the many-to-many relationship between
/// JournalEntries and Moods. Each entry can have one primary mood and
/// up to two secondary moods.
/// </summary>
[Table("EntryMoods")]
public class EntryMood
{
    /// <summary>
    /// Primary key for the entry-mood relationship.
    /// </summary>
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to the JournalEntry.
    /// </summary>
    [Indexed]
    public int JournalEntryId { get; set; }

    /// <summary>
    /// Foreign key to the Mood.
    /// </summary>
    [Indexed]
    public int MoodId { get; set; }

    /// <summary>
    /// Indicates whether this is the primary mood (true) or a secondary mood (false).
    /// Only one primary mood is allowed per entry.
    /// </summary>
    public bool IsPrimary { get; set; }

    /// <summary>
    /// Order of secondary moods (1 or 2). Only relevant for secondary moods.
    /// Primary mood has order 0.
    /// </summary>
    public int SecondaryOrder { get; set; }
}

