using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Doodle.Models;
using Doodle.Services;

namespace Doodle.ViewModels;

/// <summary>
/// ViewModel for viewing a journal entry in read-only mode.
/// </summary>
public partial class ViewJournalEntryViewModel : ObservableObject
{
    private readonly JournalService _journalService;
    private readonly MoodService _moodService;
    private readonly TagService _tagService;

    [ObservableProperty]
    private JournalEntry? _entry;

    [ObservableProperty]
    private Mood? _primaryMood;

    [ObservableProperty]
    private List<Mood> _secondaryMoods = new();

    [ObservableProperty]
    private List<Tag> _tags = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    /// <summary>
    /// Gets a value indicating whether the entry is from today (and can be edited).
    /// </summary>
    public bool IsTodayEntry => Entry != null && Entry.EntryDate.Date == DateTime.Today;

    public ViewJournalEntryViewModel(JournalService journalService, MoodService moodService, TagService tagService)
    {
        _journalService = journalService;
        _moodService = moodService;
        _tagService = tagService;
    }

    /// <summary>
    /// Loads a journal entry by ID.
    /// </summary>
    [RelayCommand]
    public async Task LoadEntryAsync(int entryId)
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            Entry = await _journalService.GetEntryByIdAsync(entryId);
            
            if (Entry == null)
            {
                ErrorMessage = "Entry not found.";
                return;
            }

            // Load moods
            var (primary, secondary) = await _moodService.GetEntryMoodsAsync(Entry.Id);
            PrimaryMood = primary;
            SecondaryMoods = secondary;

            // Load tags
            Tags = await _tagService.GetEntryTagsAsync(Entry.Id);
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
    /// Loads a journal entry by date.
    /// </summary>
    [RelayCommand]
    public async Task LoadEntryByDateAsync(DateTime date)
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            Entry = await _journalService.GetEntryByDateAsync(date);
            
            if (Entry == null)
            {
                ErrorMessage = "No entry found for this date.";
                return;
            }

            // Load moods
            var (primary, secondary) = await _moodService.GetEntryMoodsAsync(Entry.Id);
            PrimaryMood = primary;
            SecondaryMoods = secondary;

            // Load tags
            Tags = await _tagService.GetEntryTagsAsync(Entry.Id);
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
}
