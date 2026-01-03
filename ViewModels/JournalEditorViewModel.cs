using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Doodle.Models;
using Doodle.Services;

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

    [ObservableProperty]
    private JournalEntry? _currentEntry;

    [ObservableProperty]
    private DateTime _selectedDate = DateTime.Today;

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
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _isEditMode;

    /// <summary>
    /// Initializes a new instance of the JournalEditorViewModel.
    /// </summary>
    public JournalEditorViewModel(JournalService journalService, MoodService moodService, TagService tagService)
    {
        _journalService = journalService;
        _moodService = moodService;
        _tagService = tagService;
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

            // Load available moods and tags
            await LoadAvailableMoodsAsync();
            await LoadAvailableTagsAsync();

            // Try to get existing entry for the selected date
            var entry = await _journalService.GetEntryByDateAsync(SelectedDate);

            if (entry != null)
            {
                // Edit mode: load existing entry
                CurrentEntry = entry;
                Title = entry.Title ?? string.Empty;
                Content = entry.Content;
                WordCount = entry.WordCount;
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

            if (IsEditMode && CurrentEntry != null)
            {
                // Update existing entry
                CurrentEntry.Title = Title;
                CurrentEntry.Content = Content;
                CurrentEntry.CalculateWordCount();
                WordCount = CurrentEntry.WordCount;

                entry = await _journalService.UpdateEntryAsync(CurrentEntry);
            }
            else
            {
                // Create new entry
                entry = new JournalEntry
                {
                    Title = Title,
                    Content = Content,
                    EntryDate = SelectedDate
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

            await _journalService.DeleteEntryAsync(CurrentEntry.Id);

            // Reset to create mode
            CurrentEntry = null;
            Title = string.Empty;
            Content = string.Empty;
            WordCount = 0;
            SelectedPrimaryMood = null;
            SelectedPrimaryMoodId = null;
            SelectedSecondaryMoods = new List<Mood>();
            SelectedTags = new List<Tag>();
            IsEditMode = false;
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
}

