/**
 * Signature Pad Resize Handler
 *
 * KNOWN ISSUE: The underlying Blazor.SignaturePad NuGet package (which wraps signature_pad.js)
 * has a known issue where the canvas does not properly handle resize events, particularly on
 * mobile devices when the user changes orientation (portrait <-> landscape).
 *
 * The problem manifests as:
 * - Signature appears zoomed/scaled incorrectly after resize
 * - Signature position shifts after orientation change
 * - Canvas dimensions don't match the container after resize
 *
 * This module provides a workaround by:
 * 1. Listening for window resize and orientation change events
 * 2. Debouncing the events to prevent excessive redraws
 * 3. Calling back to Blazor to trigger a component refresh
 *
 * References:
 * - https://github.com/szimek/signature_pad/issues/362
 * - https://www.telerik.com/blazor-ui/documentation/knowledge-base/signature-relative-width-height
 */

window.SignatureResizeHandler = {
    // Store references to registered components
    _registrations: new Map(),

    // Debounce timer
    _resizeTimeout: null,

    // Track if we've initialized the global listeners
    _initialized: false,

    /**
     * Initialize global resize/orientation listeners
     */
    _initGlobalListeners: function() {
        if (this._initialized) return;

        const handler = this._handleResize.bind(this);

        // Handle window resize (desktop)
        window.addEventListener('resize', handler);

        // Handle orientation change (mobile)
        // Note: orientationchange is deprecated but still widely supported
        // Screen Orientation API is the modern replacement
        window.addEventListener('orientationchange', handler);

        // Modern Screen Orientation API (where supported)
        if (screen.orientation) {
            screen.orientation.addEventListener('change', handler);
        }

        this._initialized = true;
    },

    /**
     * Debounced resize handler
     */
    _handleResize: function() {
        // Clear existing timeout
        if (this._resizeTimeout) {
            clearTimeout(this._resizeTimeout);
        }

        // Debounce: wait 250ms after last resize event
        this._resizeTimeout = setTimeout(() => {
            this._notifyAllComponents();
        }, 250);
    },

    /**
     * Notify all registered components about the resize
     */
    _notifyAllComponents: function() {
        this._registrations.forEach((dotNetRef, elementId) => {
            try {
                dotNetRef.invokeMethodAsync('OnWindowResized');
            } catch (e) {
                // Component may have been disposed
                console.warn(`SignatureResizeHandler: Failed to notify component ${elementId}`, e);
                this._registrations.delete(elementId);
            }
        });
    },

    /**
     * Register a Blazor component to receive resize notifications
     * @param {string} elementId - Unique identifier for the signature pad element
     * @param {object} dotNetRef - DotNet object reference for callbacks
     */
    register: function(elementId, dotNetRef) {
        this._initGlobalListeners();
        this._registrations.set(elementId, dotNetRef);
        console.debug(`SignatureResizeHandler: Registered ${elementId}`);
    },

    /**
     * Unregister a component (call on dispose)
     * @param {string} elementId - The element ID to unregister
     */
    unregister: function(elementId) {
        this._registrations.delete(elementId);
        console.debug(`SignatureResizeHandler: Unregistered ${elementId}`);
    },

    /**
     * Get current window dimensions
     * @returns {object} Object with width and height
     */
    getWindowSize: function() {
        return {
            width: window.innerWidth,
            height: window.innerHeight,
            orientation: screen.orientation ? screen.orientation.type :
                (window.innerHeight > window.innerWidth ? 'portrait' : 'landscape')
        };
    },

    /**
     * Force a specific signature pad to redraw
     * This can be called after programmatic resize
     * @param {string} canvasSelector - CSS selector for the canvas element
     */
    triggerRedraw: function(canvasSelector) {
        const canvas = document.querySelector(canvasSelector);
        if (canvas) {
            // Dispatch a resize event on the canvas
            canvas.dispatchEvent(new Event('resize'));
        }
    }
};
