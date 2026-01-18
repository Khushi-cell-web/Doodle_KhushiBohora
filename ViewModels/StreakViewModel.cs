using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Doodle.Services;

namespace Doodle.ViewModels;

/// <summary>
/// ViewModel for displaying streak statistics.
/// Shows current streak, longest streak, and missed days information.
/// </summary>
public partial class StreakViewModel : ObservableObject
{
    private readonly StreakService _streakService;
    private readonly JournalService _journalService;

    [ObservableProperty]
    private int _currentStreak;

    [ObservableProperty]
    private int _longestStreak;

    [ObservableProperty]
    private int _totalEntries;

    [ObservableProperty]
    private int _missedDays;

    [ObservableProperty]
    private int _totalWords;

    [ObservableProperty]
    private int _averageWords;

    [ObservableProperty]
    private bool _isLoading;

    /// <summary>
    /// Initializes a new instance of the StreakViewModel.
    /// </summary>
    public StreakViewModel(StreakService streakService, JournalService journalService)
    {
        _streakService = streakService;
        _journalService = journalService;
    }

    /// <summary>
    /// Loads current streak statistics.
    /// </summary>
    [RelayCommand]
    public async Task LoadStreakStatsAsync()
    {
        try
        {
            IsLoading = true;
            var stats = await _streakService.GetStreakStatsAsync();
            CurrentStreak = stats.CurrentStreak;
            LongestStreak = stats.LongestStreak;
            TotalEntries = stats.TotalEntries;
            MissedDays = stats.MissedDays;

            // Calculate word counts
            var allEntries = await _journalService.GetAllEntriesAsync();
            if (allEntries.Any())
            {
                TotalWords = allEntries.Sum(e => e.WordCount);
                AverageWords = (int)Math.Round(allEntries.Average(e => e.WordCount));
            }
            else
            {
                TotalWords = 0;
                AverageWords = 0;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading streak stats: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
}

