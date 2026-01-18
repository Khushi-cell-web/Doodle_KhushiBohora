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
    private readonly CategoryService _categoryService;

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
    private List<Category> _availableCategories = new();

    [ObservableProperty]
    private List<string> _selectedCategories = new();

    [ObservableProperty]
    private List<JournalEntry> _filteredEntries = new();

    [ObservableProperty]
    private List<JournalEntry> _paginatedEntries = new();

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _pageSize = 10;

    [ObservableProperty]
    private int _totalPages;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private JournalEntry? _selectedEntry;

    [ObservableProperty]
    private string _newTagName = string.Empty;

    [ObservableProperty]
    private string _newCategoryName = string.Empty;

    [ObservableProperty]
    private string _newMoodName = string.Empty;

    [ObservableProperty]
    private string _newMoodIcon = string.Empty;

    [ObservableProperty]
    private string _newMoodCategory = "Positive";

    /// <summary>
    /// Initializes a new instance of the SearchViewModel.
    /// </summary>
    public SearchViewModel(JournalService journalService, MoodService moodService, TagService tagService, CategoryService categoryService)
    {
        _journalService = journalService;
        _moodService = moodService;
        _tagService = tagService;
        _categoryService = categoryService;
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
            AvailableCategories = await _categoryService.GetAllCategoriesAsync();
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

            // Filter by categories if selected
            if (SelectedCategories.Count > 0)
            {
                results = results.Where(e => 
                    e.Category != null && SelectedCategories.Contains(e.Category)).ToList();
            }

            FilteredEntries = results.OrderByDescending(e => e.EntryDate).ToList();
            UpdatePagination();
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
        SelectedCategories = new List<string>();
        FilteredEntries = new List<JournalEntry>();
        PaginatedEntries = new List<JournalEntry>();
        CurrentPage = 1;
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

    /// <summary>
    /// Toggles a category filter selection.
    /// </summary>
    /// <param name="categoryName">The name of the category to toggle.</param>
    public void ToggleCategoryFilter(string categoryName)
    {
        if (SelectedCategories.Contains(categoryName))
        {
            SelectedCategories.Remove(categoryName);
        }
        else
        {
            SelectedCategories.Add(categoryName);
        }
    }

    /// <summary>
    /// Updates pagination based on filtered entries.
    /// </summary>
    private void UpdatePagination()
    {
        TotalPages = (int)Math.Ceiling(FilteredEntries.Count / (double)PageSize);
        if (TotalPages == 0) TotalPages = 1;
        if (CurrentPage > TotalPages) CurrentPage = TotalPages;
        if (CurrentPage < 1) CurrentPage = 1;

        PaginatedEntries = FilteredEntries
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .ToList();
    }

    /// <summary>
    /// Navigates to the next page.
    /// </summary>
    [RelayCommand]
    public void NextPage()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
            UpdatePagination();
        }
    }

    /// <summary>
    /// Navigates to the previous page.
    /// </summary>
    [RelayCommand]
    public void PreviousPage()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            UpdatePagination();
        }
    }

    /// <summary>
    /// Navigates to a specific page.
    /// </summary>
    public void GoToPage(int page)
    {
        if (page >= 1 && page <= TotalPages)
        {
            CurrentPage = page;
            UpdatePagination();
        }
    }

    /// <summary>
    /// Creates a new tag and refreshes the available tags list.
    /// </summary>
    [RelayCommand]
    public async Task CreateTagAsync()
    {
        if (string.IsNullOrWhiteSpace(NewTagName))
        {
            return;
        }

        try
        {
            var newTag = new Tag
            {
                Name = NewTagName.Trim(),
                Color = "#787F56", // Default color
                IsPredefined = false,
                CreatedAt = DateTime.UtcNow
            };

            await _tagService.CreateTagAsync(newTag);
            NewTagName = string.Empty;
            
            // Refresh available tags
            AvailableTags = await _tagService.GetAllTagsAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error creating tag: {ex.Message}");
        }
    }

    /// <summary>
    /// Creates a new category and refreshes the available categories list.
    /// </summary>
    [RelayCommand]
    public async Task CreateCategoryAsync()
    {
        if (string.IsNullOrWhiteSpace(NewCategoryName))
        {
            return;
        }

        try
        {
            var newCategory = new Category
            {
                Name = NewCategoryName.Trim(),
                Color = "#787F56", // Default color
                IsPredefined = false,
                CreatedAt = DateTime.UtcNow
            };

            await _categoryService.CreateCategoryAsync(newCategory);
            NewCategoryName = string.Empty;
            
            // Refresh available categories
            AvailableCategories = await _categoryService.GetAllCategoriesAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error creating category: {ex.Message}");
        }
    }

    /// <summary>
    /// Creates a new mood and refreshes the available moods list.
    /// </summary>
    [RelayCommand]
    public async Task CreateMoodAsync()
    {
        if (string.IsNullOrWhiteSpace(NewMoodName))
        {
            return;
        }

        try
        {
            var newMood = new Mood
            {
                Name = NewMoodName.Trim(),
                Category = NewMoodCategory,
                Icon = string.IsNullOrWhiteSpace(NewMoodIcon) ? "😊" : NewMoodIcon.Trim(),
                IsPredefined = false
            };

            await _moodService.CreateCustomMoodAsync(newMood);
            NewMoodName = string.Empty;
            NewMoodIcon = string.Empty;
            NewMoodCategory = "Positive";
            
            // Refresh available moods
            AvailableMoods = await _moodService.GetAllMoodsAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error creating mood: {ex.Message}");
        }
    }
}

