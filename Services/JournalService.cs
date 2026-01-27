using SQLite;
using Doodle.Models;
using Doodle.Data;

namespace Doodle.Services;

/// <summary>
/// Service responsible for managing journal entries.
/// Enforces the business rule that only one entry per calendar day is allowed.
/// </summary>
public class JournalService
{
    private readonly DatabaseService _databaseService;

    /// <summary>
    /// Initializes a new instance of the JournalService.
    /// </summary>
    /// <param name="databaseService">The database service for data access.</param>
    public JournalService(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    /// <summary>
    /// Creates a new journal entry. Enforces the one-entry-per-day rule.
    /// </summary>
    /// <param name="entry">The journal entry to create.</param>
    /// <returns>The created entry with generated Id and timestamps.</returns>
    /// <exception cref="InvalidOperationException">Thrown if an entry already exists for the specified date.</exception>
    public async Task<JournalEntry> CreateEntryAsync(JournalEntry entry)
    {
        // Normalize the entry date to midnight (date only, no time)
        var entryDate = entry.EntryDate.Date;

        // Check if an entry already exists for this date
        var existingEntry = await GetEntryByDateAsync(entryDate);
        if (existingEntry != null)
        {
            throw new InvalidOperationException($"An entry already exists for {entryDate:yyyy-MM-dd}. Only one entry per day is allowed.");
        }

        // Set timestamps
        entry.EntryDate = entryDate;
        entry.CreatedAt = DateTime.UtcNow;
        entry.UpdatedAt = DateTime.UtcNow;

        // Calculate word count
        entry.CalculateWordCount();

        // Insert into database
        await _databaseService.Database.InsertAsync(entry);

        return entry;
    }

    /// <summary>
    /// Updates an existing journal entry.
    /// </summary>
    /// <param name="entry">The journal entry to update.</param>
    /// <returns>The updated entry.</returns>
    public async Task<JournalEntry> UpdateEntryAsync(JournalEntry entry)
    {
        // Update timestamp
        entry.UpdatedAt = DateTime.UtcNow;

        // Recalculate word count
        entry.CalculateWordCount();

        // Update in database
        await _databaseService.Database.UpdateAsync(entry);

        return entry;
    }

    /// <summary>
    /// Deletes a journal entry by its ID.
    /// Also removes associated moods and tags.
    /// </summary>
    /// <param name="id">The ID of the entry to delete.</param>
    public async Task DeleteEntryAsync(int id)
    {
        // Delete associated moods
        await _databaseService.Database.Table<EntryMood>()
            .Where(em => em.JournalEntryId == id)
            .DeleteAsync();

        // Delete associated tags
        await _databaseService.Database.Table<EntryTag>()
            .Where(et => et.JournalEntryId == id)
            .DeleteAsync();

        // Delete the entry
        await _databaseService.Database.DeleteAsync<JournalEntry>(id);
    }

    /// <summary>
    /// Gets a journal entry by its ID.
    /// </summary>
    /// <param name="id">The ID of the entry.</param>
    /// <returns>The journal entry, or null if not found.</returns>
    public async Task<JournalEntry?> GetEntryByIdAsync(int id)
    {
        return await _databaseService.Database.Table<JournalEntry>()
            .Where(e => e.Id == id)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Gets a journal entry by its date. Returns null if no entry exists for that date.
    /// </summary>
    /// <param name="date">The date to search for (time component is ignored).</param>
    /// <returns>The journal entry for the specified date, or null if not found.</returns>
    public async Task<JournalEntry?> GetEntryByDateAsync(DateTime date)
    {
        var dateOnly = date.Date;
        return await _databaseService.Database.Table<JournalEntry>()
            .Where(e => e.EntryDate == dateOnly)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Gets all journal entries, ordered by date (newest first).
    /// </summary>
    /// <returns>A list of all journal entries.</returns>
    public async Task<List<JournalEntry>> GetAllEntriesAsync()
    {
        return await _databaseService.Database.Table<JournalEntry>()
            .OrderByDescending(e => e.EntryDate)
            .ToListAsync();
    }

    /// <summary>
    /// Gets journal entries within a date range.
    /// </summary>
    /// <param name="startDate">The start date (inclusive).</param>
    /// <param name="endDate">The end date (inclusive).</param>
    /// <returns>A list of entries within the date range.</returns>
    public async Task<List<JournalEntry>> GetEntriesByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        // Normalize dates to ensure only date comparison (no time component)
        var start = startDate.Date;
        var end = endDate.Date;
        
        System.Diagnostics.Debug.WriteLine($"GetEntriesByDateRangeAsync: Requested range from {start:yyyy-MM-dd} to {end:yyyy-MM-dd}");
        
        // Get all entries from database
        var allEntries = await _databaseService.Database.Table<JournalEntry>()
            .ToListAsync();
        
        System.Diagnostics.Debug.WriteLine($"GetEntriesByDateRangeAsync: Total entries in database: {allEntries.Count}");
        
        // Filter entries where EntryDate (normalized to date only) is within range
        var filteredEntries = allEntries
            .Where(e => 
            {
                // Normalize entry date to date only (remove time component)
                var entryDate = e.EntryDate.Date;
                
                // Check if entry date is within the range (inclusive on both ends)
                var isInRange = entryDate >= start && entryDate <= end;
                
                if (isInRange)
                {
                    System.Diagnostics.Debug.WriteLine($"Entry {e.Id}: EntryDate={entryDate:yyyy-MM-dd} is IN range");
                }
                
                return isInRange;
            })
            .OrderByDescending(e => e.EntryDate)
            .ToList();
        
        System.Diagnostics.Debug.WriteLine($"GetEntriesByDateRangeAsync: Found {filteredEntries.Count} entries in range {start:yyyy-MM-dd} to {end:yyyy-MM-dd}");
        
        // Log all entry dates for debugging
        foreach (var entry in allEntries)
        {
            System.Diagnostics.Debug.WriteLine($"All Entry {entry.Id}: EntryDate={entry.EntryDate:yyyy-MM-dd HH:mm:ss} (Date only: {entry.EntryDate.Date:yyyy-MM-dd})");
        }
        
        return filteredEntries;
    }

    /// <summary>
    /// Searches journal entries by title or content.
    /// </summary>
    /// <param name="searchTerm">The search term to match against title and content.</param>
    /// <returns>A list of matching entries.</returns>
    public async Task<List<JournalEntry>> SearchEntriesAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return await GetAllEntriesAsync();
        }

        var lowerSearchTerm = searchTerm.ToLowerInvariant();

        return await _databaseService.Database.Table<JournalEntry>()
            .Where(e => 
                (e.Title != null && e.Title.ToLower().Contains(lowerSearchTerm)) ||
                e.Content.ToLower().Contains(lowerSearchTerm))
            .OrderByDescending(e => e.EntryDate)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all dates that have journal entries.
    /// </summary>
    /// <returns>A list of dates (without time component) that have entries.</returns>
    public async Task<List<DateTime>> GetEntryDatesAsync()
    {
        var entries = await _databaseService.Database.Table<JournalEntry>()
            .ToListAsync();

        return entries.Select(e => e.EntryDate.Date).Distinct().OrderByDescending(d => d).ToList();
    }
}

