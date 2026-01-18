using Doodle.Models;
using Doodle.Data;

namespace Doodle.Services;

/// <summary>
/// Service for analytics operations including mood history tracking.
/// </summary>
public class AnalyticsService
{
    private readonly DatabaseService _databaseService;
    private readonly JournalService _journalService;
    private readonly MoodService _moodService;

    public AnalyticsService(DatabaseService databaseService, JournalService journalService, MoodService moodService)
    {
        _databaseService = databaseService;
        _journalService = journalService;
        _moodService = moodService;
    }

    /// <summary>
    /// Gets mood history data for the specified date range.
    /// Returns primary mood for each entry date.
    /// </summary>
    /// <param name="startDate">Start date of the range (optional).</param>
    /// <param name="endDate">End date of the range (optional).</param>
    /// <returns>List of mood history points with date and mood category.</returns>
    public async Task<List<MoodHistoryPoint>> GetMoodHistoryAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            // Get all entries
            var allEntries = await _journalService.GetAllEntriesAsync();

            // Filter by date range if provided
            List<JournalEntry> filteredEntries = allEntries;
            if (startDate.HasValue || endDate.HasValue)
            {
                var start = startDate ?? DateTime.MinValue;
                var end = endDate ?? DateTime.MaxValue;
                filteredEntries = allEntries
                    .Where(e => e.EntryDate >= start.Date && e.EntryDate <= end.Date)
                    .OrderBy(e => e.EntryDate)
                    .ToList();
            }
            else
            {
                filteredEntries = allEntries.OrderBy(e => e.EntryDate).ToList();
            }

            var moodHistory = new List<MoodHistoryPoint>();

            // Get primary mood for each entry
            foreach (var entry in filteredEntries)
            {
                var primaryMood = await _moodService.GetPrimaryMoodAsync(entry.Id);
                
                if (primaryMood != null)
                {
                    moodHistory.Add(new MoodHistoryPoint
                    {
                        Date = entry.EntryDate,
                        MoodCategory = primaryMood.Category,
                        MoodName = primaryMood.Name,
                        MoodIcon = primaryMood.Icon
                    });
                }
            }

            return moodHistory;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting mood history: {ex.Message}");
            return new List<MoodHistoryPoint>();
        }
    }

    /// <summary>
    /// Gets mood history for a quick date range (7, 30, or 90 days).
    /// </summary>
    /// <param name="days">Number of days to look back (7, 30, or 90).</param>
    /// <returns>List of mood history points.</returns>
    public async Task<List<MoodHistoryPoint>> GetMoodHistoryForDaysAsync(int days)
    {
        var endDate = DateTime.Today;
        var startDate = endDate.AddDays(-days);
        return await GetMoodHistoryAsync(startDate, endDate);
    }
}

/// <summary>
/// Represents a single point in mood history.
/// </summary>
public class MoodHistoryPoint
{
    public DateTime Date { get; set; }
    public string MoodCategory { get; set; } = string.Empty; // Positive, Neutral, or Negative
    public string MoodName { get; set; } = string.Empty;
    public string? MoodIcon { get; set; }
}
