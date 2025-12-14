// Theme Editor JavaScript - Keyboard shortcuts and utilities

window.themeEditorShortcuts = {
    _dotNetRef: null,
    _keydownHandler: null,

    register: function (dotNetRef) {
        this._dotNetRef = dotNetRef;

        this._keydownHandler = (e) => {
            // Check if we're in an input field - don't intercept shortcuts there
            const activeElement = document.activeElement;
            const isInInput = activeElement && (
                activeElement.tagName === 'INPUT' ||
                activeElement.tagName === 'TEXTAREA' ||
                activeElement.isContentEditable
            );

            // Ctrl+Z - Undo
            if (e.ctrlKey && !e.shiftKey && e.key === 'z') {
                if (!isInInput) {
                    e.preventDefault();
                    this._dotNetRef.invokeMethodAsync('OnUndoShortcut');
                }
            }
            // Ctrl+Y or Ctrl+Shift+Z - Redo
            else if ((e.ctrlKey && e.key === 'y') || (e.ctrlKey && e.shiftKey && e.key === 'z') || (e.ctrlKey && e.shiftKey && e.key === 'Z')) {
                if (!isInInput) {
                    e.preventDefault();
                    this._dotNetRef.invokeMethodAsync('OnRedoShortcut');
                }
            }
            // Ctrl+S - Save (prevent default browser save)
            else if (e.ctrlKey && e.key === 's') {
                e.preventDefault();
                // Optional: Could invoke save method
            }
        };

        document.addEventListener('keydown', this._keydownHandler);
    },

    unregister: function () {
        if (this._keydownHandler) {
            document.removeEventListener('keydown', this._keydownHandler);
            this._keydownHandler = null;
        }
        this._dotNetRef = null;
    }
};
