using SQLite;

namespace Doodle.Models;

/// <summary>
/// Junction table representing the many-to-many relationship between
/// JournalEntries and Tags. An entry can have multiple tags, and a tag
/// can be associated with multiple entries.
/// </summary>
[Table("EntryTags")]
public class EntryTag
{
    /// <summary>
    /// Primary key for the entry-tag relationship.
    /// </summary>
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to the JournalEntry.
    /// </summary>
    [Indexed]
    public int JournalEntryId { get; set; }

    /// <summary>
    /// Foreign key to the Tag.
    /// </summary>
    [Indexed]
    public int TagId { get; set; }
}

