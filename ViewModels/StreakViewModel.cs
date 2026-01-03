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

    [ObservableProperty]
    private int _currentStreak;

    [ObservableProperty]
    private int _longestStreak;

    [ObservableProperty]
    private int _totalEntries;

    [ObservableProperty]
    private int _missedDays;

    [ObservableProperty]
    private bool _isLoading;

    /// <summary>
    /// Initializes a new instance of the StreakViewModel.
    /// </summary>
    public StreakViewModel(StreakService streakService)
    {
        _streakService = streakService;
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

