// ===================================================================
// Workflow Canvas - JavaScript Interop
// E.3 MinimapControls Implementation
// ===================================================================

let dotNetRef = null;
let containerElement = null;

/**
 * Initialize the workflow canvas with keyboard event handlers
 * @param {Object} objRef - DotNet object reference for callbacks
 * @param {HTMLElement} element - Container element reference
 */
window.initWorkflowCanvas = (objRef, element) => {
    dotNetRef = objRef;
    containerElement = element;

    document.addEventListener('keydown', handleKeyDown);
    document.addEventListener('keyup', handleKeyUp);
};

/**
 * Dispose the workflow canvas and remove event handlers
 */
window.disposeWorkflowCanvas = () => {
    document.removeEventListener('keydown', handleKeyDown);
    document.removeEventListener('keyup', handleKeyUp);
    dotNetRef = null;
    containerElement = null;
};

/**
 * Handle keydown events
 * @param {KeyboardEvent} e - Keyboard event
 */
function handleKeyDown(e) {
    if (dotNetRef) {
        // Handle keyboard shortcuts
        if (e.ctrlKey || e.metaKey) {
            if (e.key === '0') {
                e.preventDefault();
                dotNetRef.invokeMethodAsync('HandleResetView');
                return;
            }
            if (e.key === '1') {
                e.preventDefault();
                dotNetRef.invokeMethodAsync('HandleFitView');
                return;
            }
        }

        dotNetRef.invokeMethodAsync('HandleKeyDown', e.key);
    }
}

/**
 * Handle keyup events
 * @param {KeyboardEvent} e - Keyboard event
 */
function handleKeyUp(e) {
    if (dotNetRef) {
        dotNetRef.invokeMethodAsync('HandleKeyUp', e.key);
    }
}

/**
 * Get the bounding client rect of an element
 * @param {HTMLElement} element - Target element
 * @returns {Object} - Bounding rect with left, top, width, height
 */
window.getBoundingClientRect = (element) => {
    if (!element) {
        return { left: 0, top: 0, width: 0, height: 0 };
    }

    const rect = element.getBoundingClientRect();
    return {
        left: rect.left,
        top: rect.top,
        width: rect.width,
        height: rect.height
    };
};

/**
 * Get the scroll position of the container
 * @param {HTMLElement} element - Container element
 * @returns {Object} - Scroll position with scrollLeft and scrollTop
 */
window.getScrollPosition = (element) => {
    if (!element) {
        return { scrollLeft: 0, scrollTop: 0 };
    }

    return {
        scrollLeft: element.scrollLeft,
        scrollTop: element.scrollTop
    };
};

/**
 * Set the scroll position of the container
 * @param {HTMLElement} element - Container element
 * @param {number} left - Scroll left position
 * @param {number} top - Scroll top position
 */
window.setScrollPosition = (element, left, top) => {
    if (element) {
        element.scrollLeft = left;
        element.scrollTop = top;
    }
};

/**
 * Get mouse position relative to an element
 * @param {MouseEvent} event - Mouse event
 * @param {HTMLElement} element - Reference element
 * @returns {Object} - Position with x and y coordinates
 */
window.getMousePosition = (event, element) => {
    if (!element) {
        return { x: event.clientX, y: event.clientY };
    }

    const rect = element.getBoundingClientRect();
    return {
        x: event.clientX - rect.left,
        y: event.clientY - rect.top
    };
};

/**
 * Check if an element is in the viewport
 * @param {HTMLElement} element - Element to check
 * @returns {boolean} - Whether element is visible
 */
window.isElementInViewport = (element) => {
    if (!element) return false;

    const rect = element.getBoundingClientRect();
    return (
        rect.top >= 0 &&
        rect.left >= 0 &&
        rect.bottom <= (window.innerHeight || document.documentElement.clientHeight) &&
        rect.right <= (window.innerWidth || document.documentElement.clientWidth)
    );
};

/**
 * Smoothly scroll to center an element in the viewport
 * @param {HTMLElement} container - Container element
 * @param {HTMLElement} target - Target element to center
 */
window.scrollToCenter = (container, target) => {
    if (!container || !target) return;

    const containerRect = container.getBoundingClientRect();
    const targetRect = target.getBoundingClientRect();

    const scrollLeft = target.offsetLeft - (containerRect.width / 2) + (targetRect.width / 2);
    const scrollTop = target.offsetTop - (containerRect.height / 2) + (targetRect.height / 2);

    container.scrollTo({
        left: scrollLeft,
        top: scrollTop,
        behavior: 'smooth'
    });
};
