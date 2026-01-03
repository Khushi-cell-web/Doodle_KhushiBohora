using Doodle.Models;
using Doodle.Data;
using Doodle.Services;

namespace Doodle.Services;

/// <summary>
/// Service responsible for calculating and tracking journaling streaks.
/// A streak is a consecutive sequence of days with journal entries.
/// </summary>
public class StreakService
{
    private readonly DatabaseService _databaseService;
    private readonly JournalService _journalService;

    /// <summary>
    /// Initializes a new instance of the StreakService.
    /// </summary>
    /// <param name="databaseService">The database service for data access.</param>
    /// <param name="journalService">The journal service for entry retrieval.</param>
    public StreakService(DatabaseService databaseService, JournalService journalService)
    {
        _databaseService = databaseService;
        _journalService = journalService;
    }

    /// <summary>
    /// Calculates the current journaling streak (consecutive days with entries ending today or yesterday).
    /// </summary>
    /// <returns>The current streak count in days.</returns>
    public async Task<int> GetCurrentStreakAsync()
    {
        var entryDates = await _journalService.GetEntryDatesAsync();
        if (entryDates.Count == 0)
            return 0;

        // Sort dates in descending order (newest first)
        var sortedDates = entryDates.OrderByDescending(d => d).ToList();

        // Start from today or yesterday (if today has no entry, start from yesterday)
        var today = DateTime.Today;
        var checkDate = sortedDates.Contains(today) ? today : today.AddDays(-1);

        int streak = 0;
        var currentDate = checkDate;

        // Count consecutive days backwards
        while (sortedDates.Contains(currentDate))
        {
            streak++;
            currentDate = currentDate.AddDays(-1);
        }

        // If we started from yesterday and today has no entry, the streak is valid
        // If we started from today, the streak includes today
        return streak;
    }

    /// <summary>
    /// Calculates the longest streak ever achieved.
    /// </summary>
    /// <returns>The longest streak count in days.</returns>
    public async Task<int> GetLongestStreakAsync()
    {
        var entryDates = await _journalService.GetEntryDatesAsync();
        if (entryDates.Count == 0)
            return 0;

        // Sort dates in ascending order (oldest first)
        var sortedDates = entryDates.OrderBy(d => d).ToList();

        int longestStreak = 0;
        int currentStreak = 1;

        // Iterate through dates and find the longest consecutive sequence
        for (int i = 1; i < sortedDates.Count; i++)
        {
            var previousDate = sortedDates[i - 1];
            var currentDate = sortedDates[i];

            // Check if dates are consecutive
            if (currentDate == previousDate.AddDays(1))
            {
                currentStreak++;
            }
            else
            {
                // Streak broken, update longest if needed
                if (currentStreak > longestStreak)
                {
                    longestStreak = currentStreak;
                }
                currentStreak = 1;
            }
        }

        // Check the last streak
        if (currentStreak > longestStreak)
        {
            longestStreak = currentStreak;
        }

        return longestStreak;
    }

    /// <summary>
    /// Gets all dates that are missing entries (dates between the first and last entry that don't have entries).
    /// </summary>
    /// <returns>A list of dates that are missing entries.</returns>
    public async Task<List<DateTime>> GetMissedDaysAsync()
    {
        var entryDates = await _journalService.GetEntryDatesAsync();
        if (entryDates.Count == 0)
            return new List<DateTime>();

        var sortedDates = entryDates.OrderBy(d => d).ToList();
        var firstDate = sortedDates.First();
        var lastDate = sortedDates.Last();

        var missedDays = new List<DateTime>();
        var entryDateSet = new HashSet<DateTime>(sortedDates);

        // Check all dates between first and last
        var currentDate = firstDate.AddDays(1);
        while (currentDate < lastDate)
        {
            if (!entryDateSet.Contains(currentDate))
            {
                missedDays.Add(currentDate);
            }
            currentDate = currentDate.AddDays(1);
        }

        return missedDays;
    }

    /// <summary>
    /// Gets streak statistics including current streak, longest streak, and total entries.
    /// </summary>
    /// <returns>A tuple containing current streak, longest streak, total entries, and missed days count.</returns>
    public async Task<(int CurrentStreak, int LongestStreak, int TotalEntries, int MissedDays)> GetStreakStatsAsync()
    {
        var currentStreak = await GetCurrentStreakAsync();
        var longestStreak = await GetLongestStreakAsync();
        var allEntries = await _journalService.GetAllEntriesAsync();
        var missedDays = await GetMissedDaysAsync();

        return (currentStreak, longestStreak, allEntries.Count, missedDays.Count);
    }

    /// <summary>
    /// Checks if a specific date has a journal entry.
    /// </summary>
    /// <param name="date">The date to check.</param>
    /// <returns>True if an entry exists for the date, false otherwise.</returns>
    public async Task<bool> HasEntryForDateAsync(DateTime date)
    {
        var entry = await _journalService.GetEntryByDateAsync(date);
        return entry != null;
    }
}

