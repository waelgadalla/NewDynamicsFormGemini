// Editor keyboard shortcuts and interop functions

let dotNetRef = null;

/**
 * Register keyboard shortcuts for the editor
 * @param {DotNetObjectReference} objRef - Reference to the Blazor component
 */
window.registerKeyboardShortcuts = (objRef) => {
    dotNetRef = objRef;
    document.addEventListener('keydown', handleGlobalKeyDown);
};

/**
 * Unregister keyboard shortcuts (cleanup)
 */
window.unregisterKeyboardShortcuts = () => {
    document.removeEventListener('keydown', handleGlobalKeyDown);
    dotNetRef = null;
};

/**
 * Handle global keyboard events for view switching
 * @param {KeyboardEvent} e - The keyboard event
 */
function handleGlobalKeyDown(e) {
    // Only handle Ctrl+1/2/3 without other modifiers
    if (e.ctrlKey && !e.shiftKey && !e.altKey && !e.metaKey) {
        switch (e.key) {
            case '1':
                e.preventDefault();
                dotNetRef?.invokeMethodAsync('SwitchToView', 0); // Design
                break;
            case '2':
                e.preventDefault();
                dotNetRef?.invokeMethodAsync('SwitchToView', 1); // Preview
                break;
            case '3':
                e.preventDefault();
                dotNetRef?.invokeMethodAsync('SwitchToView', 2); // JSON
                break;
        }
    }
}

/**
 * Focus an element by ID
 * @param {string} elementId - The ID of the element to focus
 */
window.focusElement = (elementId) => {
    const element = document.getElementById(elementId);
    if (element) {
        element.focus();
    }
};

/**
 * Copy text to clipboard
 * @param {string} text - The text to copy
 * @returns {Promise<boolean>} - Whether the copy was successful
 */
window.copyToClipboard = async (text) => {
    try {
        await navigator.clipboard.writeText(text);
        return true;
    } catch (err) {
        console.error('Failed to copy to clipboard:', err);
        return false;
    }
};

// === Unsaved Changes Warning ===

let hasUnsavedChanges = false;

/**
 * Enable unsaved changes warning (browser will prompt on page close/refresh)
 */
window.enableUnsavedWarning = () => {
    hasUnsavedChanges = true;
    window.addEventListener('beforeunload', handleBeforeUnload);
};

/**
 * Disable unsaved changes warning
 */
window.disableUnsavedWarning = () => {
    hasUnsavedChanges = false;
    window.removeEventListener('beforeunload', handleBeforeUnload);
};

/**
 * Handle beforeunload event - prompts user if there are unsaved changes
 * @param {BeforeUnloadEvent} e - The beforeunload event
 */
function handleBeforeUnload(e) {
    if (hasUnsavedChanges) {
        e.preventDefault();
        // Modern browsers require returnValue to be set
        e.returnValue = '';
        return '';
    }
}
