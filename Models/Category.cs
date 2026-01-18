using SQLite;

namespace Doodle.Models;

/// <summary>
/// Represents a category for organizing journal entries.
/// </summary>
[Table("Categories")]
public class Category
{
    /// <summary>
    /// Primary key for the category.
    /// </summary>
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    /// <summary>
    /// The name of the category (e.g., "Work", "Health", "Travel").
    /// </summary>
    [Unique, MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The color associated with the category (hex format, e.g., "#4A90E2").
    /// </summary>
    [MaxLength(10)]
    public string? Color { get; set; }

    /// <summary>
    /// Indicates whether this is a predefined category (true) or a user-created custom category (false).
    /// </summary>
    public bool IsPredefined { get; set; }

    /// <summary>
    /// Timestamp when the category was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Static helper class for managing predefined categories.
/// </summary>
public static class PredefinedCategories
{
    /// <summary>
    /// Gets all predefined categories.
    /// </summary>
    public static List<Category> GetAllPredefinedCategories()
    {
        return new List<Category>
        {
            new Category { Name = "Work", Color = "#4A90E2", IsPredefined = true, CreatedAt = DateTime.UtcNow },
            new Category { Name = "Health", Color = "#E24AE2", IsPredefined = true, CreatedAt = DateTime.UtcNow },
            new Category { Name = "Travel", Color = "#E2E24A", IsPredefined = true, CreatedAt = DateTime.UtcNow },
            new Category { Name = "Family", Color = "#E24A4A", IsPredefined = true, CreatedAt = DateTime.UtcNow },
            new Category { Name = "Friends", Color = "#4AE24A", IsPredefined = true, CreatedAt = DateTime.UtcNow },
            new Category { Name = "Hobby", Color = "#4AE2E2", IsPredefined = true, CreatedAt = DateTime.UtcNow },
            new Category { Name = "Education", Color = "#9B59B6", IsPredefined = true, CreatedAt = DateTime.UtcNow },
            new Category { Name = "Personal", Color = "#F39C12", IsPredefined = true, CreatedAt = DateTime.UtcNow }
        };
    }
}
