using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Doodle.Models;
using Doodle.Services;
using Microsoft.JSInterop;

namespace Doodle.ViewModels;

/// <summary>
/// ViewModel for the Journal Editor page.
/// Manages the creation, editing, and saving of journal entries with moods and tags.
/// </summary>
public partial class JournalEditorViewModel : ObservableObject
{
    private readonly JournalService _journalService;
    private readonly MoodService _moodService;
    private readonly TagService _tagService;
    private readonly CategoryService _categoryService;

    [ObservableProperty]
    private JournalEntry? _currentEntry;

    [ObservableProperty]
    private DateTime _selectedDate = DateTime.Today;

    /// <summary>
    /// Automatically resets the selected date to today if a different date is selected.
    /// </summary>
    partial void OnSelectedDateChanged(DateTime value)
    {
        if (value.Date != DateTime.Today)
        {
            SelectedDate = DateTime.Today;
            ErrorMessage = $"Only today's date ({DateTime.Today:MMMM dd, yyyy}) can be used for journal entries. Date has been reset to today.";
        }
    }

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _content = string.Empty;

    [ObservableProperty]
    private int _wordCount;

    [ObservableProperty]
    private List<Mood> _availableMoods = new();

    [ObservableProperty]
    private Mood? _selectedPrimaryMood;

    [ObservableProperty]
    private int? _selectedPrimaryMoodId;

    [ObservableProperty]
    private List<Mood> _selectedSecondaryMoods = new();

    [ObservableProperty]
    private List<Mood> _availableSecondaryMoods = new();

    [ObservableProperty]
    private List<Tag> _availableTags = new();

    [ObservableProperty]
    private List<Tag> _selectedTags = new();

    [ObservableProperty]
    private List<Category> _availableCategories = new();

    [ObservableProperty]
    private string? _selectedCategory;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string? _successMessage;

    [ObservableProperty]
    private bool _isEditMode;

    // Formatting toolbar commands
    public const string TextareaId = "entry-content";

    // Add new items properties
    [ObservableProperty]
    private string _newTagName = string.Empty;

    [ObservableProperty]
    private string _newCategoryName = string.Empty;

    [ObservableProperty]
    private string _newMoodName = string.Empty;

    [ObservableProperty]
    private string _newMoodIcon = "😊";

    [ObservableProperty]
    private string _newMoodCategory = "Positive";

    /// <summary>
    /// Initializes a new instance of the JournalEditorViewModel.
    /// </summary>
    public JournalEditorViewModel(JournalService journalService, MoodService moodService, TagService tagService, CategoryService categoryService)
    {
        _journalService = journalService;
        _moodService = moodService;
        _tagService = tagService;
        _categoryService = categoryService;
    }

    /// <summary>
    /// Loads the journal entry for the selected date.
    /// </summary>
    [RelayCommand]
    public async Task LoadEntryForDateAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            // Validate that only today's date can be used
            if (SelectedDate.Date != DateTime.Today)
            {
                ErrorMessage = $"Only today's date ({DateTime.Today:MMMM dd, yyyy}) can be used for journal entries. Please select today's date.";
                SelectedDate = DateTime.Today;
                IsLoading = false;
                return;
            }

            // Load available moods, tags, and categories
            await LoadAvailableMoodsAsync();
            await LoadAvailableTagsAsync();
            await LoadAvailableCategoriesAsync();

            // Try to get existing entry for the selected date
            var entry = await _journalService.GetEntryByDateAsync(SelectedDate);

            if (entry != null)
            {
                // Edit mode: load existing entry
                CurrentEntry = entry;
                Title = entry.Title ?? string.Empty;
                Content = entry.Content;
                WordCount = entry.WordCount;
                SelectedCategory = entry.Category;
                IsEditMode = true;

                // Load moods
                var (primary, secondary) = await _moodService.GetEntryMoodsAsync(entry.Id);
                SelectedPrimaryMood = primary;
                SelectedPrimaryMoodId = primary?.Id;
                SelectedSecondaryMoods = secondary ?? new List<Mood>();

                // Load tags
                SelectedTags = await _tagService.GetEntryTagsAsync(entry.Id);
            }
            else
            {
                // Create mode: initialize new entry
                CurrentEntry = null;
                Title = string.Empty;
                Content = string.Empty;
                WordCount = 0;
                SelectedCategory = null;
                IsEditMode = false;
                SelectedPrimaryMood = null;
                SelectedPrimaryMoodId = null;
                SelectedSecondaryMoods = new List<Mood>();
                SelectedTags = new List<Tag>();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading entry: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Saves the current journal entry (create or update).
    /// </summary>
    [RelayCommand]
    public async Task SaveEntryAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;
            SuccessMessage = null;

            // Validate that only today's date can be used
            if (SelectedDate.Date != DateTime.Today)
            {
                ErrorMessage = $"Only today's date ({DateTime.Today:MMMM dd, yyyy}) can be used for journal entries. Cannot save entry for {SelectedDate:MMMM dd, yyyy}.";
                return;
            }

            // Validate primary mood is selected
            if (!SelectedPrimaryMoodId.HasValue)
            {
                ErrorMessage = "Please select a primary mood.";
                return;
            }

            // Get the selected primary mood object
            SelectedPrimaryMood = AvailableMoods.FirstOrDefault(m => m.Id == SelectedPrimaryMoodId.Value);
            if (SelectedPrimaryMood == null)
            {
                ErrorMessage = "Selected primary mood not found.";
                return;
            }

            // Validate content is not empty
            if (string.IsNullOrWhiteSpace(Content))
            {
                ErrorMessage = "Journal entry content cannot be empty.";
                return;
            }

            JournalEntry entry;
            var isUpdate = IsEditMode && CurrentEntry != null;

            if (isUpdate)
            {
                // Update existing entry
                // CurrentEntry is guaranteed non-null because isUpdate == true
                var existing = CurrentEntry!;
                existing.Title = Title;
                existing.Content = Content;
                existing.Category = SelectedCategory;
                existing.CalculateWordCount();
                WordCount = existing.WordCount;

                entry = await _journalService.UpdateEntryAsync(existing);
            }
            else
            {
                // Create new entry
                entry = new JournalEntry
                {
                    Title = Title,
                    Content = Content,
                    EntryDate = SelectedDate,
                    Category = SelectedCategory
                };
                entry.CalculateWordCount();
                WordCount = entry.WordCount;

                entry = await _journalService.CreateEntryAsync(entry);
                CurrentEntry = entry;
                IsEditMode = true;
            }

            // Save moods
            var secondaryMoodIds = SelectedSecondaryMoods.Select(m => m.Id).ToList();
            await _moodService.SetEntryMoodsAsync(entry.Id, SelectedPrimaryMood.Id, secondaryMoodIds);

            // Save tags
            var tagIds = SelectedTags.Select(t => t.Id).ToList();
            await _tagService.SetEntryTagsAsync(entry.Id, tagIds);

            ErrorMessage = null; // Clear any previous errors
            SuccessMessage = isUpdate ? "Journal entry updated successfully." : "Journal entry created successfully.";
        }
        catch (InvalidOperationException ex)
        {
            ErrorMessage = ex.Message;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error saving entry: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Deletes the current journal entry.
    /// </summary>
    [RelayCommand]
    public async Task DeleteEntryAsync()
    {
        if (CurrentEntry == null || !IsEditMode)
            return;

        try
        {
            IsLoading = true;
            ErrorMessage = null;
            SuccessMessage = null;

            await _journalService.DeleteEntryAsync(CurrentEntry.Id);

            // Reset to create mode
            CurrentEntry = null;
            Title = string.Empty;
            Content = string.Empty;
            WordCount = 0;
            SelectedCategory = null;
            SelectedPrimaryMood = null;
            SelectedPrimaryMoodId = null;
            SelectedSecondaryMoods = new List<Mood>();
            SelectedTags = new List<Tag>();
            IsEditMode = false;

            SuccessMessage = "Journal entry deleted successfully.";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error deleting entry: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Updates the word count when content changes.
    /// </summary>
    partial void OnContentChanged(string value)
    {
        if (CurrentEntry != null)
        {
            CurrentEntry.Content = value;
            CurrentEntry.CalculateWordCount();
            WordCount = CurrentEntry.WordCount;
        }
        else
        {
            var tempEntry = new JournalEntry { Content = value };
            tempEntry.CalculateWordCount();
            WordCount = tempEntry.WordCount;
        }
    }

    /// <summary>
    /// Loads all available moods from the service.
    /// </summary>
    private async Task LoadAvailableMoodsAsync()
    {
        AvailableMoods = await _moodService.GetAllMoodsAsync();
        AvailableSecondaryMoods = AvailableMoods.ToList(); // Copy for secondary selection
    }

    /// <summary>
    /// Loads all available tags from the service.
    /// </summary>
    private async Task LoadAvailableTagsAsync()
    {
        AvailableTags = await _tagService.GetAllTagsAsync();
    }

    /// <summary>
    /// Loads all available categories from the service.
    /// </summary>
    private async Task LoadAvailableCategoriesAsync()
    {
        AvailableCategories = await _categoryService.GetAllCategoriesAsync();
    }

    /// <summary>
    /// Adds a secondary mood (if less than 2 are already selected).
    /// </summary>
    public void AddSecondaryMood(Mood mood)
    {
        if (SelectedSecondaryMoods.Count < 2 && !SelectedSecondaryMoods.Any(m => m.Id == mood.Id))
        {
            SelectedSecondaryMoods.Add(mood);
        }
    }

    /// <summary>
    /// Updates the selected primary mood when the ID changes.
    /// </summary>
    partial void OnSelectedPrimaryMoodIdChanged(int? value)
    {
        if (value.HasValue)
        {
            SelectedPrimaryMood = AvailableMoods.FirstOrDefault(m => m.Id == value.Value);
        }
        else
        {
            SelectedPrimaryMood = null;
        }
    }

    /// <summary>
    /// Removes a secondary mood.
    /// </summary>
    public void RemoveSecondaryMood(Mood mood)
    {
        SelectedSecondaryMoods.Remove(mood);
    }

    /// <summary>
    /// Toggles a tag selection.
    /// </summary>
    public void ToggleTag(Tag tag)
    {
        if (SelectedTags.Contains(tag))
        {
            SelectedTags.Remove(tag);
        }
        else
        {
            SelectedTags.Add(tag);
        }
    }

    /// <summary>
    /// Formats text as bold (**text**).
    /// </summary>
    [RelayCommand]
    public async Task FormatBoldAsync(IJSRuntime jsRuntime)
    {
        var newContent = await jsRuntime.InvokeAsync<string>("editorHelpers.wrapTextWithMarkdown", TextareaId, "**", "**", "text");
        if (!string.IsNullOrEmpty(newContent))
        {
            Content = newContent;
        }
    }

    /// <summary>
    /// Formats text as italic (*text*).
    /// </summary>
    [RelayCommand]
    public async Task FormatItalicAsync(IJSRuntime jsRuntime)
    {
        var newContent = await jsRuntime.InvokeAsync<string>("editorHelpers.wrapTextWithMarkdown", TextareaId, "*", "*", "text");
        if (!string.IsNullOrEmpty(newContent))
        {
            Content = newContent;
        }
    }

    /// <summary>
    /// Formats text as underline (<u>text</u>).
    /// </summary>
    [RelayCommand]
    public async Task FormatUnderlineAsync(IJSRuntime jsRuntime)
    {
        try
        {
            var newContent = await jsRuntime.InvokeAsync<string>("editorHelpers.wrapTextWithMarkdown", TextareaId, "<u>", "</u>", "text");
            if (!string.IsNullOrEmpty(newContent))
            {
                Content = newContent;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error formatting underline: {ex.Message}");
        }
    }

    /// <summary>
    /// Formats text as code (`text`).
    /// </summary>
    [RelayCommand]
    public async Task FormatCodeAsync(IJSRuntime jsRuntime)
    {
        try
        {
            var newContent = await jsRuntime.InvokeAsync<string>("editorHelpers.wrapTextWithMarkdown", TextareaId, "`", "`", "code");
            if (!string.IsNullOrEmpty(newContent))
            {
                Content = newContent;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error formatting code: {ex.Message}");
        }
    }

    /// <summary>
    /// Inserts a bullet list item (- item).
    /// </summary>
    [RelayCommand]
    public async Task FormatBulletListAsync(IJSRuntime jsRuntime)
    {
        try
        {
            var newContent = await jsRuntime.InvokeAsync<string>("editorHelpers.insertListItem", TextareaId, "- ");
            if (!string.IsNullOrEmpty(newContent))
            {
                Content = newContent;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error inserting bullet list: {ex.Message}");
        }
    }

    /// <summary>
    /// Inserts a numbered list item (1. item).
    /// </summary>
    [RelayCommand]
    public async Task FormatNumberedListAsync(IJSRuntime jsRuntime)
    {
        try
        {
            var newContent = await jsRuntime.InvokeAsync<string>("editorHelpers.insertListItem", TextareaId, "1. ");
            if (!string.IsNullOrEmpty(newContent))
            {
                Content = newContent;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error inserting numbered list: {ex.Message}");
        }
    }

    /// <summary>
    /// Formats text as a link ([text](url)).
    /// </summary>
    [RelayCommand]
    public async Task FormatLinkAsync(IJSRuntime jsRuntime)
    {
        try
        {
            var newContent = await jsRuntime.InvokeAsync<string>("editorHelpers.wrapTextWithMarkdown", TextareaId, "[", "](url)", "link text");
            if (!string.IsNullOrEmpty(newContent))
            {
                Content = newContent;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error formatting link: {ex.Message}");
        }
    }

    /// <summary>
    /// Formats text as Heading 1 (# text).
    /// </summary>
    [RelayCommand]
    public async Task FormatHeading1Async(IJSRuntime jsRuntime)
    {
        try
        {
            var newContent = await jsRuntime.InvokeAsync<string>("editorHelpers.wrapTextWithMarkdown", TextareaId, "# ", "", "Heading 1");
            if (!string.IsNullOrEmpty(newContent))
            {
                Content = newContent;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error formatting heading 1: {ex.Message}");
        }
    }

    /// <summary>
    /// Formats text as Heading 2 (## text).
    /// </summary>
    [RelayCommand]
    public async Task FormatHeading2Async(IJSRuntime jsRuntime)
    {
        try
        {
            var newContent = await jsRuntime.InvokeAsync<string>("editorHelpers.wrapTextWithMarkdown", TextareaId, "## ", "", "Heading 2");
            if (!string.IsNullOrEmpty(newContent))
            {
                Content = newContent;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error formatting heading 2: {ex.Message}");
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
            ErrorMessage = $"Error creating tag: {ex.Message}";
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
            ErrorMessage = $"Error creating category: {ex.Message}";
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
            NewMoodIcon = "😊";
            NewMoodCategory = "Positive";
            
            // Refresh available moods
            await LoadAvailableMoodsAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error creating mood: {ex.Message}");
            ErrorMessage = $"Error creating mood: {ex.Message}";
        }
    }
}

