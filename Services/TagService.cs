using Doodle.Models;
using Doodle.Data;

namespace Doodle.Services;

/// <summary>
/// Service responsible for managing tags and their associations with journal entries.
/// Supports both predefined and user-created custom tags.
/// </summary>
public class TagService
{
    private readonly DatabaseService _databaseService;

    /// <summary>
    /// Initializes a new instance of the TagService.
    /// </summary>
    /// <param name="databaseService">The database service for data access.</param>
    public TagService(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    /// <summary>
    /// Gets all available tags.
    /// </summary>
    /// <returns>A list of all tags, ordered by name.</returns>
    public async Task<List<Tag>> GetAllTagsAsync()
    {
        return await _databaseService.Database.Table<Tag>()
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all predefined tags.
    /// </summary>
    /// <returns>A list of predefined tags.</returns>
    public async Task<List<Tag>> GetPredefinedTagsAsync()
    {
        return await _databaseService.Database.Table<Tag>()
            .Where(t => t.IsPredefined)
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Gets a tag by its ID.
    /// </summary>
    /// <param name="id">The ID of the tag.</param>
    /// <returns>The tag, or null if not found.</returns>
    public async Task<Tag?> GetTagByIdAsync(int id)
    {
        return await _databaseService.Database.Table<Tag>()
            .Where(t => t.Id == id)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Gets a tag by its name.
    /// </summary>
    /// <param name="name">The name of the tag.</param>
    /// <returns>The tag, or null if not found.</returns>
    public async Task<Tag?> GetTagByNameAsync(string name)
    {
        return await _databaseService.Database.Table<Tag>()
            .Where(t => t.Name == name)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Creates a new custom tag. If a tag with the same name already exists, returns the existing tag.
    /// </summary>
    /// <param name="tag">The tag to create.</param>
    /// <returns>The created or existing tag.</returns>
    public async Task<Tag> CreateTagAsync(Tag tag)
    {
        // Check if tag with same name already exists
        var existing = await GetTagByNameAsync(tag.Name);
        if (existing != null)
        {
            return existing;
        }

        tag.IsPredefined = false;
        tag.CreatedAt = DateTime.UtcNow;
        await _databaseService.Database.InsertAsync(tag);
        return tag;
    }

    /// <summary>
    /// Gets all tags associated with a journal entry.
    /// </summary>
    /// <param name="entryId">The ID of the journal entry.</param>
    /// <returns>A list of tags associated with the entry.</returns>
    public async Task<List<Tag>> GetEntryTagsAsync(int entryId)
    {
        var entryTags = await _databaseService.Database.Table<EntryTag>()
            .Where(et => et.JournalEntryId == entryId)
            .ToListAsync();

        var tags = new List<Tag>();
        foreach (var entryTag in entryTags)
        {
            var tag = await GetTagByIdAsync(entryTag.TagId);
            if (tag != null)
            {
                tags.Add(tag);
            }
        }

        return tags;
    }

    /// <summary>
    /// Sets the tags for a journal entry. Replaces any existing tag associations.
    /// </summary>
    /// <param name="entryId">The ID of the journal entry.</param>
    /// <param name="tagIds">The IDs of tags to associate with the entry.</param>
    public async Task SetEntryTagsAsync(int entryId, List<int> tagIds)
    {
        // Remove existing tag associations
        await _databaseService.Database.Table<EntryTag>()
            .Where(et => et.JournalEntryId == entryId)
            .DeleteAsync();

        // Add new tag associations
        foreach (var tagId in tagIds)
        {
            var entryTag = new EntryTag
            {
                JournalEntryId = entryId,
                TagId = tagId
            };
            await _databaseService.Database.InsertAsync(entryTag);
        }
    }

    /// <summary>
    /// Adds a tag to a journal entry without removing existing tags.
    /// </summary>
    /// <param name="entryId">The ID of the journal entry.</param>
    /// <param name="tagId">The ID of the tag to add.</param>
    public async Task AddTagToEntryAsync(int entryId, int tagId)
    {
        // Check if association already exists
        var existing = await _databaseService.Database.Table<EntryTag>()
            .Where(et => et.JournalEntryId == entryId && et.TagId == tagId)
            .FirstOrDefaultAsync();

        if (existing == null)
        {
            var entryTag = new EntryTag
            {
                JournalEntryId = entryId,
                TagId = tagId
            };
            await _databaseService.Database.InsertAsync(entryTag);
        }
    }

    /// <summary>
    /// Removes a tag from a journal entry.
    /// </summary>
    /// <param name="entryId">The ID of the journal entry.</param>
    /// <param name="tagId">The ID of the tag to remove.</param>
    public async Task RemoveTagFromEntryAsync(int entryId, int tagId)
    {
        await _databaseService.Database.Table<EntryTag>()
            .Where(et => et.JournalEntryId == entryId && et.TagId == tagId)
            .DeleteAsync();
    }

    /// <summary>
    /// Gets all journal entries that have a specific tag.
    /// </summary>
    /// <param name="tagId">The ID of the tag.</param>
    /// <returns>A list of journal entry IDs that have this tag.</returns>
    public async Task<List<int>> GetEntryIdsByTagAsync(int tagId)
    {
        var entryTags = await _databaseService.Database.Table<EntryTag>()
            .Where(et => et.TagId == tagId)
            .ToListAsync();

        return entryTags.Select(et => et.JournalEntryId).Distinct().ToList();
    }

    /// <summary>
    /// Deletes a custom tag and removes all associations with journal entries.
    /// Predefined tags cannot be deleted.
    /// </summary>
    /// <param name="tagId">The ID of the tag to delete.</param>
    /// <exception cref="InvalidOperationException">Thrown if attempting to delete a predefined tag.</exception>
    public async Task DeleteTagAsync(int tagId)
    {
        var tag = await GetTagByIdAsync(tagId);
        if (tag == null)
            return;

        if (tag.IsPredefined)
        {
            throw new InvalidOperationException("Cannot delete predefined tags.");
        }

        // Remove all associations
        await _databaseService.Database.Table<EntryTag>()
            .Where(et => et.TagId == tagId)
            .DeleteAsync();

        // Delete the tag
        await _databaseService.Database.DeleteAsync<Tag>(tagId);
    }
}

