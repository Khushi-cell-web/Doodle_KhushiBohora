using SQLite;

namespace Doodle.Models;

/// <summary>
/// Represents a single journal entry in the Doodle application.
/// Enforces the business rule that only one entry per calendar day is allowed.
/// </summary>
[Table("JournalEntries")]
public class JournalEntry
{
    /// <summary>
    /// Primary key for the journal entry.
    /// </summary>
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    /// <summary>
    /// The title of the journal entry (optional).
    /// </summary>
    [MaxLength(200)]
    public string? Title { get; set; }

    /// <summary>
    /// The Markdown-formatted content of the journal entry.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// The date this entry is associated with (date only, no time).
    /// Used to enforce the one-entry-per-day rule.
    /// </summary>
    [Indexed]
    public DateTime EntryDate { get; set; }

    /// <summary>
    /// System-generated timestamp indicating when the entry was first created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// System-generated timestamp indicating when the entry was last updated.
    /// Automatically updated on each modification.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Calculated word count of the entry content.
    /// Updated automatically when content changes.
    /// </summary>
    public int WordCount { get; set; }

    /// <summary>
    /// The category this entry belongs to (optional).
    /// </summary>
    [MaxLength(50)]
    public string? Category { get; set; }

    /// <summary>
    /// Static readonly array of whitespace characters used for word count calculation.
    /// Using static readonly instead of constant array for better performance when called repeatedly.
    /// </summary>
    private static readonly char[] WordSeparators = { ' ', '\t', '\n', '\r' };

    /// <summary>
    /// Calculates the word count from the Content property.
    /// </summary>
    /// <returns>The number of words in the content.</returns>
    public void CalculateWordCount()
    {
        if (string.IsNullOrWhiteSpace(Content))
        {
            WordCount = 0;
            return;
        }

        // Split by whitespace and count non-empty segments
        WordCount = Content
            .Split(WordSeparators, StringSplitOptions.RemoveEmptyEntries)
            .Length;
    }
}

