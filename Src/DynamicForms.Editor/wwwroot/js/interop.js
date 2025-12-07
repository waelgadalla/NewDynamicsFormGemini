// wwwroot/js/interop.js

window.registerKeyboardShortcuts = (dotNetRef) => {
    document.addEventListener('keydown', (e) => {
        // Only trigger if not in an input field (unless it's a specific command like Save)
        const isInput = e.target.tagName === 'INPUT' || e.target.tagName === 'TEXTAREA' || e.target.isContentEditable;
        
        if (e.ctrlKey || e.metaKey) {
            switch (e.key.toLowerCase()) {
                case 's':
                    e.preventDefault();
                    dotNetRef.invokeMethodAsync('Save');
                    break;
                case 'z':
                    e.preventDefault();
                    if (e.shiftKey) {
                        dotNetRef.invokeMethodAsync('Redo');
                    } else {
                        dotNetRef.invokeMethodAsync('Undo');
                    }
                    break;
                case 'y':
                    e.preventDefault();
                    dotNetRef.invokeMethodAsync('Redo');
                    break;
                case 'c':
                    if (!isInput) {
                        e.preventDefault();
                        dotNetRef.invokeMethodAsync('Copy');
                    }
                    break;
                case 'v':
                    if (!isInput) {
                        e.preventDefault();
                        dotNetRef.invokeMethodAsync('Paste');
                    }
                    break;
                case 'd':
                    e.preventDefault();
                    dotNetRef.invokeMethodAsync('Duplicate');
                    break;
            }
        } else if (e.key === 'Delete') {
            if (!isInput) {
                dotNetRef.invokeMethodAsync('Delete');
            }
        } else if (e.key === 'Escape') {
            dotNetRef.invokeMethodAsync('Escape');
        }
    });
};

window.setTheme = (theme) => {
    document.body.setAttribute('data-theme', theme);
};

window.getTheme = () => {
    return document.body.getAttribute('data-theme') || 'light';
};
