// wwwroot/js/interop.js

window.editorInterop = {
    downloadFile: (fileName, content) => {
        const blob = new Blob([content], { type: 'application/json' });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = fileName;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(url);
    },
    
    registerKeyboardShortcuts: (dotNetRef) => {
        document.addEventListener('keydown', (e) => {
            // Only trigger if not in an input field (unless it's a specific command like Save)
            const isInput = e.target.tagName === 'INPUT' || e.target.tagName === 'TEXTAREA' || e.target.isContentEditable;
            
            if (e.ctrlKey || e.metaKey) {
                switch (e.key.toLowerCase()) {
                    case 's':
                        e.preventDefault();
                        dotNetRef.invokeMethodAsync('OnSave');
                        break;
                    case 'z':
                        e.preventDefault();
                        if (e.shiftKey) {
                            dotNetRef.invokeMethodAsync('OnRedo');
                        } else {
                            dotNetRef.invokeMethodAsync('OnUndo');
                        }
                        break;
                    case 'y':
                        e.preventDefault();
                        dotNetRef.invokeMethodAsync('OnRedo');
                        break;
                    case 'c':
                        if (!isInput) {
                            // Don't prevent default, allow copy if something is selected
                            dotNetRef.invokeMethodAsync('OnCopy');
                        }
                        break;
                    case 'v':
                        if (!isInput) {
                            e.preventDefault();
                            dotNetRef.invokeMethodAsync('OnPaste');
                        }
                        break;
                    case 'd':
                        if (e.ctrlKey) { // Duplicate only on Ctrl+D
                             e.preventDefault();
                             dotNetRef.invokeMethodAsync('OnDuplicate');
                        }
                        break;
                }
            } else if (e.key === 'Delete') {
                if (!isInput) {
                    dotNetRef.invokeMethodAsync('OnDelete');
                }
            } else if (e.key === 'Escape') {
                dotNetRef.invokeMethodAsync('OnEscape');
            }
        });
    },

    setTheme: (theme) => {
        document.body.setAttribute('data-theme', theme);
    },

    getTheme: () => {
        return document.body.getAttribute('data-theme') || 'light';
    }
};
