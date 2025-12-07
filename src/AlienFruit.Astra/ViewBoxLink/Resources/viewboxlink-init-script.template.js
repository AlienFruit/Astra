ViewBoxRegistry.getOrOnReady('{{ ViewBoxId }}', function(viewBox) {
    viewBox.registerUri('{{ Uri }}', '{{ Id }}', '{{ SelectedClassName }}', '{{ DefaultClassName }}', {{ ScrollUp }});
});

{{ if OnClick && OnClick != "" }}
document.getElementById('{{ Id }}')
    .addEventListener('click', function (event) {
        {{ OnClick }}
    });
{{ end }}