ViewBoxRegistry.getOrOnReady('{{ ViewBoxId }}', function(viewBox) {
    // Register URIs for monitoring
    {{ if Uris && Uris.length > 0 }}
    var groupUris = [{{#each Uris}}'{{this}}'{{#unless @last}},{{/unless}}{{/each}}];
    var groupElement = document.getElementById('{{ Id }}');

    if (groupElement) {
        // Function to update group element class based on active URIs
        function updateGroupClass() {
            var currentPath = window.location.pathname;
            var isAnyActive = groupUris.some(function(uri) {
                return currentPath.toLowerCase() === uri.toLowerCase();
            });

            {{ if SelectedClassName && SelectedClassName != "" && DefaultClassName && DefaultClassName != "" }}
            groupElement.className = isAnyActive ? '{{ SelectedClassName }}' : '{{ DefaultClassName }}';
            {{ else if SelectedClassName && SelectedClassName != "" }}
            if (isAnyActive) {
                groupElement.className = '{{ SelectedClassName }}';
            }
            {{ else if DefaultClassName && DefaultClassName != "" }}
            if (!isAnyActive) {
                groupElement.className = '{{ DefaultClassName }}';
            }
            {{ end }}
        }

        // Initial update
        updateGroupClass();

        // Override the viewBox _selectUri method to also update our group
        var originalSelectUri = viewBox._selectUri;
        viewBox._selectUri = function(uriToSelect) {
            // Call original method
            originalSelectUri.call(this, uriToSelect);

            // Update our group element
            updateGroupClass();
        };
    }
    {{ end }}
});
