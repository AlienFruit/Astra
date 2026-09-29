// window history
{{ if ChangingBrowserAddressEnable }}
window.addEventListener('load', () => {
    window.addEventListener('popstate', (e) => {
        ViewBoxRegistry
            .get('{{ Id }}')
            .sendRequestAndRestoreScrollPosition(window.location.href);
    }, false);
}, false);
{{ end }}

// create view box
ViewBoxRegistry.create('{{ Id }}', {
    id: '{{ Id }}',
    changingBrowserAddressEnable: {{ ChangingBrowserAddressEnable }},
    startLoadingEventDelay: {{ StartLoadingEventDelay }},
    connectionErrorMessage: '{{ ConnectionErrorMessage }}'
});

// cleanup resources, if viewbox was created inside another viewbox
{{ if ParrentViewBoxId && ParrentViewBoxId != "" }}
ViewBoxRegistry.getOrOnReady('{{ ParrentViewBoxId }}', function(parentViewBox) {
    parentViewBox.subscribeOnStartLoading(() => {
        ViewBoxRegistry.destroy('{{ Id }}');
    });
});
{{ end }}

{{ if OnStartLoadingJsFunction && OnStartLoadingJsFunction != "" }}
ViewBoxRegistry
    .get('{{ Id }}')
    .subscribeOnStartLoading({{ OnStartLoadingJsFunction }});
{{ end }}

{{ if OnTimeoutAfterStartLoadingJsFunction && OnTimeoutAfterStartLoadingJsFunction != "" }}
ViewBoxRegistry
    .get('{{ Id }}')
    .subscribeOnTimeoutAfterStartLoading({{ OnTimeoutAfterStartLoadingJsFunction }});
{{ end }}

{{ if OnFinishLoadingJsFunction && OnFinishLoadingJsFunction != "" }}
ViewBoxRegistry
    .get('{{ Id }}')
    .subscribeOnFinishLoading({{ OnFinishLoadingJsFunction }});
{{ end }}

{{ if OnScriptsExecutedJsFunction && OnScriptsExecutedJsFunction != "" }}
ViewBoxRegistry.get('{{ Id }}').subscribeOnScriptsExecuted({{ OnScriptsExecutedJsFunction }});
{{ end }}

{{ if LockAfterFirstLoad }}
ViewBoxRegistry.getOrOnReady('{{ Id }}', x => x.subscribeOnFinishLoading(() => x.lock()));
{{ end }}
