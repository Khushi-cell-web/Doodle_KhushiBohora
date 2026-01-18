using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Doodle.Models;
using Doodle.Services;

namespace Doodle.ViewModels;

/// <summary>
/// ViewModel for the Dashboard page with analytics and statistics.
/// </summary>
public partial class DashboardViewModel : ObservableObject
{
    private readonly JournalService _journalService;
    private readonly MoodService _moodService;
    private readonly TagService _tagService;
    private readonly CategoryService _categoryService;
    private readonly StreakService _streakService;
    private readonly AnalyticsService _analyticsService;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private int _totalEntries;

    [ObservableProperty]
    private int _currentStreak;

    [ObservableProperty]
    private int _longestStreak;

    [ObservableProperty]
    private Dictionary<string, int> _moodDistribution = new();

    [ObservableProperty]
    private Mood? _mostFrequentMood;

    [ObservableProperty]
    private Dictionary<string, int> _tagUsage = new();

    [ObservableProperty]
    private Dictionary<string, int> _categoryBreakdown = new();

    [ObservableProperty]
    private List<(DateTime Date, int WordCount)> _wordCountTrends = new();

    [ObservableProperty]
    private DateTime? _analyticsStartDate;

    [ObservableProperty]
    private DateTime? _analyticsEndDate;

    [ObservableProperty]
    private double _averageWordCount;

    [ObservableProperty]
    private int _totalWords;

    [ObservableProperty]
    private List<MoodHistoryPoint> _moodHistory = new();

    [ObservableProperty]
    private int _selectedDateRangeDays = 30; // Default to 30 days

    public DashboardViewModel(JournalService journalService, MoodService moodService, TagService tagService, CategoryService categoryService, StreakService streakService, AnalyticsService analyticsService)
    {
        _journalService = journalService;
        _moodService = moodService;
        _tagService = tagService;
        _categoryService = categoryService;
        _streakService = streakService;
        _analyticsService = analyticsService;
    }

    /// <summary>
    /// Loads all dashboard analytics.
    /// </summary>
    [RelayCommand]
    public async Task LoadAnalyticsAsync()
    {
        try
        {
            IsLoading = true;

            // Load basic stats
            var allEntries = await _journalService.GetAllEntriesAsync();
            TotalEntries = allEntries.Count;

            // Load streak stats
            var streakStats = await _streakService.GetStreakStatsAsync();
            CurrentStreak = streakStats.CurrentStreak;
            LongestStreak = streakStats.LongestStreak;

            // Load mood history
            await LoadMoodHistoryAsync();

            // Filter entries by date range if specified
            List<JournalEntry> entriesForAnalysis = allEntries;
            if (AnalyticsStartDate.HasValue || AnalyticsEndDate.HasValue)
            {
                var start = AnalyticsStartDate ?? DateTime.MinValue;
                var end = AnalyticsEndDate ?? DateTime.MaxValue;
                entriesForAnalysis = allEntries
                    .Where(e => e.EntryDate >= start.Date && e.EntryDate <= end.Date)
                    .ToList();
            }

            // Calculate mood distribution
            await CalculateMoodDistributionAsync(entriesForAnalysis);

            // Calculate tag usage
            await CalculateTagUsageAsync(entriesForAnalysis);

            // Calculate category breakdown
            await CalculateCategoryBreakdownAsync(entriesForAnalysis);

            // Calculate word count trends
            CalculateWordCountTrends(entriesForAnalysis);

            // Calculate averages
            if (entriesForAnalysis.Any())
            {
                TotalWords = entriesForAnalysis.Sum(e => e.WordCount);
                AverageWordCount = entriesForAnalysis.Average(e => e.WordCount);
            }
            else
            {
                TotalWords = 0;
                AverageWordCount = 0;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading analytics: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Calculates mood distribution from entries.
    /// </summary>
    private async Task CalculateMoodDistributionAsync(List<JournalEntry> entries)
    {
        var moodCounts = new Dictionary<string, int>();
        var moodFrequency = new Dictionary<int, int>(); // moodId -> count

        foreach (var entry in entries)
        {
            var (primary, secondary) = await _moodService.GetEntryMoodsAsync(entry.Id);
            
            if (primary != null)
            {
                if (moodFrequency.ContainsKey(primary.Id))
                {
                    moodFrequency[primary.Id]++;
                }
                else
                {
                    moodFrequency[primary.Id] = 1;
                }
            }

            foreach (var mood in secondary)
            {
                if (moodFrequency.ContainsKey(mood.Id))
                {
                    moodFrequency[mood.Id]++;
                }
                else
                {
                    moodFrequency[mood.Id] = 1;
                }
            }
        }

        // Convert to mood name -> count
        var allMoods = await _moodService.GetAllMoodsAsync();
        MoodDistribution = new Dictionary<string, int>();
        int maxCount = 0;
        Mood? mostFrequent = null;

        foreach (var kvp in moodFrequency)
        {
            var mood = allMoods.FirstOrDefault(m => m.Id == kvp.Key);
            if (mood != null)
            {
                MoodDistribution[mood.Name] = kvp.Value;
                if (kvp.Value > maxCount)
                {
                    maxCount = kvp.Value;
                    mostFrequent = mood;
                }
            }
        }

        MostFrequentMood = mostFrequent;
    }

    /// <summary>
    /// Calculates tag usage statistics.
    /// </summary>
    private async Task CalculateTagUsageAsync(List<JournalEntry> entries)
    {
        var tagCounts = new Dictionary<int, int>(); // tagId -> count

        foreach (var entry in entries)
        {
            var tags = await _tagService.GetEntryTagsAsync(entry.Id);
            foreach (var tag in tags)
            {
                if (tagCounts.ContainsKey(tag.Id))
                {
                    tagCounts[tag.Id]++;
                }
                else
                {
                    tagCounts[tag.Id] = 1;
                }
            }
        }

        // Convert to tag name -> count
        var allTags = await _tagService.GetAllTagsAsync();
        TagUsage = new Dictionary<string, int>();
        foreach (var kvp in tagCounts)
        {
            var tag = allTags.FirstOrDefault(t => t.Id == kvp.Key);
            if (tag != null)
            {
                TagUsage[tag.Name] = kvp.Value;
            }
        }
    }

    /// <summary>
    /// Calculates category breakdown.
    /// </summary>
    private async Task CalculateCategoryBreakdownAsync(List<JournalEntry> entries)
    {
        CategoryBreakdown = await _categoryService.GetCategoryCountsAsync();
        
        // Filter to only entries in our analysis set
        var filteredBreakdown = new Dictionary<string, int>();
        foreach (var entry in entries)
        {
            var category = entry.Category ?? "Uncategorized";
            if (filteredBreakdown.ContainsKey(category))
            {
                filteredBreakdown[category]++;
            }
            else
            {
                filteredBreakdown[category] = 1;
            }
        }
        CategoryBreakdown = filteredBreakdown;
    }

    /// <summary>
    /// Calculates word count trends over time.
    /// </summary>
    private void CalculateWordCountTrends(List<JournalEntry> entries)
    {
        WordCountTrends = entries
            .OrderBy(e => e.EntryDate)
            .Select(e => (e.EntryDate, e.WordCount))
            .ToList();
    }

    /// <summary>
    /// Loads mood history data based on selected date range.
    /// </summary>
    private async Task LoadMoodHistoryAsync()
    {
        try
        {
            if (SelectedDateRangeDays > 0)
            {
                MoodHistory = await _analyticsService.GetMoodHistoryForDaysAsync(SelectedDateRangeDays);
            }
            else if (AnalyticsStartDate.HasValue || AnalyticsEndDate.HasValue)
            {
                MoodHistory = await _analyticsService.GetMoodHistoryAsync(AnalyticsStartDate, AnalyticsEndDate);
            }
            else
            {
                MoodHistory = await _analyticsService.GetMoodHistoryAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading mood history: {ex.Message}");
            MoodHistory = new List<MoodHistoryPoint>();
        }
    }

    /// <summary>
    /// Updates the date range for mood history and reloads data.
    /// </summary>
    public async Task UpdateMoodHistoryDateRangeAsync(int days)
    {
        SelectedDateRangeDays = days;
        await LoadMoodHistoryAsync();
    }
}
