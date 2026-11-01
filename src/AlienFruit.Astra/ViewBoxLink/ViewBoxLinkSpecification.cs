using AlienFruit.Astra.Abstractions;

namespace AlienFruit.Astra.ViewBoxLink
{
    /// <summary>
    /// Configuration of a view-box-link instance, passed to <see cref="IInitScriptRenderer"/>
    /// and rendered into the client-side init script.
    /// </summary>
    public class ViewBoxLinkSpecification : ISpecification
    {
        private const string initScriptTemplatePath = "AlienFruit.Astra.ViewBoxLink.Resources.viewboxlink-init-script.template.js";

        /// <summary>
        /// Gets or sets the unique identifier of the link element.
        /// </summary>
        public required string Id { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the target view-box container.
        /// </summary>
        public required string ViewBoxId { get; set; }

        /// <summary>
        /// Gets or sets the navigation target URI loaded into the view-box on click.
        /// </summary>
        public required Uri Uri { get; set; }

        /// <summary>
        /// Gets or sets the CSS classes applied when the link matches the current URL.
        /// </summary>
        public string? SelectedClassName { get; set; }

        /// <summary>
        /// Gets or sets the CSS classes applied to the link by default.
        /// </summary>
        public string? DefaultClassName { get; set; }

        /// <summary>
        /// Gets or sets the JavaScript function called when the link is clicked.
        /// </summary>
        public string? OnClick { get; set; }

        /// <summary>
        /// Gets or sets whether the page scrolls to the top after navigation. Default is true.
        /// </summary>
        public bool ScrollUp { get; set; } = true;

        /// <summary>
        /// Gets the path to the init script template for this specification.
        /// </summary>
        public string InitScriptTemplatePath => initScriptTemplatePath;
    }
}
