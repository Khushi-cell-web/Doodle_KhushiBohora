using SQLite;

namespace Doodle.Models;

/// <summary>
/// Represents a journaling streak period in the Doodle application.
/// A streak is a consecutive sequence of days with journal entries.
/// </summary>
[Table("Streaks")]
public class Streak
{
    /// <summary>
    /// Primary key for the streak record.
    /// </summary>
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    /// <summary>
    /// The start date of the streak (inclusive).
    /// </summary>
    [Indexed]
    public DateTime StartDate { get; set; }

    /// <summary>
    /// The end date of the streak (inclusive).
    /// </summary>
    [Indexed]
    public DateTime EndDate { get; set; }

    /// <summary>
    /// The length of the streak in days.
    /// </summary>
    public int Length { get; set; }

    /// <summary>
    /// Indicates whether this is the current active streak.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// System-generated timestamp indicating when the streak record was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// System-generated timestamp indicating when the streak record was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
