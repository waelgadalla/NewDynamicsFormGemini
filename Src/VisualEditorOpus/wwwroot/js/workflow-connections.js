// ===================================================================
// Workflow Connections - JavaScript Interop
// E.2 ConnectionDrawing Implementation
// ===================================================================

window.workflowConnections = {
    /**
     * Get the total length of an SVG path
     * @param {SVGPathElement} pathElement - The SVG path element
     * @returns {number} - Total length of the path
     */
    getPathLength: (pathElement) => {
        if (!pathElement || typeof pathElement.getTotalLength !== 'function') {
            return 0;
        }
        return pathElement.getTotalLength();
    },

    /**
     * Get a point at a specific length along the path
     * @param {SVGPathElement} pathElement - The SVG path element
     * @param {number} length - Length along the path
     * @returns {Object} - Point with x and y coordinates
     */
    getPointAtLength: (pathElement, length) => {
        if (!pathElement || typeof pathElement.getPointAtLength !== 'function') {
            return { x: 0, y: 0 };
        }
        const point = pathElement.getPointAtLength(length);
        return { x: point.x, y: point.y };
    },

    /**
     * Animate a path being drawn
     * @param {SVGPathElement} pathElement - The SVG path element
     * @param {number} duration - Animation duration in milliseconds
     */
    animatePath: (pathElement, duration) => {
        if (!pathElement || typeof pathElement.getTotalLength !== 'function') {
            return;
        }

        const length = pathElement.getTotalLength();
        pathElement.style.strokeDasharray = length;
        pathElement.style.strokeDashoffset = length;
        pathElement.style.animation = `draw-path ${duration}ms ease forwards`;
    },

    /**
     * Get the midpoint of a path for positioning delete buttons
     * @param {SVGPathElement} pathElement - The SVG path element
     * @returns {Object} - Point with x and y coordinates
     */
    getPathMidpoint: (pathElement) => {
        if (!pathElement || typeof pathElement.getTotalLength !== 'function') {
            return { x: 0, y: 0 };
        }

        const length = pathElement.getTotalLength();
        const midPoint = pathElement.getPointAtLength(length / 2);
        return { x: midPoint.x, y: midPoint.y };
    },

    /**
     * Get mouse position relative to SVG element
     * @param {MouseEvent} event - Mouse event
     * @param {SVGElement} svgElement - The SVG container element
     * @returns {Object} - Point with x and y coordinates
     */
    getMousePosition: (event, svgElement) => {
        if (!svgElement) {
            return { x: event.clientX, y: event.clientY };
        }

        const rect = svgElement.getBoundingClientRect();
        return {
            x: event.clientX - rect.left,
            y: event.clientY - rect.top
        };
    },

    /**
     * Check if a point is near a path (for hit testing)
     * @param {SVGPathElement} pathElement - The SVG path element
     * @param {number} x - X coordinate
     * @param {number} y - Y coordinate
     * @param {number} threshold - Distance threshold
     * @returns {boolean} - Whether point is near path
     */
    isPointNearPath: (pathElement, x, y, threshold = 10) => {
        if (!pathElement || typeof pathElement.getTotalLength !== 'function') {
            return false;
        }

        const length = pathElement.getTotalLength();
        const step = 5; // Check every 5 pixels

        for (let i = 0; i < length; i += step) {
            const point = pathElement.getPointAtLength(i);
            const dx = point.x - x;
            const dy = point.y - y;
            const distance = Math.sqrt(dx * dx + dy * dy);

            if (distance <= threshold) {
                return true;
            }
        }

        return false;
    }
};

// Add CSS animation for path drawing
(function() {
    const style = document.createElement('style');
    style.textContent = `
        @keyframes draw-path {
            to {
                stroke-dashoffset: 0;
            }
        }
    `;
    document.head.appendChild(style);
})();
