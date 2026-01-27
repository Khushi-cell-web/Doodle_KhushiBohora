// Editor helper functions for inserting text at cursor position

window.editorHelpers = {
    /**
     * Inserts text at the current cursor position in a textarea.
     * @param {string} textareaId - The ID of the textarea element.
     * @param {string} textToInsert - The text to insert.
     */
    insertTextAtCursor: function (textareaId, textToInsert) {
        try {
            const textarea = document.getElementById(textareaId);
            if (!textarea) {
                console.error(`Textarea with id '${textareaId}' not found`);
                return null;
            }

            const start = textarea.selectionStart;
            const end = textarea.selectionEnd;
            const text = textarea.value;

            // Insert the text
            const newText = text.substring(0, start) + textToInsert + text.substring(end);
            textarea.value = newText;

            // Set cursor position after the inserted text
            const newCursorPos = start + textToInsert.length;
            textarea.setSelectionRange(newCursorPos, newCursorPos);

            // Focus the textarea
            textarea.focus();

            // Trigger input event to update Blazor binding
            const event = new Event('input', { bubbles: true });
            textarea.dispatchEvent(event);

            // Also trigger change event for better compatibility
            const changeEvent = new Event('change', { bubbles: true });
            textarea.dispatchEvent(changeEvent);

            return newText;
        } catch (error) {
            console.error('Error inserting text at cursor:', error);
            return null;
        }
    },

    /**
     * Inserts a list item (bullet or numbered) at the current line or wraps selected text.
     * @param {string} textareaId - The ID of the textarea element.
     * @param {string} listMarker - The list marker to insert (e.g., "- " or "1. ").
     */
    insertListItem: function (textareaId, listMarker) {
        try {
            const textarea = document.getElementById(textareaId);
            if (!textarea) {
                console.error(`Textarea with id '${textareaId}' not found`);
                return null;
            }

            const start = textarea.selectionStart;
            const end = textarea.selectionEnd;
            const text = textarea.value;
            const selectedText = text.substring(start, end);

            // Find the start of the current line
            let lineStart = start;
            while (lineStart > 0 && text[lineStart - 1] !== '\n') {
                lineStart--;
            }

            // Find the end of the current line
            let lineEnd = end;
            while (lineEnd < text.length && text[lineEnd] !== '\n') {
                lineEnd++;
            }

            const currentLine = text.substring(lineStart, lineEnd);
            const lineBeforeCursor = text.substring(lineStart, start);
            const lineAfterSelection = text.substring(end, lineEnd);

            // Check if the line already starts with a list marker
            const isBulletList = /^[\s]*[-*+]\s/.test(currentLine);
            const isNumberedList = /^[\s]*\d+\.\s/.test(currentLine);

            let newText;
            let newCursorPos;

            if (selectedText && selectedText.trim().length > 0) {
                // If text is selected, wrap each line in the selection with list markers
                const lines = selectedText.split('\n');
                const wrappedLines = lines.map(line => {
                    if (line.trim().length === 0) return line;
                    // Remove existing list markers if any
                    const cleanedLine = line.replace(/^[\s]*[-*+]\s/, '').replace(/^[\s]*\d+\.\s/, '');
                    return listMarker + cleanedLine.trim();
                });
                const wrappedText = wrappedLines.join('\n');
                
                newText = text.substring(0, start) + wrappedText + text.substring(end);
                newCursorPos = start + wrappedText.length;
            } else {
                // No selection - insert list marker at start of current line
                if (isBulletList || isNumberedList) {
                    // Line already has a list marker, just move cursor after it
                    const existingMarkerMatch = currentLine.match(/^[\s]*([-*+]|\d+\.)\s/);
                    if (existingMarkerMatch) {
                        const markerEnd = lineStart + existingMarkerMatch[0].length;
                        newText = text;
                        newCursorPos = Math.max(markerEnd, start);
                    } else {
                        // Insert new marker
                        newText = text.substring(0, lineStart) + listMarker + text.substring(lineStart);
                        newCursorPos = lineStart + listMarker.length;
                    }
                } else {
                    // Insert list marker at start of line
                    newText = text.substring(0, lineStart) + listMarker + text.substring(lineStart);
                    newCursorPos = lineStart + listMarker.length;
                }
            }

            textarea.value = newText;
            textarea.setSelectionRange(newCursorPos, newCursorPos);
            textarea.focus();

            // Trigger input event to update Blazor binding
            const event = new Event('input', { bubbles: true });
            textarea.dispatchEvent(event);

            const changeEvent = new Event('change', { bubbles: true });
            textarea.dispatchEvent(changeEvent);

            return newText;
        } catch (error) {
            console.error('Error inserting list item:', error);
            return null;
        }
    },

    /**
     * Wraps selected text (or placeholder) with markdown syntax.
     * @param {string} textareaId - The ID of the textarea element.
     * @param {string} prefix - The prefix to add (e.g., "**" for bold).
     * @param {string} suffix - The suffix to add (e.g., "**" for bold).
     * @param {string} placeholder - Placeholder text if nothing is selected.
     */
    wrapTextWithMarkdown: function (textareaId, prefix, suffix, placeholder) {
        try {
            const textarea = document.getElementById(textareaId);
            if (!textarea) {
                console.error(`Textarea with id '${textareaId}' not found`);
                return null;
            }

            const start = textarea.selectionStart;
            const end = textarea.selectionEnd;
            const text = textarea.value;
            const selectedText = text.substring(start, end);

            let textToWrap = selectedText || placeholder;
            const wrappedText = prefix + textToWrap + suffix;

            // Replace selected text with wrapped text
            const newText = text.substring(0, start) + wrappedText + text.substring(end);
            textarea.value = newText;

            // Set cursor position
            if (selectedText) {
                // If text was selected, place cursor after the wrapped text
                const newCursorPos = start + wrappedText.length;
                textarea.setSelectionRange(newCursorPos, newCursorPos);
            } else {
                // If no text selected, place cursor between prefix and suffix
                const newCursorPos = start + prefix.length;
                textarea.setSelectionRange(newCursorPos, newCursorPos);
            }

            // Focus the textarea
            textarea.focus();

            // Trigger input event to update Blazor binding
            const event = new Event('input', { bubbles: true });
            textarea.dispatchEvent(event);

            // Also trigger change event for better compatibility
            const changeEvent = new Event('change', { bubbles: true });
            textarea.dispatchEvent(changeEvent);

            return newText;
        } catch (error) {
            console.error('Error wrapping text with markdown:', error);
            return null;
        }
    }
};
