using SQLite;

namespace Doodle.Models;

/// <summary>
/// Represents a mood that can be associated with journal entries.
/// Moods are categorized as Positive, Neutral, or Negative.
/// </summary>
[Table("Moods")]
public class Mood
{
    /// <summary>
    /// Primary key for the mood.
    /// </summary>
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    /// <summary>
    /// The name of the mood (e.g., "Happy", "Sad", "Calm").
    /// </summary>
    [Unique, MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The category of the mood: Positive, Neutral, or Negative.
    /// </summary>
    [MaxLength(20)]
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether this is a predefined mood (true) or a user-created custom mood (false).
    /// </summary>
    public bool IsPredefined { get; set; }

    /// <summary>
    /// Icon or emoji representation for the mood (optional, for UI display).
    /// </summary>
    [MaxLength(10)]
    public string? Icon { get; set; }
}

/// <summary>
/// Enumeration of predefined mood categories.
/// </summary>
public enum MoodCategory
{
    Positive,
    Neutral,
    Negative
}

/// <summary>
/// Static helper class for managing predefined moods.
/// </summary>
public static class PredefinedMoods
{
    /// <summary>
    /// Gets all predefined moods organized by category.
    /// </summary>
    public static List<Mood> GetAllPredefinedMoods()
    {
        return new List<Mood>
        {
            // Positive moods
            new Mood { Name = "Happy", Category = "Positive", IsPredefined = true, Icon = "😊" },
            new Mood { Name = "Excited", Category = "Positive", IsPredefined = true, Icon = "🤩" },
            new Mood { Name = "Relaxed", Category = "Positive", IsPredefined = true, Icon = "😌" },
            new Mood { Name = "Grateful", Category = "Positive", IsPredefined = true, Icon = "🙏" },
            new Mood { Name = "Confident", Category = "Positive", IsPredefined = true, Icon = "💪" },

            // Neutral moods
            new Mood { Name = "Calm", Category = "Neutral", IsPredefined = true, Icon = "😐" },
            new Mood { Name = "Thoughtful", Category = "Neutral", IsPredefined = true, Icon = "🤔" },
            new Mood { Name = "Curious", Category = "Neutral", IsPredefined = true, Icon = "🧐" },
            new Mood { Name = "Nostalgic", Category = "Neutral", IsPredefined = true, Icon = "😊" },
            new Mood { Name = "Bored", Category = "Neutral", IsPredefined = true, Icon = "😑" },

            // Negative moods
            new Mood { Name = "Sad", Category = "Negative", IsPredefined = true, Icon = "😢" },
            new Mood { Name = "Angry", Category = "Negative", IsPredefined = true, Icon = "😠" },
            new Mood { Name = "Stressed", Category = "Negative", IsPredefined = true, Icon = "😰" },
            new Mood { Name = "Lonely", Category = "Negative", IsPredefined = true, Icon = "😔" },
            new Mood { Name = "Anxious", Category = "Negative", IsPredefined = true, Icon = "😟" }
        };
    }
}

