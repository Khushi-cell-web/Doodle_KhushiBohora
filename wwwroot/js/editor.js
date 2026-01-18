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
