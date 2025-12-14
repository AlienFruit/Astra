namespace AlienFruit.Astra.ViewBoxLinkGroup
{
    /// <summary>
    /// Specification class that contains configuration data for initializing a view-box-link-group element.
    /// Used to pass parameters from the server-side tag helper to the client-side JavaScript initialization script.
    /// </summary>
    public class ViewBoxLinkGroupSpecification
    {
        /// <summary>
        /// Gets the path to the JavaScript template file used for initializing view-box-link-group elements.
        /// </summary>
        public const string InitScriptTemplatePath = "AlienFruit.Astra.ViewBoxLinkGroup.Resources.viewboxlink-group-init-script.template.js";

        /// <summary>
        /// Gets or sets the unique identifier for the view-box-link-group element.
        /// </summary>
        public required string Id { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the view-box container associated with this group.
        /// </summary>
        public required string ViewBoxId { get; set; }

        /// <summary>
        /// Gets or sets the CSS classes applied when at least one of the monitored URIs is active.
        /// </summary>
        public string? SelectedClassName { get; set; }

        /// <summary>
        /// Gets or sets the CSS classes applied by default to the group element when no monitored URIs are active.
        /// </summary>
        public string? DefaultClassName { get; set; }

        /// <summary>
        /// Gets or sets the array of URIs that will activate the selected state for this group.
        /// </summary>
        public required string[] UriToActivete { get; set; }
    }
}