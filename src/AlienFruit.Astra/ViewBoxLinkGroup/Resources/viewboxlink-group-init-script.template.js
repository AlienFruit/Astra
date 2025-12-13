ViewBoxRegistry.getOrOnReady('{{ ViewBoxId }}', function(viewBox) {
    var uriListToActivate = [{{ for uri in UriToActivete }}'{{ uri }}'{{ if !for.last }}, {{ end }}{{ end }}];
    viewBox.registerUriGroup('{{ Id }}', '{{ SelectedClassName }}', '{{ DefaultClassName }}', uriListToActivate);
});