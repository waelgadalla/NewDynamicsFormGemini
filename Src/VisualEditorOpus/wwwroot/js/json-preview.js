// JSON Preview JavaScript Interop

let dotNetRef = null;
let contentElement = null;

/**
 * Initialize the JSON Preview component
 * @param {object} objRef - DotNet object reference
 * @param {HTMLElement} element - JSON content element reference
 */
window.initJsonPreview = (objRef, element) => {
    dotNetRef = objRef;
    contentElement = element;

    // Setup keyboard shortcuts
    document.addEventListener('keydown', handleKeyDown);

    // Setup scroll sync
    if (contentElement) {
        contentElement.addEventListener('scroll', handleScroll);
    }
};

/**
 * Dispose the JSON Preview component
 */
window.disposeJsonPreview = () => {
    document.removeEventListener('keydown', handleKeyDown);
    if (contentElement) {
        contentElement.removeEventListener('scroll', handleScroll);
    }
    dotNetRef = null;
    contentElement = null;
};

/**
 * Handle keyboard shortcuts
 * @param {KeyboardEvent} e - Keyboard event
 */
function handleKeyDown(e) {
    // Ctrl+F to toggle search
    if (e.ctrlKey && e.key === 'f') {
        e.preventDefault();
        dotNetRef?.invokeMethodAsync('ToggleSearch');
    }
}

/**
 * Handle scroll events for minimap sync
 */
function handleScroll() {
    if (contentElement) {
        const scrollPosition = contentElement.scrollTop / contentElement.scrollHeight;
        dotNetRef?.invokeMethodAsync('UpdateScrollPosition', scrollPosition);
    }
}

/**
 * Download JSON content as a file
 * @param {string} content - JSON content
 * @param {string} filename - Desired filename
 */
window.downloadJson = (content, filename) => {
    const blob = new Blob([content], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
};

/**
 * Trigger file input for JSON import
 * @param {object} objRef - DotNet object reference
 */
window.triggerFileInput = (objRef) => {
    const input = document.createElement('input');
    input.type = 'file';
    input.accept = '.json,application/json';
    input.onchange = async (e) => {
        const file = e.target.files[0];
        if (file) {
            try {
                const content = await file.text();
                objRef.invokeMethodAsync('HandleFileImport', content);
            } catch (error) {
                console.error('Error reading file:', error);
            }
        }
    };
    input.click();
};

/**
 * Scroll to a specific line in the JSON content
 * @param {number} lineNumber - Line number to scroll to (1-based)
 */
window.scrollToLine = (lineNumber) => {
    if (contentElement) {
        const lineHeight = 24; // Should match CSS line-height
        const scrollTop = (lineNumber - 1) * lineHeight;
        contentElement.scrollTo({
            top: scrollTop,
            behavior: 'smooth'
        });
    }
};

/**
 * Highlight a search match in the content
 * @param {number} startIndex - Start index of the match
 * @param {number} length - Length of the match
 */
window.highlightSearchMatch = (startIndex, length) => {
    // This would require more complex DOM manipulation
    // For now, we rely on server-side highlighting
};

/**
 * Copy text to clipboard with fallback for older browsers
 * @param {string} text - Text to copy
 * @returns {Promise<boolean>} - Success status
 */
window.copyToClipboard = async (text) => {
    try {
        if (navigator.clipboard && window.isSecureContext) {
            await navigator.clipboard.writeText(text);
            return true;
        } else {
            // Fallback for older browsers
            const textArea = document.createElement('textarea');
            textArea.value = text;
            textArea.style.position = 'fixed';
            textArea.style.left = '-999999px';
            textArea.style.top = '-999999px';
            document.body.appendChild(textArea);
            textArea.focus();
            textArea.select();
            const result = document.execCommand('copy');
            document.body.removeChild(textArea);
            return result;
        }
    } catch (error) {
        console.error('Failed to copy:', error);
        return false;
    }
};
