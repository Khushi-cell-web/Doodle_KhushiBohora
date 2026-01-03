using SQLite;
using Doodle.Models;
using System.IO;
using Microsoft.Maui.Storage;

namespace Doodle.Data;

/// <summary>
/// Service responsible for initializing and managing the SQLite database connection.
/// Handles database creation, table schema setup, and initial data seeding.
/// </summary>
public class DatabaseService
{
    private SQLiteAsyncConnection? _database;
    private readonly string _databasePath;

    /// <summary>
    /// Initializes a new instance of the DatabaseService.
    /// </summary>
    public DatabaseService()
    {
        // Determine the database path based on the platform
        var databaseFileName = "doodle.db3";
        
        // For MAUI, use FileSystem.AppDataDirectory which works across all platforms
        _databasePath = Path.Combine(FileSystem.AppDataDirectory, databaseFileName);
    }

    /// <summary>
    /// Gets the SQLite database connection. Creates the connection if it doesn't exist.
    /// </summary>
    public SQLiteAsyncConnection Database
    {
        get
        {
            if (_database == null)
            {
                _database = new SQLiteAsyncConnection(_databasePath);
            }
            return _database;
        }
    }

    /// <summary>
    /// Initializes the database by creating all tables and seeding initial data.
    /// This method should be called once during application startup.
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            // Create all tables
            await Database.CreateTableAsync<JournalEntry>();
            await Database.CreateTableAsync<Mood>();
            await Database.CreateTableAsync<Tag>();
            await Database.CreateTableAsync<EntryMood>();
            await Database.CreateTableAsync<EntryTag>();

            // Seed predefined moods if they don't exist
            await SeedPredefinedMoodsAsync();

            // Seed some predefined tags if they don't exist
            await SeedPredefinedTagsAsync();
        }
        catch (Exception ex)
        {
            // Log error (in production, use proper logging)
            System.Diagnostics.Debug.WriteLine($"Database initialization error: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Seeds the database with predefined moods if they don't already exist.
    /// </summary>
    private async Task SeedPredefinedMoodsAsync()
    {
        var existingMoods = await Database.Table<Mood>()
            .Where(m => m.IsPredefined)
            .ToListAsync();

        if (existingMoods.Count == 0)
        {
            var predefinedMoods = PredefinedMoods.GetAllPredefinedMoods();
            foreach (var mood in predefinedMoods)
            {
                await Database.InsertAsync(mood);
            }
        }
    }

    /// <summary>
    /// Seeds the database with some common predefined tags.
    /// </summary>
    private async Task SeedPredefinedTagsAsync()
    {
        var existingTags = await Database.Table<Tag>()
            .Where(t => t.IsPredefined)
            .ToListAsync();

        if (existingTags.Count == 0)
        {
            var predefinedTags = new List<Tag>
            {
                new Tag { Name = "Work", Color = "#4A90E2", IsPredefined = true, CreatedAt = DateTime.UtcNow },
                new Tag { Name = "Family", Color = "#E24A4A", IsPredefined = true, CreatedAt = DateTime.UtcNow },
                new Tag { Name = "Friends", Color = "#4AE24A", IsPredefined = true, CreatedAt = DateTime.UtcNow },
                new Tag { Name = "Travel", Color = "#E2E24A", IsPredefined = true, CreatedAt = DateTime.UtcNow },
                new Tag { Name = "Health", Color = "#E24AE2", IsPredefined = true, CreatedAt = DateTime.UtcNow },
                new Tag { Name = "Hobby", Color = "#4AE2E2", IsPredefined = true, CreatedAt = DateTime.UtcNow }
            };

            foreach (var tag in predefinedTags)
            {
                await Database.InsertAsync(tag);
            }
        }
    }

    /// <summary>
    /// Closes the database connection. Should be called when the application is shutting down.
    /// </summary>
    public async Task CloseAsync()
    {
        if (_database != null)
        {
            await _database.CloseAsync();
            _database = null;
        }
    }
}

