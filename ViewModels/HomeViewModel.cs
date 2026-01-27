using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Doodle.Models;
using Doodle.Services;

namespace Doodle.ViewModels;

/// <summary>
/// ViewModel for the Home page with pagination logic for journal entries.
/// </summary>
public partial class HomeViewModel : ObservableObject
{
    private readonly JournalService _journalService;

    // Pagination constants
    private const int FirstPageEntryCount = 3;
    private const int SubsequentPageEntryCount = 5;

    [ObservableProperty]
    private List<JournalEntry> _allEntries = new();

    [ObservableProperty]
    private List<JournalEntry> _currentPageEntries = new();

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages = 1;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _hasNextPage;

    [ObservableProperty]
    private bool _hasPreviousPage;

    /// <summary>
    /// Initializes a new instance of the HomeViewModel.
    /// </summary>
    public HomeViewModel(JournalService journalService)
    {
        _journalService = journalService;
    }

    /// <summary>
    /// Loads all journal entries and initializes pagination.
    /// </summary>
    [RelayCommand]
    public async Task LoadEntriesAsync()
    {
        try
        {
            IsLoading = true;

            // Load all entries and sort by most recent first
            var entries = await _journalService.GetAllEntriesAsync();
            AllEntries = entries?.OrderByDescending(e => e.EntryDate)
                                 .ThenByDescending(e => e.CreatedAt)
                                 .ToList() ?? new List<JournalEntry>();

            // Calculate total pages
            CalculateTotalPages();

            // Load current page
            LoadCurrentPage();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading entries: {ex.Message}");
            AllEntries = new List<JournalEntry>();
            CurrentPageEntries = new List<JournalEntry>();
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Calculates the total number of pages based on pagination rules.
    /// </summary>
    private void CalculateTotalPages()
    {
        if (AllEntries.Count == 0)
        {
            TotalPages = 1;
            return;
        }

        // First page has 3 entries
        if (AllEntries.Count <= FirstPageEntryCount)
        {
            TotalPages = 1;
            return;
        }

        // Remaining entries after first page
        int remainingEntries = AllEntries.Count - FirstPageEntryCount;
        
        // Calculate pages for remaining entries (5 per page)
        int additionalPages = (int)Math.Ceiling(remainingEntries / (double)SubsequentPageEntryCount);
        
        TotalPages = 1 + additionalPages; // First page + additional pages
    }

    /// <summary>
    /// Loads entries for the current page based on pagination rules.
    /// </summary>
    private void LoadCurrentPage()
    {
        if (AllEntries.Count == 0)
        {
            CurrentPageEntries = new List<JournalEntry>();
            UpdatePaginationButtons();
            return;
        }

        int startIndex = 0;
        int count = 0;

        if (CurrentPage == 1)
        {
            // First page: 3 entries
            startIndex = 0;
            count = Math.Min(FirstPageEntryCount, AllEntries.Count);
        }
        else
        {
            // Subsequent pages: 5 entries
            // Skip first page entries (3) + entries from previous pages (5 each)
            int entriesToSkip = FirstPageEntryCount + (CurrentPage - 2) * SubsequentPageEntryCount;
            startIndex = entriesToSkip;
            count = Math.Min(SubsequentPageEntryCount, AllEntries.Count - entriesToSkip);
        }

        CurrentPageEntries = AllEntries.Skip(startIndex).Take(count).ToList();
        UpdatePaginationButtons();
    }

    /// <summary>
    /// Updates the state of pagination buttons.
    /// </summary>
    private void UpdatePaginationButtons()
    {
        HasNextPage = CurrentPage < TotalPages;
        HasPreviousPage = CurrentPage > 1;
    }

    /// <summary>
    /// Navigates to the next page.
    /// </summary>
    [RelayCommand]
    public void NextPage()
    {
        if (HasNextPage)
        {
            CurrentPage++;
            LoadCurrentPage();
        }
    }

    /// <summary>
    /// Navigates to the previous page.
    /// </summary>
    [RelayCommand]
    public void PreviousPage()
    {
        if (HasPreviousPage)
        {
            CurrentPage--;
            LoadCurrentPage();
        }
    }

    /// <summary>
    /// Checks if an entry is from today and can be edited.
    /// </summary>
    public bool IsTodayEntry(JournalEntry entry)
    {
        return entry?.EntryDate.Date == DateTime.Today;
    }

    /// <summary>
    /// Deletes a journal entry by its ID.
    /// </summary>
    /// <param name="entryId">The ID of the entry to delete.</param>
    [RelayCommand]
    public async Task DeleteEntryAsync(int entryId)
    {
        try
        {
            await _journalService.DeleteEntryAsync(entryId);
            
            // Reload entries to reflect the deletion
            await LoadEntriesAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error deleting entry: {ex.Message}");
            throw;
        }
    }
}
