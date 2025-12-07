"use strict";

/**
 * @typedef {Object} ViewBoxSpecification
 * @property {string} id Уникальный идентификатор (обязательный)
 * @property {boolean} changingBrowserAddressEnable Разрешает изменение адреса браузера
 * @property {string} connectionErrorMessage
 * @property {number} [startLoadingEventDelay] Задержка перед событием начала загрузки (мс, по умолчанию 100)
 */
class ViewBox {
    /**
     * @param {ViewBoxSpecification} specification Параметры инициализации
     * @throws {Error} Если обязательные параметры отсутствуют или неверного типа
     */
    constructor(specification) {
        const {
            id,
            changingBrowserAddressEnable,
            connectionErrorMessage,
            startLoadingEventDelay
        } = specification;

        // Валидация обязательных параметров
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

        // Инициализация свойств
        this._id = id;
        this._changingBrowserAddressEnable = changingBrowserAddressEnable;
        this._connectionErrorMessage = connectionErrorMessage;
        this._startLoadingEventDelay = startLoadingEventDelay;

        //local state
        this._uriArray = {};
        this._scrollUp = false;
        this._restoreScrollPosition = false;
        this._timer = undefined;
        this._contentToCancat = null;
        this._loadedScripts = new Set();
        this._isRequestInProgress = false;

        // подписчики
        this._onStartLoadingSubscribers = [];
        this._onFinishLoadingSubscribers = [];
        this._onScriptsExecutedSubscribers = [];

        for (let script of document.getElementsByTagName('script')) {
            if (script.src) {
                // Добавляем только относительный путь скрипта, например /js/test.js
                const scriptUrl = new URL(script.src, window.location.origin);
                const relativePath = scriptUrl.pathname + scriptUrl.search + scriptUrl.hash;
                this._loadedScripts.add(relativePath);
            }
        }
    }

    /**
     * Подписка на событие начала загрузки
     * @param {Function} fn
     */
    subscribeOnStartLoading(fn) {
        if (typeof fn !== 'function') {
            throw new Error('Подписчик onStartLoading должен быть функцией');
        }
        this._onStartLoadingSubscribers.push(fn);
    }

    /**
     * Подписка на событие завершения загрузки
     * @param {Function} fn
     */
    subscribeOnFinishLoading(fn) {
        if (typeof fn !== 'function') {
            throw new Error('Подписчик onFinishLoading должен быть функцией');
        }
        this._onFinishLoadingSubscribers.push(fn);
    }

    /**
     * Подписка на событие выполнения всех inline-скриптов
     * @param {Function} fn
     */
    subscribeOnScriptsExecuted(fn) {
        if (typeof fn !== 'function') {
            throw new Error('Подписчик onScriptsExecuted должен быть функцией');
        }
        this._onScriptsExecutedSubscribers.push(fn);
    }

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

    sendRequest(uri) {
        this._selectUri(uri);
        this._sendRequest(uri, null, false);
        return false;
    }

    sendRequestAndRestoreScrollPosition(uri) {
        this._restoreScrollPosition = true;
        this._selectUri(uri);
        this._sendRequest(uri, null, true);
        return false;
    }

    /**
     * Отменяет текущий выполняющийся запрос
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
     * Проверяет, выполняется ли запрос в данный момент
     * @returns {boolean} true, если запрос выполняется
     */
    get isRequestInProgress() {
        return this._isRequestInProgress;
    }

    /**
     * Очищает все ресурсы ViewBox (таймеры, подписчики)
     */
    cleanup() {
        // Очищаем таймеры
        if (this._timer) {
            clearTimeout(this._timer);
            this._timer = undefined;
        }

        // Очищаем подписчики
        this._onStartLoadingSubscribers = [];
        this._onFinishLoadingSubscribers = [];
        this._onScriptsExecutedSubscribers = [];
    }

    _selectUri(uriToSelect) {
        Object.values(this._uriArray).forEach(tabs => {
            tabs.forEach(tab => {
                const tabElement = document.getElementById(tab.id);
                if (!tabElement) {
                    return;
                }

                if (tab.uri !== uriToSelect) {
                    tabElement.className = tab.defaultClass;
                    return;
                }

                if (tab.selectedClass !== null && tab.selectedClass !== '') {
                    tabElement.className = tab.selectedClass;
                }

                this._scrollUp = tab.scrollUp;
            });
        });
    }

    _sendRequest(uri, postParams, dontSaveHistory) {
        // Отмена предыдущего запроса, если он еще выполняется
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

        // Выполнение запроса
        fetch(uri, fetchOptions)
            .then(response => {
                // Сохраняем финальный URL после возможного редиректа
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
                    clearTimeout(this._timer); // Отменяем таймер загрузки при отмене запроса
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
            self._onStartLoadingSubscribers.forEach(fn => {
                try { fn(); } catch (e) { console.error('onStartLoading subscriber error', e); }
            });
        }, this._startLoadingEventDelay, this);
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

        // Обработка контента
        if (!voidTag.test(content)) {
            document.getElementById(this._id).innerHTML = html_content;
        }

        // Прокрутка страницы
        if (this._scrollUp) {
            window.scrollTo(0, 0);
            this._scrollUp = false;
        } else if (this._restoreScrollPosition) {
            window.scrollTo(0, history.state?.scroll || 0);
            this._restoreScrollPosition = false;
        }

        // Извлечение и загрузка внешних скриптов
        let srcMatch;
        while ((srcMatch = srcRe.exec(content)) !== null) {
            try {
                await this._loadScript(srcMatch[1]);
            } catch (e) {
                console.error(`Script load error: ${srcMatch[1]}`, e);
            }
        }

        // Извлечение и выполнение встроенных
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

    static create(id, specification) {
        const instance = new ViewBox(specification);
        this._registry[id] = instance;
        document.dispatchEvent(new CustomEvent('ViewBoxReady', { detail: { viewBoxId: id } }));
        return instance;
    }

    static get(id) {
        const instance = this._registry[id];
        if (!instance) throw new Error(`ViewBox ${id} not found`);
        return instance;
    }

    static isCreated(id) {
        return id in this._registry;
    }

    /**
     * Уничтожает объект ViewBox и удаляет его из реестра
     * @param {string} id Идентификатор ViewBox для уничтожения
     * @returns {boolean} true, если объект был успешно уничтожен, false если объект не найден
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
     * Уничтожает все объекты ViewBox в реестре
     */
    static destroyAll() {
        const ids = Object.keys(this._registry);
        ids.forEach(id => this.destroy(id));
    }

    /**
     * Получает экземпляр ViewBox по id, либо вызывает callback, когда он будет готов
     * @param {string} id - идентификатор ViewBox
     * @param {function} callback - функция, принимающая экземпляр ViewBox
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
     * Открывает ссылку в указанном ViewBox.
     * @param {string} viewBoxId - Идентификатор ViewBox.
     * @param {string} href - URL для открытия.
     */
    static sendRequest(viewBoxId, href) {
        ViewBoxRegistry.getOrOnReady(viewBoxId, function (viewBox) {
            viewBox.sendRequest(href);
        });
    }
}