using AlienFruit.Astra.Abstractions;
using AlienFruit.Astra.Models;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Reflection;

namespace AlienFruit.Astra.ViewBox
{
    /// <summary>
    /// Tag helper for creating view-box containers that enable dynamic content loading without full page refreshes.
    /// The view-box acts as a container for AJAX-loaded content and provides event handling for loading states.
    /// </summary>
    [HtmlTargetElement("view-box")]
    public class ViewBox(IHtmlResourceRenderer htmlResourceRenderer, IResourceCompressor resourceCompressor) : TagHelper
    {
        private const char space = ' ';
        private static Resource defaultConnettionError = new EmbeddedResource(
            name: "ViewBoxError.html", 
            path: "AlienFruit.Astra.ViewBox.Resources.ViewBoxError.html", 
            assembly: Assembly.GetExecutingAssembly());

        /// <summary>
        /// Gets or sets the unique identifier for this view-box container.
        /// This ID is used to link view-box-link elements to this container and for JavaScript interactions.
        /// </summary>
        public required string Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the JavaScript function to call when content loading starts.
        /// </summary>
        public string? OnStartLoadingJsFunction { get; set; }

        /// <summary>
        /// Gets or sets the name of the JavaScript function to call when loading times out after start.
        /// This can be used to display loading indicators or handle timeout scenarios.
        /// </summary>
        public string? OnTimeoutAfterStartLoadingJsFunction { get; set; }

        /// <summary>
        /// Gets or sets the name of the JavaScript function to call when content loading finishes successfully.
        /// </summary>
        public string? OnFinishLoadingJsFunction { get; set; }

        /// <summary>
        /// Gets or sets the name of the JavaScript function to call after all scripts in the loaded page have executed.
        /// </summary>
        public string? OnScriptsExecutedJsFunction { get; set; }

        /// <summary>
        /// Gets or sets the delay in milliseconds before triggering the start loading event.
        /// Default value is 100 milliseconds.
        /// </summary>
        public int StartLoadingEventDelay { get; set; } = 100;

        /// <summary>
        /// Gets or sets the CSS classes to apply to the view-box container.
        /// </summary>
        public string? Class { get; set; }

        /// <summary>
        /// Gets or sets the inline CSS styles for the view-box container.
        /// </summary>
        public string? Style { get; set; }

        /// <summary>
        /// Gets or sets the ARIA role attribute for accessibility purposes.
        /// </summary>
        public string? Role { get; set; }

        /// <summary>
        /// Gets or sets the HTML tag name to use for the view-box container. Defaults to "main".
        /// </summary>
        public string? TagName { get; set; }

        /// <summary>
        /// Gets or sets whether the browser address should be updated when navigating within this view-box.
        /// Default value is true.
        /// </summary>
        public bool ChangingBrowserAddressEnable { get; set; } = true;

        /// <summary>
        /// Gets or sets the ID of the parent view-box container for creating hierarchical navigation.
        /// </summary>
        public string? ParentViewBoxId { get; set; }

        /// <summary>
        /// Gets or sets the resource containing the HTML message to display when connection errors occur.
        /// If not set, a default error message will be used.
        /// </summary>
        public Resource? ConnectionErrorMessageResource { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the view-box should lock after the first content load, preventing further navigation.
        /// </summary>
        public bool LockAfterFirstLoad { get; set; } = false;

        /// <summary>
        /// Processes the view-box tag helper and generates the necessary HTML and JavaScript for dynamic content loading.
        /// This method sets up the container element, initializes JavaScript resources, and configures event handlers.
        /// </summary>
        /// <param name="context">The context object containing information associated with the current tag helper.</param>
        /// <param name="output">The output object that will have its TagName, Attributes, and child content set.</param>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (string.IsNullOrWhiteSpace(Id))
            {
                throw new ArgumentException($"Attribute {nameof(Id)} is required");
            }
            output.Attributes.Add("id", Id);

            if (string.IsNullOrWhiteSpace(Style) == false)
            {
                output.Attributes.SetAttribute("style", Style);
            }

            if (string.IsNullOrWhiteSpace(Class) == false)
            {
                output.Attributes.SetAttribute("class", Class);
            }

            if (string.IsNullOrWhiteSpace(Role) == false)
            {
                output.Attributes.SetAttribute("role", Role);
            }

            output.TagName = string.IsNullOrWhiteSpace(TagName) ? "main" : TagName;

            var specification = new ViewBoxSpecification
            {
                Id = Id,
                StartLoadingEventDelay = StartLoadingEventDelay,
                OnFinishLoadingJsFunction = OnFinishLoadingJsFunction,
                OnStartLoadingJsFunction = OnStartLoadingJsFunction,
                OnTimeoutAfterStartLoadingJsFunction = OnTimeoutAfterStartLoadingJsFunction,
                OnScriptsExecutedJsFunction = OnScriptsExecutedJsFunction,
                ChangingBrowserAddressEnable = ChangingBrowserAddressEnable,
                ParrentViewBoxId = ParentViewBoxId,
                LockAfterFirstLoad = LockAfterFirstLoad,
                ConnectionErrorMessage = GetConnectionErrorMessage()
            };

            htmlResourceRenderer.AddJsCodeFromTemplate(
                $"viewbox-init-{specification.Id}.js",
                ViewBoxSpecification.InitScriptTemplatePath,
                specification, AstraResourceLocation.Body);

            output.PostContent.AppendHtml(htmlResourceRenderer.RenderBodyResource($"viewbox-init-{specification.Id}.js"));
        }

        private string GetConnectionErrorMessage()
        {
            var content = resourceCompressor.CompressToString(this.ConnectionErrorMessageResource ?? defaultConnettionError);
            return content.Replace('\r', space).Replace('\n', space);
        }
    }
}
