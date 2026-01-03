using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Doodle.Models;
using Doodle.Services;

namespace Doodle.ViewModels;

/// <summary>
/// ViewModel for the Search and Filter page.
/// Manages searching entries by text, date range, moods, and tags.
/// </summary>
public partial class SearchViewModel : ObservableObject
{
    private readonly JournalService _journalService;
    private readonly MoodService _moodService;
    private readonly TagService _tagService;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private DateTime? _startDate;

    [ObservableProperty]
    private DateTime? _endDate;

    [ObservableProperty]
    private List<Mood> _availableMoods = new();

    [ObservableProperty]
    private List<int> _selectedMoodIds = new();

    [ObservableProperty]
    private List<Tag> _availableTags = new();

    [ObservableProperty]
    private List<int> _selectedTagIds = new();

    [ObservableProperty]
    private List<JournalEntry> _filteredEntries = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private JournalEntry? _selectedEntry;

    /// <summary>
    /// Initializes a new instance of the SearchViewModel.
    /// </summary>
    public SearchViewModel(JournalService journalService, MoodService moodService, TagService tagService)
    {
        _journalService = journalService;
        _moodService = moodService;
        _tagService = tagService;
    }

    /// <summary>
    /// Loads available moods and tags for filtering.
    /// </summary>
    [RelayCommand]
    public async Task LoadFilterOptionsAsync()
    {
        try
        {
            IsLoading = true;
            AvailableMoods = await _moodService.GetAllMoodsAsync();
            AvailableTags = await _tagService.GetAllTagsAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading filter options: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Performs a search and filter operation based on current criteria.
    /// </summary>
    [RelayCommand]
    public async Task SearchAsync()
    {
        try
        {
            IsLoading = true;

            List<JournalEntry> results;

            // Start with text search if provided
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                results = await _journalService.SearchEntriesAsync(SearchText);
            }
            else
            {
                results = await _journalService.GetAllEntriesAsync();
            }

            // Filter by date range if provided
            if (StartDate.HasValue || EndDate.HasValue)
            {
                var start = StartDate ?? DateTime.MinValue;
                var end = EndDate ?? DateTime.MaxValue;
                results = results.Where(e => e.EntryDate >= start.Date && e.EntryDate <= end.Date).ToList();
            }

            // Filter by moods if selected
            if (SelectedMoodIds.Count > 0)
            {
                var entryIdsByMoods = new HashSet<int>();
                foreach (var moodId in SelectedMoodIds)
                {
                    var entryIds = await _moodService.GetEntryIdsByMoodAsync(moodId);
                    foreach (var entryId in entryIds)
                    {
                        entryIdsByMoods.Add(entryId);
                    }
                }
                results = results.Where(e => entryIdsByMoods.Contains(e.Id)).ToList();
            }

            // Filter by tags if selected
            if (SelectedTagIds.Count > 0)
            {
                var entryIdsByTags = new HashSet<int>();
                foreach (var tagId in SelectedTagIds)
                {
                    var entryIds = await _tagService.GetEntryIdsByTagAsync(tagId);
                    foreach (var entryId in entryIds)
                    {
                        entryIdsByTags.Add(entryId);
                    }
                }
                results = results.Where(e => entryIdsByTags.Contains(e.Id)).ToList();
            }

            FilteredEntries = results.OrderByDescending(e => e.EntryDate).ToList();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error performing search: {ex.Message}");
            FilteredEntries = new List<JournalEntry>();
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Clears all search and filter criteria.
    /// </summary>
    [RelayCommand]
    public void ClearFilters()
    {
        SearchText = string.Empty;
        StartDate = null;
        EndDate = null;
        SelectedMoodIds = new List<int>();
        SelectedTagIds = new List<int>();
        FilteredEntries = new List<JournalEntry>();
        SelectedEntry = null;
    }

    /// <summary>
    /// Toggles a mood filter selection.
    /// </summary>
    /// <param name="moodId">The ID of the mood to toggle.</param>
    public void ToggleMoodFilter(int moodId)
    {
        if (SelectedMoodIds.Contains(moodId))
        {
            SelectedMoodIds.Remove(moodId);
        }
        else
        {
            SelectedMoodIds.Add(moodId);
        }
    }

    /// <summary>
    /// Toggles a tag filter selection.
    /// </summary>
    /// <param name="tagId">The ID of the tag to toggle.</param>
    public void ToggleTagFilter(int tagId)
    {
        if (SelectedTagIds.Contains(tagId))
        {
            SelectedTagIds.Remove(tagId);
        }
        else
        {
            SelectedTagIds.Add(tagId);
        }
    }

    /// <summary>
    /// Loads the full details of a selected entry including moods and tags.
    /// </summary>
    /// <param name="entryId">The ID of the entry to load.</param>
    public async Task LoadEntryDetailsAsync(int entryId)
    {
        SelectedEntry = await _journalService.GetEntryByIdAsync(entryId);
    }
}

