using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Doodle.Models;
using Doodle.Services;
using System;
using System.IO;
using System.Text.RegularExpressions;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using PdfColors = QuestPDF.Helpers.Colors;
using Markdig;

namespace Doodle.ViewModels;

/// <summary>
/// ViewModel for exporting journal entries to PDF.
/// </summary>
public partial class ExportJournalViewModel : ObservableObject
{
    private readonly JournalService _journalService;
    private readonly MoodService _moodService;
    private readonly TagService _tagService;

    [ObservableProperty]
    private DateTime _startDate = new DateTime(2026, 1, 1);

    [ObservableProperty]
    private DateTime _endDate = DateTime.Today;

    [ObservableProperty]
    private bool _isExporting;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string? _successMessage;

    [ObservableProperty]
    private int _entriesCount;

    public ExportJournalViewModel(JournalService journalService, MoodService moodService, TagService tagService)
    {
        _journalService = journalService;
        _moodService = moodService;
        _tagService = tagService;
    }

    /// <summary>
    /// Updates the count of entries in the selected date range.
    /// </summary>
    [RelayCommand]
    public async Task UpdateEntriesCountAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"UpdateEntriesCountAsync: StartDate={StartDate:yyyy-MM-dd}, EndDate={EndDate:yyyy-MM-dd}");
            
            // Ensure dates are normalized
            var start = StartDate.Date;
            var end = EndDate.Date;
            
            System.Diagnostics.Debug.WriteLine($"UpdateEntriesCountAsync: Normalized StartDate={start:yyyy-MM-dd}, EndDate={end:yyyy-MM-dd}");
            
            var entries = await _journalService.GetEntriesByDateRangeAsync(start, end);
            EntriesCount = entries.Count;
            
            System.Diagnostics.Debug.WriteLine($"UpdateEntriesCountAsync: Found {EntriesCount} entries in range");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error counting entries: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            EntriesCount = 0;
        }
    }

    /// <summary>
    /// Exports journal entries to PDF.
    /// </summary>
    [RelayCommand]
    public async Task ExportToPdfAsync()
    {
        System.Diagnostics.Debug.WriteLine("=== ExportToPdfAsync STARTED ===");
        try
        {
            IsExporting = true;
            ErrorMessage = null;
            SuccessMessage = null;
            
            System.Diagnostics.Debug.WriteLine($"IsExporting set to: {IsExporting}");
            
            System.Diagnostics.Debug.WriteLine($"Export date range: {StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd}");

            // Validate date range
            if (StartDate > EndDate)
            {
                ErrorMessage = "Start date must be before or equal to end date.";
                IsExporting = false;
                return;
            }

            // Normalize dates to ensure only date comparison (no time component)
            var start = StartDate.Date;
            var end = EndDate.Date;
            
            System.Diagnostics.Debug.WriteLine($"Export: Normalized date range: {start:yyyy-MM-dd} to {end:yyyy-MM-dd}");

            // Get entries in date range
            var entries = await _journalService.GetEntriesByDateRangeAsync(start, end);
            
            System.Diagnostics.Debug.WriteLine($"Export: Retrieved {entries.Count} entries for export");
            
            if (entries.Count == 0)
            {
                ErrorMessage = "No entries found in the selected date range.";
                IsExporting = false;
                return;
            }

            System.Diagnostics.Debug.WriteLine($"Found {entries.Count} entries to export");

            // Generate PDF export
            var fileName = $"Doodle_Export_{StartDate:yyyy-MM-dd}_to_{EndDate:yyyy-MM-dd}.pdf";
            
            try
            {
                // Save to Downloads folder
                var downloadsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                
                // Ensure Downloads folder exists
                if (!Directory.Exists(downloadsFolder))
                {
                    Directory.CreateDirectory(downloadsFolder);
                    System.Diagnostics.Debug.WriteLine($"Created Downloads directory: {downloadsFolder}");
                }

                var filePath = Path.Combine(downloadsFolder, fileName);
                
                System.Diagnostics.Debug.WriteLine($"Attempting to save file to: {filePath}");
                System.Diagnostics.Debug.WriteLine($"Directory exists: {Directory.Exists(downloadsFolder)}");
                System.Diagnostics.Debug.WriteLine($"Downloads folder: {downloadsFolder}");
                
                // Generate and save PDF
                await GeneratePdfExportAsync(entries, filePath);
                
                // Verify file was created
                if (File.Exists(filePath))
                {
                    var fileInfo = new FileInfo(filePath);
                    System.Diagnostics.Debug.WriteLine($"Export file saved successfully!");
                    System.Diagnostics.Debug.WriteLine($"File path: {filePath}");
                    System.Diagnostics.Debug.WriteLine($"File size: {fileInfo.Length} bytes");
                }
                else
                {
                    throw new IOException($"File was not created at path: {filePath}");
                }

                // Store export record in database
                await StoreExportRecordAsync(fileName, filePath, entries.Count, StartDate, EndDate);

                SuccessMessage = $"✅ Export saved successfully!\n\n{entries.Count} entries exported.\n\nFile: {fileName}\nLocation: {filePath}\n\nYou can find the file in your Downloads folder:\n{downloadsFolder}";
                
                System.Diagnostics.Debug.WriteLine("=== EXPORT SUCCESS ===");
                System.Diagnostics.Debug.WriteLine($"SuccessMessage set: {SuccessMessage}");
            }
            catch (Exception fileEx)
            {
                var errorMsg = $"Error saving export file: {fileEx.Message}";
                if (fileEx.InnerException != null)
                {
                    errorMsg += $"\nInner exception: {fileEx.InnerException.Message}";
                }
                ErrorMessage = errorMsg;
                System.Diagnostics.Debug.WriteLine($"=== EXPORT FILE ERROR ===");
                System.Diagnostics.Debug.WriteLine($"Error: {fileEx.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {fileEx.StackTrace}");
                if (fileEx.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {fileEx.InnerException.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            var errorMsg = $"Error exporting entries: {ex.Message}";
            if (ex.InnerException != null)
            {
                errorMsg += $"\nInner exception: {ex.InnerException.Message}";
            }
            ErrorMessage = errorMsg;
            System.Diagnostics.Debug.WriteLine($"=== EXPORT ERROR ===");
            System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
        }
        finally
        {
            IsExporting = false;
            System.Diagnostics.Debug.WriteLine("=== EXPORT COMPLETED ===");
            System.Diagnostics.Debug.WriteLine($"IsExporting set to: {IsExporting}");
            System.Diagnostics.Debug.WriteLine($"Final SuccessMessage: {SuccessMessage}");
            System.Diagnostics.Debug.WriteLine($"Final ErrorMessage: {ErrorMessage}");
        }
    }

    private async Task GeneratePdfExportAsync(List<JournalEntry> entries, string filePath)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        
        // Load all entry data (moods, tags) before building PDF
        var entryData = new List<(JournalEntry Entry, Mood? PrimaryMood, List<Mood> SecondaryMoods, List<Tag> Tags)>();
        
        foreach (var entry in entries)
        {
            var (primaryMood, secondaryMoods) = await _moodService.GetEntryMoodsAsync(entry.Id);
            var tags = await _tagService.GetEntryTagsAsync(entry.Id);
            entryData.Add((entry, primaryMood, secondaryMoods, tags));
        }
        
        // Now build PDF synchronously with all data loaded
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(PdfColors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                page.Header()
                    .Text("Doodle Journal Export")
                    .FontSize(20)
                    .Bold()
                    .AlignCenter();

                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Column(column =>
                    {
                        column.Spacing(0.5f, Unit.Centimetre);
                        
                        // Export info
                        column.Item().Text($"Exported on: {DateTime.Now:g}");
                        column.Item().Text($"Date Range: {StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd}");
                        column.Item().PaddingTop(0.5f, Unit.Centimetre).LineHorizontal(1).LineColor(PdfColors.Grey.Medium);
                        
                        // Entries
                        foreach (var (entry, primaryMood, secondaryMoods, tags) in entryData)
                        {
                            column.Item().PaddingTop(1, Unit.Centimetre).Column(entryColumn =>
                            {
                                // Entry date
                                entryColumn.Item().Text(entry.EntryDate.ToString("MMMM dd, yyyy"))
                                    .FontSize(14)
                                    .Bold()
                                    .FontColor(PdfColors.Blue.Medium);
                                
                                // Entry title
                                if (!string.IsNullOrEmpty(entry.Title))
                                {
                                    entryColumn.Item().PaddingTop(0.2f, Unit.Centimetre)
                                        .Text(entry.Title)
                                        .FontSize(12)
                                        .Bold();
                                }
                                
                                // Moods
                                if (primaryMood != null || secondaryMoods.Any())
                                {
                                    var moodText = primaryMood != null ? $"Primary Mood: {primaryMood.Name}" : "";
                                    if (secondaryMoods.Any())
                                    {
                                        var secondaryText = string.Join(", ", secondaryMoods.Select(m => m.Name));
                                        moodText += (string.IsNullOrEmpty(moodText) ? "" : " | ") + $"Secondary: {secondaryText}";
                                    }
                                    entryColumn.Item().PaddingTop(0.2f, Unit.Centimetre)
                                        .Text(moodText)
                                        .FontSize(9)
                                        .FontColor(PdfColors.Grey.Darken1);
                                }
                                
                                // Tags
                                if (tags.Any())
                                {
                                    var tagText = "Tags: " + string.Join(", ", tags.Select(t => t.Name));
                                    entryColumn.Item().PaddingTop(0.1f, Unit.Centimetre)
                                        .Text(tagText)
                                        .FontSize(9)
                                        .FontColor(PdfColors.Grey.Darken1);
                                }
                                
                                // Entry content with markdown formatting preserved
                                entryColumn.Item().PaddingTop(0.3f, Unit.Centimetre)
                                    .Text(text =>
                                    {
                                        RenderMarkdownToPdf(text, entry.Content ?? "", 10);
                                    });
                                
                                // Metadata
                                entryColumn.Item().PaddingTop(0.3f, Unit.Centimetre)
                                    .Text($"Word Count: {entry.WordCount} | Created: {entry.CreatedAt:g} | Updated: {entry.UpdatedAt:g}")
                                    .FontSize(8)
                                    .FontColor(PdfColors.Grey.Medium);
                                
                                // Separator
                                entryColumn.Item().PaddingTop(0.5f, Unit.Centimetre)
                                    .LineHorizontal(0.5f)
                                    .LineColor(PdfColors.Grey.Lighten2);
                            });
                        }
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Page ").FontSize(8).FontColor(PdfColors.Grey.Medium);
                        x.CurrentPageNumber().FontSize(8).FontColor(PdfColors.Grey.Medium);
                        x.Span(" of ").FontSize(8).FontColor(PdfColors.Grey.Medium);
                        x.TotalPages().FontSize(8).FontColor(PdfColors.Grey.Medium);
                    });
            });
        });

        document.GeneratePdf(filePath);
    }

    /// <summary>
    /// Renders markdown content to QuestPDF text spans with formatting preserved (bold, italic, links, etc.).
    /// </summary>
    private void RenderMarkdownToPdf(TextDescriptor text, string markdownContent, float fontSize)
    {
        if (string.IsNullOrWhiteSpace(markdownContent))
        {
            return;
        }

        // Convert markdown to HTML first using Markdig
        var html = Markdown.ToHtml(markdownContent);
        
        // Parse HTML and convert to QuestPDF text spans
        // Handle common markdown elements: bold, italic, links, headings, lists
        
        // Process the HTML line by line to preserve formatting
        var lines = html.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        bool isFirstLine = true;
        
        foreach (var line in lines)
        {
            if (!isFirstLine && !string.IsNullOrWhiteSpace(line))
            {
                text.Span("\n");
            }
            isFirstLine = false;
            
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }
            
            RenderFormattedLine(text, line, fontSize);
        }
    }

    /// <summary>
    /// Renders a single line of HTML-formatted text to PDF spans.
    /// Handles nested tags by processing HTML recursively.
    /// </summary>
    private void RenderFormattedLine(TextDescriptor text, string htmlLine, float fontSize)
    {
        // Remove HTML entities first
        htmlLine = System.Net.WebUtility.HtmlDecode(htmlLine);
        
        // Process HTML recursively to handle nested formatting
        ProcessHtmlContent(text, htmlLine, fontSize);
    }

    /// <summary>
    /// Recursively processes HTML content to extract and render formatted text.
    /// Handles nested tags like bold text with links inside.
    /// </summary>
    private void ProcessHtmlContent(TextDescriptor text, string html, float fontSize)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return;
        }

        // Handle line breaks first
        if (html.Contains("<br"))
        {
            var parts = Regex.Split(html, @"<br\s*/?>", RegexOptions.IgnoreCase);
            for (int i = 0; i < parts.Length; i++)
            {
                if (i > 0) text.Span("\n");
                ProcessHtmlContent(text, parts[i], fontSize);
            }
            return;
        }

        // Find the first HTML tag
        var tagMatch = Regex.Match(html, @"<(\w+)(?:\s+[^>]*)?>(.*?)</\1>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        
        if (!tagMatch.Success)
        {
            // No more tags, render as plain text
            var plainText = Regex.Replace(html, @"<[^>]+>", "");
            plainText = System.Net.WebUtility.HtmlDecode(plainText);
            if (!string.IsNullOrWhiteSpace(plainText))
            {
                text.Span(plainText).FontSize(fontSize);
            }
            return;
        }

        // Render text before the tag
        var beforeText = html.Substring(0, tagMatch.Index);
        ProcessHtmlContent(text, beforeText, fontSize);

        // Process the tag
        var tagName = tagMatch.Groups[1].Value.ToLower();
        var tagContent = tagMatch.Groups[2].Value;
        
        // Handle different tag types with formatting
        if (tagName == "h1")
        {
            var headingText = ExtractPlainText(tagContent);
            text.Span(headingText).FontSize(fontSize * 1.8f).Bold();
        }
        else if (tagName == "h2")
        {
            var headingText = ExtractPlainText(tagContent);
            text.Span(headingText).FontSize(fontSize * 1.5f).Bold();
        }
        else if (tagName == "h3")
        {
            var headingText = ExtractPlainText(tagContent);
            text.Span(headingText).FontSize(fontSize * 1.2f).Bold();
        }
        else if (tagName == "strong" || tagName == "b")
        {
            // Check if there are nested tags (like links)
            if (tagContent.Contains("<"))
            {
                // Process nested content - links will be formatted as links, other text as bold
                ProcessHtmlContentWithBold(text, tagContent, fontSize);
            }
            else
            {
                var boldText = ExtractPlainText(tagContent);
                text.Span(boldText).FontSize(fontSize).Bold();
            }
        }
        else if (tagName == "em" || tagName == "i")
        {
            if (tagContent.Contains("<"))
            {
                ProcessHtmlContentWithItalic(text, tagContent, fontSize);
            }
            else
            {
                var italicText = ExtractPlainText(tagContent);
                text.Span(italicText).FontSize(fontSize).Italic();
            }
        }
        else if (tagName == "u")
        {
            if (tagContent.Contains("<"))
            {
                ProcessHtmlContent(text, tagContent, fontSize);
            }
            else
            {
                var underlineText = ExtractPlainText(tagContent);
                text.Span(underlineText).FontSize(fontSize).Underline();
            }
        }
        else if (tagName == "code")
        {
            var codeText = ExtractPlainText(tagContent);
            text.Span(codeText).FontSize(fontSize * 0.9f).FontFamily("Courier New");
        }
        else if (tagName == "a")
        {
            // Extract URL from the full tag match
            var hrefMatch = Regex.Match(tagMatch.Value, @"href=[""']([^""']+)[""']", RegexOptions.IgnoreCase);
            var linkText = ExtractPlainText(tagContent);
            // Format as link (underlined, blue color)
            text.Span(linkText).FontSize(fontSize).Underline().FontColor(PdfColors.Blue.Medium);
        }
        else if (tagName == "p")
        {
            ProcessHtmlContent(text, tagContent, fontSize);
            text.Span("\n\n");
        }
        else if (tagName == "li")
        {
            text.Span("• ");
            ProcessHtmlContent(text, tagContent, fontSize);
            text.Span("\n");
        }
        else
        {
            // Unknown tag, just process its content
            ProcessHtmlContent(text, tagContent, fontSize);
        }

        // Process text after the tag
        var afterText = html.Substring(tagMatch.Index + tagMatch.Length);
        ProcessHtmlContent(text, afterText, fontSize);
    }

    /// <summary>
    /// Processes HTML content with bold formatting applied to non-link text.
    /// </summary>
    private void ProcessHtmlContentWithBold(TextDescriptor text, string html, float fontSize)
    {
        // Find links first (they take priority)
        var linkMatch = Regex.Match(html, @"<a\s+href=[""']([^""']+)[""'][^>]*>(.*?)</a>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        
        if (linkMatch.Success)
        {
            // Render text before link as bold
            var beforeLink = html.Substring(0, linkMatch.Index);
            var beforeText = ExtractPlainText(beforeLink);
            if (!string.IsNullOrWhiteSpace(beforeText))
            {
                text.Span(beforeText).FontSize(fontSize).Bold();
            }
            
            // Render link (underlined, blue) - links override bold
            var linkText = ExtractPlainText(linkMatch.Groups[2].Value);
            text.Span(linkText).FontSize(fontSize).Underline().FontColor(PdfColors.Blue.Medium);
            
            // Process text after link
            var afterLink = html.Substring(linkMatch.Index + linkMatch.Length);
            ProcessHtmlContentWithBold(text, afterLink, fontSize);
        }
        else
        {
            // No links, just render as bold
            var plainText = ExtractPlainText(html);
            if (!string.IsNullOrWhiteSpace(plainText))
            {
                text.Span(plainText).FontSize(fontSize).Bold();
            }
        }
    }

    /// <summary>
    /// Processes HTML content with italic formatting applied to non-link text.
    /// </summary>
    private void ProcessHtmlContentWithItalic(TextDescriptor text, string html, float fontSize)
    {
        // Find links first (they take priority)
        var linkMatch = Regex.Match(html, @"<a\s+href=[""']([^""']+)[""'][^>]*>(.*?)</a>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        
        if (linkMatch.Success)
        {
            // Render text before link as italic
            var beforeLink = html.Substring(0, linkMatch.Index);
            var beforeText = ExtractPlainText(beforeLink);
            if (!string.IsNullOrWhiteSpace(beforeText))
            {
                text.Span(beforeText).FontSize(fontSize).Italic();
            }
            
            // Render link (underlined, blue) - links override italic
            var linkText = ExtractPlainText(linkMatch.Groups[2].Value);
            text.Span(linkText).FontSize(fontSize).Underline().FontColor(PdfColors.Blue.Medium);
            
            // Process text after link
            var afterLink = html.Substring(linkMatch.Index + linkMatch.Length);
            ProcessHtmlContentWithItalic(text, afterLink, fontSize);
        }
        else
        {
            // No links, just render as italic
            var plainText = ExtractPlainText(html);
            if (!string.IsNullOrWhiteSpace(plainText))
            {
                text.Span(plainText).FontSize(fontSize).Italic();
            }
        }
    }

    /// <summary>
    /// Extracts plain text from HTML by removing all tags.
    /// </summary>
    private string ExtractPlainText(string html)
    {
        var text = Regex.Replace(html, @"<[^>]+>", "");
        return System.Net.WebUtility.HtmlDecode(text);
    }

    private Task StoreExportRecordAsync(string fileName, string filePath, int entryCount, DateTime startDate, DateTime endDate)
    {
        try
        {
            // Store export metadata in preferences
            var exportKey = $"export_{DateTime.UtcNow:yyyyMMddHHmmss}";
            var exportData = $"{fileName}|{filePath}|{entryCount}|{startDate:yyyy-MM-dd}|{endDate:yyyy-MM-dd}";
            
            // For now, we'll use a simple approach - store the latest export info
            // In a full implementation, you might create an ExportHistory table
            // Export metadata storage can be implemented here if needed
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error storing export record: {ex.Message}");
        }
        return Task.CompletedTask;
    }
}
