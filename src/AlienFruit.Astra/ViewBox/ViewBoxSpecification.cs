using AlienFruit.Astra.Abstractions;

namespace AlienFruit.Astra.ViewBox
{
    /// <summary>
    /// Configuration of a view-box instance, passed to <see cref="IInitScriptRenderer"/>
    /// and rendered into the client-side init script.
    /// </summary>
    public class ViewBoxSpecification : ISpecification
    {
        /// <summary>
        /// Manifest resource path of the client-side viewbox script, registered as a startup asset.
        /// </summary>
        public const string JsResourcePath = "AlienFruit.Astra.ViewBox.Resources.viewbox.js";
        private const string initScriptTemplatePath = "AlienFruit.Astra.ViewBox.Resources.viewbox-init-script.template.js";

        /// <summary>
        /// Gets or sets the unique identifier of the view-box container.
        /// </summary>
        public required string Id { get; set; }

        /// <summary>
        /// Gets or sets the JavaScript function called when content loading starts.
        /// </summary>
        public string? OnStartLoadingJsFunction { get; set; }

        /// <summary>
        /// Gets or sets the JavaScript function called when loading times out after start.
        /// </summary>
        public string? OnTimeoutAfterStartLoadingJsFunction { get; set; }

        /// <summary>
        /// Gets or sets the JavaScript function called when content loading finishes.
        /// </summary>
        public string? OnFinishLoadingJsFunction { get; set; }

        /// <summary>
        /// Gets or sets the JavaScript function called after all scripts in the loaded content have executed.
        /// </summary>
        public string? OnScriptsExecutedJsFunction { get; set; }

        /// <summary>
        /// Gets or sets the delay in milliseconds before the start-loading event fires. Default is 100.
        /// </summary>
        public int StartLoadingEventDelay { get; set; } = 100;

        /// <summary>
        /// Gets or sets whether the browser address is updated on navigation within this view-box. Default is true.
        /// </summary>
        public bool ChangingBrowserAddressEnable { get; set; } = true;

        /// <summary>
        /// Gets or sets the identifier of the parent view-box for hierarchical navigation.
        /// </summary>
        public string? ParrentViewBoxId { get; set; }

        /// <summary>
        /// Gets or sets the message displayed when the connection fails (normalized single-line HTML).
        /// </summary>
        public required string ConnectionErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets whether the view-box locks after the first successful load. Default is false.
        /// </summary>
        public bool LockAfterFirstLoad { get; set; } = false;

        /// <summary>
        /// Gets the path to the init script template for this specification.
        /// </summary>
        public string InitScriptTemplatePath => initScriptTemplatePath;
    }
}
