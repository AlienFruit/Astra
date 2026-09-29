"use strict";

/**
 * @typedef {Object} ViewBoxSpecification
 * @property {string} id Unique identifier (required)
 * @property {boolean} changingBrowserAddressEnable Allows browser address change
 * @property {string} connectionErrorMessage
 * @property {number} [startLoadingEventDelay] Delay before start loading event (ms, default 100)
 */
class ViewBox {
    /**
     * @param {ViewBoxSpecification} specification Initialization parameters
     * @throws {Error} If required parameters are missing or of wrong type
     */
    constructor(specification) {
        const {
            id,
            changingBrowserAddressEnable,
            connectionErrorMessage,
            startLoadingEventDelay
        } = specification;

        // Validate required parameters
        if (typeof id !== 'string' || !id.trim()) {
            throw new Error('Invalid or missing id: must be non-empty string');
        }
        if (typeof changingBrowserAddressEnable !== 'boolean') {
            throw new Error('changingBrowserAddressEnable must be boolean');
        }
        if (typeof connectionErrorMessage !== 'string' || !connectionErrorMessage.trim()) {
            throw new Error('Invalid or missing connectionErrorMessage: must be non-empty string');
        }
        if (typeof startLoadingEventDelay !== 'number' || startLoadingEventDelay < 0) {
            throw new Error('startLoadingEventDelay must be a non-negative number');
        }

        // Initialize properties
        this._id = id;
        this._changingBrowserAddressEnable = changingBrowserAddressEnable;
        this._connectionErrorMessage = connectionErrorMessage;
        this._startLoadingEventDelay = startLoadingEventDelay;

        //local state
        this._uriArray = {};
        this._groupsArray = [];
        this._scrollUp = false;
        this._restoreScrollPosition = false;
        this._timer = undefined;
        this._contentToCancat = null;
        this._loadedScripts = new Set();
        this._isRequestInProgress = false;

        // subscribers
        this._onTimeoutAfterStartLoadingSubscribers = [];
        this._onStartLoadingSubscribers = [];
        this._onFinishLoadingSubscribers = [];
        this._onScriptsExecutedSubscribers = [];

        this._isLocked = false;

        for (let script of document.getElementsByTagName('script')) {
            if (script.src) {
                // Add only relative script path, e.g. /js/test.js
                const scriptUrl = new URL(script.src, window.location.origin);
                const relativePath = scriptUrl.pathname + scriptUrl.search + scriptUrl.hash;
                this._loadedScripts.add(relativePath);
            }
        }
    }

    /**
     * Subscribe to start loading event
     * @param {Function} fn
     */
    subscribeOnStartLoading(fn) {
        if (typeof fn !== 'function') {
            throw new Error('onStartLoading subscriber must be a function');
        }
        this._onStartLoadingSubscribers.push(fn);
    }

     /**
     * Subscribe to timeout after start loading event
     * @param {Function} fn
     */
     subscribeOnTimeoutAfterStartLoading(fn) {
        if (typeof fn !== 'function') {
            throw new Error('onTimeoutAfterStartLoading subscriber must be a function');
        }
        this._onTimeoutAfterStartLoadingSubscribers.push(fn);
    }

    /**
     * Subscribe to finish loading event
     * @param {Function} fn
     */
    subscribeOnFinishLoading(fn) {
        if (typeof fn !== 'function') {
            throw new Error('onFinishLoading subscriber must be a function');
        }
        this._onFinishLoadingSubscribers.push(fn);
    }

    /**
     * Subscribe to all inline scripts executed event
     * @param {Function} fn
     */
    subscribeOnScriptsExecuted(fn) {
        if (typeof fn !== 'function') {
            throw new Error('onScriptsExecuted subscriber must be a function');
        }
        this._onScriptsExecutedSubscribers.push(fn);
    }

    /**
     * Registers control element for specified URI
     * @param {string} uri URI for content loading
     * @param {string} elementId HTML element ID
     * @param {string} selectedClassName CSS class for active state
     * @param {string} defaultClassName CSS class for inactive state
     * @param {boolean} scrollUp Flag to scroll page up on activation
     */
    registerUri(uri, elementId, selectedClassName, defaultClassName, scrollUp) {
        const element = document.getElementById(elementId);
        if (!element) {
            throw new Error(`There is no element with id ${elementId}`);
        }

        if (!this._uriArray[uri]) {
            this._uriArray[uri] = [];
        }

        if (elementId in this._uriArray[uri]) {
            throw new Error(`Element with id ${elementId} is already registered for uri: ${uri}`);
        }

        this._uriArray[uri].push({
            id: elementId,
            uri: uri,
            defaultClass: defaultClassName,
            selectedClass: selectedClassName,
            scrollUp: scrollUp
        });

        var self = this;

        element.onclick = function () {
            self._selectUri(uri);
            self._sendRequest(uri);
            return false;
        }
    }

    /**
     * Registers a group of elements that will change style when any of the specified URIs is active.
     * @param {string} groupElementId HTML element ID of the group container
     * @param {string} selectedClassName CSS class applied when at least one URI from uriListToActivate is active
     * @param {string} defaultClassName CSS class applied when none of the URIs from uriListToActivate is active
     * @param {string[]} uriListToActivate Array of URIs that will activate the selected state for this group
     */
    registerUriGroup(groupElementId, selectedClassName, defaultClassName, uriListToActivate) {
        this._groupsArray.push({
            id: groupElementId,
            selectedClass: selectedClassName,
            defaultClass: defaultClassName,
            uriListToActivate: uriListToActivate
        });
    }

    /**
     * Sends request to load content for specified URI
     * @param {string} uri URI for content loading
     * @returns {boolean} false (to prevent default behavior)
     */
    sendRequest(uri) {
        this._selectUri(uri);
        this._sendRequest(uri, null, false);
        return false;
    }

    /**
     * Sends request to load content with scroll position restoration
     * @param {string} uri URI for content loading
     * @returns {boolean} false (to prevent default behavior)
     */
    sendRequestAndRestoreScrollPosition(uri) {
        this._restoreScrollPosition = true;
        this._selectUri(uri);
        this._sendRequest(uri, null, true);
        return false;
    }

    /**
     * Cancels current running request
     * @returns {boolean} true if request was successfully cancelled, false if there was no request or it was already cancelled
     */
    abortCurrentRequest() {
        if (this.abortController && !this.abortController.signal.aborted) {
            this.abortController.abort();
            console.log('Current request aborted by user');
            return true;
        }
        return false;
    }

    /**
     * Checks if request is currently in progress
     * @returns {boolean} true if request is in progress
     */
    get isRequestInProgress() {
        return this._isRequestInProgress;
    }

    /** 
     * Cleans up all ViewBox resources (timers, subscribers)
     */
    cleanup() {
        // Clear timers
        if (this._timer) {
            clearTimeout(this._timer);
            this._timer = undefined;
        }

        // Clear subscribers
        this._onStartLoadingSubscribers = [];
        this._onTimeoutAfterStartLoadingSubscribers = [];
        this._onFinishLoadingSubscribers = [];
        this._onScriptsExecutedSubscribers = [];
        this._isLocked = false;
    }

    /**
     * Lock request sending for current ViewBox 
     */
    lock() {
        this._isLocked = true;
    }

    _selectUri(uriToSelect) {
        Object.values(this._uriArray).forEach(tabs => {
            tabs.forEach(tab => {
                const tabElement = document.getElementById(tab.id);
                if (!tabElement) {
                    return;
                }

                // Case-insensitive URI comparison
                if (tab.uri.toLowerCase() !== uriToSelect.toLowerCase()) {
                    tabElement.className = tab.defaultClass;
                    return;
                }

                if (tab.selectedClass !== null && tab.selectedClass !== '') {
                    tabElement.className = tab.selectedClass;
                }

                this._scrollUp = tab.scrollUp;
            });
        });

        this._selectGroups(uriToSelect);
    }

    /**
     * Processes element groups and updates their classes based on the selected URI.
     * @param {string} uriToSelect URI that was selected
     * @private
     */
    _selectGroups(uriToSelect) {
        this._groupsArray.forEach(group => {
            // Check if the selected URI is in the group's activation list (case-insensitive comparison)
            const isUriInGroup = group.uriListToActivate && group.uriListToActivate.some(
                uri => uri.toLowerCase() === uriToSelect.toLowerCase()
            );
            
            const groupElement = document.getElementById(group.id);
            if (!groupElement) {
                return;
            }

            if (isUriInGroup) {
                if (group.selectedClass !== null && group.selectedClass !== '') {
                    groupElement.className = group.selectedClass;
                }
            } else {
                groupElement.className = group.defaultClass;
            }
        });
    }

    _sendRequest(uri, postParams, dontSaveHistory) {
        if (this._isLocked) {
            return;
        }

        // Cancel previous request if it's still running
        if (this.abortController && !this.abortController.signal.aborted) {
            this.abortController.abort();
        }

        this._sendStartLoadingEvent();

        this.abortController = new AbortController();
        this._isRequestInProgress = true;
        
        const fetchOptions = {
            method: postParams ? 'POST' : 'GET',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'Ajax-Request': 'true'
            },
            signal: this.abortController.signal
        };

        if (postParams) {
            fetchOptions.body = postParams;
        }

        // Execute request
        fetch(uri, fetchOptions)
            .then(response => {
                // Save final URL after possible redirect
                this._saveBrowserHistory(response.url, dontSaveHistory);
                return Promise.all([response.text(), response.url]);
            })
            .then(([text, url]) => {
                this._handleResponse(text, url);
                this._sendEndLoadingEvent();
            })
            .catch(error => {
                this._sendEndLoadingEvent();
                if (error.name === 'AbortError') {
                    console.log('Request aborted');
                    clearTimeout(this._timer); // Cancel loading timer when request is cancelled
                    return;
                }

                document.getElementById(this._id).innerHTML = this._connectionErrorMessage;
            })
            .finally(() => {
                this._isRequestInProgress = false;
            });
    }

    _sendStartLoadingEvent() {
        this._timer = setTimeout((self) => {
            self._onTimeoutAfterStartLoadingSubscribers.forEach(fn => {
                try { fn(); } catch (e) { console.error('onTimeoutAfterStartLoading subscriber error', e); }
            });
        }, this._startLoadingEventDelay, this);

        this._onStartLoadingSubscribers.forEach(fn => {
            try { fn(); } catch (e) { console.error('onStartLoading subscriber error', e); }
        });
    }

    _sendEndLoadingEvent() {
        clearTimeout(this._timer);
        this._onFinishLoadingSubscribers.forEach(fn => {
            try { fn(); } catch (e) { console.error('onFinishLoading subscriber error', e); }
        });
    }

    _scriptsExecutedEvent() {
        this._onScriptsExecutedSubscribers.forEach(fn => {
            try { fn(); } catch (e) { console.error('onScriptsExecuted subscriber error', e); }
        });
    }

    _saveBrowserHistory(uri, dontSaveHistory) {
        if (!this._changingBrowserAddressEnable || dontSaveHistory) {
            return;
        }

        window.history.replaceState({ scroll: window.scrollY }, null, window.location.href);
        window.history.pushState({ scroll: 0 }, null, uri);
    }

    _loadScript(src) {
        if (this._loadedScripts.has(src)) {
            return Promise.resolve();
        }

        return new Promise((resolve, reject) => {
            const script = document.createElement('script');
            script.src = src;
            script.onload = () => {
                this._loadedScripts.add(src);
                resolve();
            };
            script.onerror = reject;
            document.head.appendChild(script);
        });
    }

    async _handleResponse(response, url) {
        if (!response || response.length === 0) {
            return;
        }
        
        const re = /<script\b[^>]*>([\s\S]*?)<\/script>/gm;
        const srcRe = /<script\b[^>]*src=["']([^"']+)["'][^>]*>/gm;
        const voidTag = /<void\/>/gm;

        let content = response;
        let html_content = content.replace(re, '');

        // Content processing
        if (!voidTag.test(content)) {
            document.getElementById(this._id).innerHTML = html_content;
        }

        // Page scrolling
        if (this._scrollUp) {
            window.scrollTo(0, 0);
            this._scrollUp = false;
        } else if (this._restoreScrollPosition) {
            window.scrollTo(0, history.state?.scroll || 0);
            this._restoreScrollPosition = false;
        }

        // Extract and load external scripts
        let srcMatch;
        while ((srcMatch = srcRe.exec(content)) !== null) {
            try {
                await this._loadScript(srcMatch[1]);
            } catch (e) {
                console.error(`Script load error: ${srcMatch[1]}`, e);
            }
        }

        // Extract and execute inline scripts
        let j_script_content = '';
        let match;
        while ((match = re.exec(content)) !== null) {
            j_script_content += match[1];
        }

        if (j_script_content) {
            try {
                eval(j_script_content);
            } catch (e) {
                console.error('Error executing inline script:', e);
            }
        }
        this._scriptsExecutedEvent();
    }
}

class ViewBoxRegistry {
    static _registry = {};

    /**
     * Creates new ViewBox instance and registers it
     * @param {string} id Unique ViewBox identifier
     * @param {ViewBoxSpecification} specification ViewBox initialization parameters
     * @returns {ViewBox} Created ViewBox instance
     * @throws {Error} If ViewBox with this id already exists
     */
    static create(id, specification) {
        if (this.isCreated(id)) {
            throw new Error(`ViewBox with id '${id}' already exists. Use destroy() first to remove existing ViewBox.`);
        }

        const instance = new ViewBox(specification);
        this._registry[id] = instance;
        document.dispatchEvent(new CustomEvent('ViewBoxReady', { detail: { viewBoxId: id } }));
        return instance;
    }

    /**
     * Gets existing ViewBox instance or creates new one if it doesn't exist
     * @param {string} id Unique ViewBox identifier
     * @param {ViewBoxSpecification} specification ViewBox initialization parameters (used only when creating)
     * @returns {ViewBox} ViewBox instance
     */
    static getOrCreate(id, specification) {
        if (this.isCreated(id)) {
            return this.get(id);
        }

        const instance = new ViewBox(specification);
        this._registry[id] = instance;
        document.dispatchEvent(new CustomEvent('ViewBoxReady', { detail: { viewBoxId: id } }));
        return instance;
    }

    /**
     * Gets ViewBox instance by identifier
     * @param {string} id ViewBox identifier
     * @returns {ViewBox} ViewBox instance
     * @throws {Error} If ViewBox with specified id is not found
     */
    static get(id) {
        const instance = this._registry[id];
        if (!instance) throw new Error(`ViewBox ${id} not found`);
        return instance;
    }

    /**
     * Checks if ViewBox with specified identifier is created
     * @param {string} id ViewBox identifier
     * @returns {boolean} true if ViewBox exists
     */
    static isCreated(id) {
        return id in this._registry;
    }

    /**
     * Destroys ViewBox object and removes it from registry
     * @param {string} id ViewBox identifier to destroy
     * @returns {boolean} true if object was successfully destroyed, false if object not found
     */
    static destroy(id) {
        const instance = this._registry[id];
        if (!instance) {
            return false;
        }

        if (instance.isRequestInProgress) {
            instance.abortCurrentRequest();
        }

        instance.cleanup();
        delete this._registry[id];
        document.dispatchEvent(new CustomEvent('ViewBoxDestroyed', { detail: { viewBoxId: id } }));
        return true;
    }

    /**
     * Destroys all ViewBox objects in registry
     * Automatically cancels all running requests before destruction
     */
    static destroyAll() {
        const ids = Object.keys(this._registry);
        ids.forEach(id => this.destroy(id));
    }

    /**
     * Gets ViewBox instance by identifier, or calls callback when it becomes ready
     * @param {string} id ViewBox identifier
     * @param {function} callback Function that will be called with ViewBox instance
     */
    static getOrOnReady(id, callback) {
        if (this.isCreated(id)) {
            callback(this.get(id));
        } else {
            const handler = function(e) {
                if (e.detail.viewBoxId === id) {
                    callback(ViewBoxRegistry.get(id));
                    document.removeEventListener('ViewBoxReady', handler);
                }
            };
            document.addEventListener('ViewBoxReady', handler);
        }
    }

    /**
     * Opens link in specified ViewBox.
     * @param {string} viewBoxId - ViewBox identifier.
     * @param {string} href - URL to open.
     */
    static sendRequest(viewBoxId, href) {
        ViewBoxRegistry.getOrOnReady(viewBoxId, function (viewBox) {
            viewBox.sendRequest(href);
        });
    }

    /**
     * Cancels current running request in specified ViewBox.
     * @param {string} viewBoxId - ViewBox identifier.
     * @returns {boolean} true if request was successfully cancelled
     */
    static abortCurrentRequest(viewBoxId) {
        if (ViewBoxRegistry.isCreated(viewBoxId)) {
            return ViewBoxRegistry.get(viewBoxId).abortCurrentRequest();
        }
        return false;
    }
}