using Doodle.Models;
using Doodle.Data;

namespace Doodle.Services;

/// <summary>
/// Service responsible for managing moods and their associations with journal entries.
/// Handles primary and secondary mood assignments (one primary, up to two secondary).
/// </summary>
public class MoodService
{
    private readonly DatabaseService _databaseService;

    /// <summary>
    /// Initializes a new instance of the MoodService.
    /// </summary>
    /// <param name="databaseService">The database service for data access.</param>
    public MoodService(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    /// <summary>
    /// Gets all available moods.
    /// </summary>
    /// <returns>A list of all moods, ordered by category and name.</returns>
    public async Task<List<Mood>> GetAllMoodsAsync()
    {
        return await _databaseService.Database.Table<Mood>()
            .OrderBy(m => m.Category)
            .ThenBy(m => m.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all predefined moods.
    /// </summary>
    /// <returns>A list of predefined moods.</returns>
    public async Task<List<Mood>> GetPredefinedMoodsAsync()
    {
        return await _databaseService.Database.Table<Mood>()
            .Where(m => m.IsPredefined)
            .OrderBy(m => m.Category)
            .ThenBy(m => m.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Gets moods by category.
    /// </summary>
    /// <param name="category">The mood category (Positive, Neutral, Negative).</param>
    /// <returns>A list of moods in the specified category.</returns>
    public async Task<List<Mood>> GetMoodsByCategoryAsync(string category)
    {
        return await _databaseService.Database.Table<Mood>()
            .Where(m => m.Category == category)
            .OrderBy(m => m.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Creates a custom mood (user-defined).
    /// </summary>
    /// <param name="mood">The mood to create.</param>
    /// <returns>The created mood with generated Id.</returns>
    public async Task<Mood> CreateCustomMoodAsync(Mood mood)
    {
        mood.IsPredefined = false;
        await _databaseService.Database.InsertAsync(mood);
        return mood;
    }

    /// <summary>
    /// Gets the primary mood for a journal entry.
    /// </summary>
    /// <param name="entryId">The ID of the journal entry.</param>
    /// <returns>The primary mood, or null if not set.</returns>
    public async Task<Mood?> GetPrimaryMoodAsync(int entryId)
    {
        var entryMood = await _databaseService.Database.Table<EntryMood>()
            .Where(em => em.JournalEntryId == entryId && em.IsPrimary)
            .FirstOrDefaultAsync();

        if (entryMood == null)
            return null;

        return await _databaseService.Database.Table<Mood>()
            .Where(m => m.Id == entryMood.MoodId)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Gets the secondary moods for a journal entry (up to two).
    /// </summary>
    /// <param name="entryId">The ID of the journal entry.</param>
    /// <returns>A list of secondary moods, ordered by SecondaryOrder.</returns>
    public async Task<List<Mood>> GetSecondaryMoodsAsync(int entryId)
    {
        var entryMoods = await _databaseService.Database.Table<EntryMood>()
            .Where(em => em.JournalEntryId == entryId && !em.IsPrimary)
            .OrderBy(em => em.SecondaryOrder)
            .ToListAsync();

        var moods = new List<Mood>();
        foreach (var entryMood in entryMoods)
        {
            var mood = await _databaseService.Database.Table<Mood>()
                .Where(m => m.Id == entryMood.MoodId)
                .FirstOrDefaultAsync();
            if (mood != null)
            {
                moods.Add(mood);
            }
        }

        return moods;
    }

    /// <summary>
    /// Gets all moods (primary and secondary) for a journal entry.
    /// </summary>
    /// <param name="entryId">The ID of the journal entry.</param>
    /// <returns>A tuple containing the primary mood and list of secondary moods.</returns>
    public async Task<(Mood? Primary, List<Mood> Secondary)> GetEntryMoodsAsync(int entryId)
    {
        var primary = await GetPrimaryMoodAsync(entryId);
        var secondary = await GetSecondaryMoodsAsync(entryId);
        return (primary, secondary);
    }

    /// <summary>
    /// Sets the moods for a journal entry. Replaces any existing mood associations.
    /// </summary>
    /// <param name="entryId">The ID of the journal entry.</param>
    /// <param name="primaryMoodId">The ID of the primary mood (required).</param>
    /// <param name="secondaryMoodIds">The IDs of secondary moods (optional, max 2).</param>
    public async Task SetEntryMoodsAsync(int entryId, int primaryMoodId, List<int>? secondaryMoodIds = null)
    {
        // Remove existing mood associations
        await _databaseService.Database.Table<EntryMood>()
            .Where(em => em.JournalEntryId == entryId)
            .DeleteAsync();

        // Add primary mood
        var primaryEntryMood = new EntryMood
        {
            JournalEntryId = entryId,
            MoodId = primaryMoodId,
            IsPrimary = true,
            SecondaryOrder = 0
        };
        await _databaseService.Database.InsertAsync(primaryEntryMood);

        // Add secondary moods (limit to 2)
        if (secondaryMoodIds != null && secondaryMoodIds.Count > 0)
        {
            var secondaryList = secondaryMoodIds.Take(2).ToList();
            for (int i = 0; i < secondaryList.Count; i++)
            {
                var secondaryEntryMood = new EntryMood
                {
                    JournalEntryId = entryId,
                    MoodId = secondaryList[i],
                    IsPrimary = false,
                    SecondaryOrder = i + 1
                };
                await _databaseService.Database.InsertAsync(secondaryEntryMood);
            }
        }
    }

    /// <summary>
    /// Gets all journal entries that have a specific mood.
    /// </summary>
    /// <param name="moodId">The ID of the mood.</param>
    /// <returns>A list of journal entry IDs that have this mood.</returns>
    public async Task<List<int>> GetEntryIdsByMoodAsync(int moodId)
    {
        var entryMoods = await _databaseService.Database.Table<EntryMood>()
            .Where(em => em.MoodId == moodId)
            .ToListAsync();

        return entryMoods.Select(em => em.JournalEntryId).Distinct().ToList();
    }
}

