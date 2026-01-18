using SQLite;
using Doodle.Models;
using Doodle.Data;

namespace Doodle.Services;

/// <summary>
/// Service responsible for managing categories and their associations with journal entries.
/// </summary>
public class CategoryService
{
    private readonly DatabaseService _databaseService;

    /// <summary>
    /// Initializes a new instance of the CategoryService.
    /// </summary>
    /// <param name="databaseService">The database service for data access.</param>
    public CategoryService(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    /// <summary>
    /// Gets all available categories.
    /// </summary>
    /// <returns>A list of all categories, ordered by name.</returns>
    public async Task<List<Category>> GetAllCategoriesAsync()
    {
        return await _databaseService.Database.Table<Category>()
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all predefined categories.
    /// </summary>
    /// <returns>A list of predefined categories.</returns>
    public async Task<List<Category>> GetPredefinedCategoriesAsync()
    {
        return await _databaseService.Database.Table<Category>()
            .Where(c => c.IsPredefined)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Gets a category by its ID.
    /// </summary>
    /// <param name="id">The ID of the category.</param>
    /// <returns>The category, or null if not found.</returns>
    public async Task<Category?> GetCategoryByIdAsync(int id)
    {
        return await _databaseService.Database.Table<Category>()
            .Where(c => c.Id == id)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Gets a category by its name.
    /// </summary>
    /// <param name="name">The name of the category.</param>
    /// <returns>The category, or null if not found.</returns>
    public async Task<Category?> GetCategoryByNameAsync(string name)
    {
        return await _databaseService.Database.Table<Category>()
            .Where(c => c.Name == name)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Creates a new custom category. If a category with the same name already exists, returns the existing category.
    /// </summary>
    /// <param name="category">The category to create.</param>
    /// <returns>The created or existing category.</returns>
    public async Task<Category> CreateCategoryAsync(Category category)
    {
        // Check if category with same name already exists
        var existing = await GetCategoryByNameAsync(category.Name);
        if (existing != null)
        {
            return existing;
        }

        category.IsPredefined = false;
        category.CreatedAt = DateTime.UtcNow;
        await _databaseService.Database.InsertAsync(category);
        return category;
    }

    /// <summary>
    /// Gets all journal entries that have a specific category.
    /// </summary>
    /// <param name="categoryName">The name of the category.</param>
    /// <returns>A list of journal entries with this category.</returns>
    public async Task<List<JournalEntry>> GetEntriesByCategoryAsync(string categoryName)
    {
        return await _databaseService.Database.Table<JournalEntry>()
            .Where(e => e.Category == categoryName)
            .OrderByDescending(e => e.EntryDate)
            .ToListAsync();
    }

    /// <summary>
    /// Gets the count of entries for each category.
    /// </summary>
    /// <returns>A dictionary mapping category names to entry counts.</returns>
    public async Task<Dictionary<string, int>> GetCategoryCountsAsync()
    {
        var entries = await _databaseService.Database.Table<JournalEntry>()
            .ToListAsync();

        var categoryCounts = new Dictionary<string, int>();
        foreach (var entry in entries)
        {
            var category = entry.Category ?? "Uncategorized";
            if (categoryCounts.ContainsKey(category))
            {
                categoryCounts[category]++;
            }
            else
            {
                categoryCounts[category] = 1;
            }
        }

        return categoryCounts;
    }
}
