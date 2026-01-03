using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Doodle.Models;
using Doodle.Services;

namespace Doodle.ViewModels;

/// <summary>
/// ViewModel for the Calendar View page.
/// Manages calendar navigation and date selection for journal entries.
/// </summary>
public partial class CalendarViewModel : ObservableObject
{
    private readonly JournalService _journalService;

    [ObservableProperty]
    private DateTime _currentMonth = DateTime.Today;

    [ObservableProperty]
    private List<DateTime> _entryDates = new();

    [ObservableProperty]
    private DateTime? _selectedDate;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private JournalEntry? _selectedEntry;

    /// <summary>
    /// Initializes a new instance of the CalendarViewModel.
    /// </summary>
    public CalendarViewModel(JournalService journalService)
    {
        _journalService = journalService;
    }

    /// <summary>
    /// Loads entry dates for the current month view.
    /// </summary>
    [RelayCommand]
    public async Task LoadEntryDatesAsync()
    {
        try
        {
            IsLoading = true;
            EntryDates = await _journalService.GetEntryDatesAsync();
        }
        catch (Exception ex)
        {
            // Handle error (in production, use proper logging)
            System.Diagnostics.Debug.WriteLine($"Error loading entry dates: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Loads the journal entry for the selected date.
    /// </summary>
    [RelayCommand]
    public async Task LoadEntryForSelectedDateAsync()
    {
        if (SelectedDate == null)
        {
            SelectedEntry = null;
            return;
        }

        try
        {
            IsLoading = true;
            SelectedEntry = await _journalService.GetEntryByDateAsync(SelectedDate.Value);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading entry: {ex.Message}");
            SelectedEntry = null;
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Navigates to the previous month.
    /// </summary>
    [RelayCommand]
    public async Task NavigateToPreviousMonthAsync()
    {
        CurrentMonth = CurrentMonth.AddMonths(-1);
        await LoadEntryDatesAsync();
    }

    /// <summary>
    /// Navigates to the next month.
    /// </summary>
    [RelayCommand]
    public async Task NavigateToNextMonthAsync()
    {
        CurrentMonth = CurrentMonth.AddMonths(1);
        await LoadEntryDatesAsync();
    }

    /// <summary>
    /// Navigates to today's date.
    /// </summary>
    [RelayCommand]
    public async Task NavigateToTodayAsync()
    {
        CurrentMonth = DateTime.Today;
        SelectedDate = DateTime.Today;
        await LoadEntryDatesAsync();
        await LoadEntryForSelectedDateAsync();
    }

    /// <summary>
    /// Checks if a date has a journal entry.
    /// </summary>
    /// <param name="date">The date to check.</param>
    /// <returns>True if the date has an entry, false otherwise.</returns>
    public bool HasEntry(DateTime date)
    {
        return EntryDates.Any(d => d.Date == date.Date);
    }

    /// <summary>
    /// Handles date selection from the calendar.
    /// </summary>
    /// <param name="date">The selected date.</param>
    public async Task SelectDateAsync(DateTime date)
    {
        SelectedDate = date;
        await LoadEntryForSelectedDateAsync();
    }
}

