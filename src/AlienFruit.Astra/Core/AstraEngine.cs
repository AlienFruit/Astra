using AlienFruit.Astra.Abstractions;
using AlienFruit.Astra.Extensions;
using AlienFruit.Astra.ViewBox;
using AlienFruit.Astra.ViewBoxLink;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using System.Collections.Concurrent;

namespace AlienFruit.Astra.Core
{
    /// <summary>
    /// The main engine for AlienFruit.Astra that manages view-boxes, view-box-links, and resource rendering.
    /// Provides methods for registering view-box specifications, rendering HTML headers, and handling layout routing.
    /// </summary>
    public class AstraEngine
    {
        private readonly IHtmlResourceRenderer htmlResourceRenderer;
        private readonly ConcurrentDictionary<string, ViewBoxLinkSpecification> linkSpecifications = new();
        private const string IncompleteLoadCheckScriptPath = "AlienFruit.Astra.Core.Resources.load-check.js";

        /// <summary>
        /// Initializes a new instance of the <see cref="AstraEngine"/> class.
        /// </summary>
        /// <param name="htmlResourceRenderer">The HTML resource renderer used for managing scripts, stylesheets, and other resources.</param>
        public AstraEngine(IHtmlResourceRenderer htmlResourceRenderer)
        {
            this.htmlResourceRenderer = htmlResourceRenderer;
            this.htmlResourceRenderer.AddScriptResource("load-check.js", IncompleteLoadCheckScriptPath, AstraResourceLocation.Body);
        }

        /// <summary>
        /// Registers a view-box specification with the engine and generates the necessary JavaScript initialization code.
        /// </summary>
        /// <param name="specification">The view-box specification containing configuration and behavior settings.</param>
        /// <returns>The current <see cref="AstraEngine"/> instance for method chaining.</returns>
        public AstraEngine AddViewBox(ViewBoxSpecification specification)
        {
            htmlResourceRenderer.AddJsCodeFromTemplate(
                $"viewbox-init-{specification.Id}.js",
                ViewBoxSpecification.InitScriptTemplatePath,
                specification);
            return this;
        }

        /// <summary>
        /// Registers a view-box-link specification with the engine and generates the necessary JavaScript initialization code.
        /// If a link with the same ID already exists, the method returns without making changes.
        /// </summary>
        /// <param name="specification">The view-box-link specification containing navigation and behavior settings.</param>
        /// <returns>The current <see cref="AstraEngine"/> instance for method chaining.</returns>
        public AstraEngine AddViewBoxLink(ViewBoxLinkSpecification specification)
        {
            if (this.linkSpecifications.TryAdd(specification.Id, specification) == false)
            {
                return this;
            }

            htmlResourceRenderer.AddJsCodeFromTemplate(
                $"viewbox-link-init-{specification.Id}.js",
                ViewBoxLinkSpecification.InitScriptTemplatePath,
                specification);

            return this;
        }

        /// <summary>
        /// Renders all necessary HTML headers including scripts, stylesheets, and meta tags required for Astra functionality.
        /// This should be called in the HTML head section of the layout.
        /// </summary>
        /// <returns>An <see cref="IHtmlContent"/> containing all the rendered headers.</returns>
        public IHtmlContent RenderHeaders()
        {
            htmlResourceRenderer.AddScriptResource("viewbox.js", ViewBoxSpecification.JsResourcePath);


            var result = new HtmlContentBuilder();
            result.AppendHtml(htmlResourceRenderer.RenderHeaders());
            result.AppendHtmlLine("<meta id=\"load-check\">");
            return result;
        }

        /// <summary>
        /// Renders the incomplete load check script that prevents displaying cached content when resources haven't loaded properly.
        /// This should be called in the Razor view to ensure proper resource loading validation.
        /// </summary>
        /// <returns>An <see cref="IHtmlContent"/> containing the load check script.</returns>
        public IHtmlContent IncompleteLoadCheck() => htmlResourceRenderer.RenderBodyResource("load-check.js");

        /// <summary>
        /// Determines the appropriate layout based on the request type.
        /// Returns null for AJAX requests (no layout) or the default layout for regular requests.
        /// </summary>
        /// <param name="context">The current HTTP context.</param>
        /// <param name="defaultLayout">The default layout path to use for non-AJAX requests.</param>
        /// <returns>The layout path or null for AJAX requests.</returns>
        public string? RouteLayout(HttpContext context, string defaultLayout)
            => context.Request.IsAjaxRequest() ? null : defaultLayout;

        /// <summary>
        /// Gets the CSS class for a view-box-link based on the current request path.
        /// Compares the link's URI with the current request path to determine if it should be marked as active.
        /// </summary>
        /// <param name="context">The current HTTP context.</param>
        /// <param name="id">The ID of the view-box-link specification.</param>
        /// <returns>The CSS class string for the link (includes active state if applicable).</returns>
        /// <exception cref="ArgumentException">Thrown when no view-box-link specification exists with the specified ID.</exception>
        public string GetViewBoxLinkClass(HttpContext context, string id)
        {
            if (linkSpecifications.TryGetValue(id, out var specification) == false)
            {
                throw new ArgumentException($"There is no NavLink with id:{id}");
            }

            return context.Request.Path.ToString()
                .Equals(specification.Uri.ToString(), StringComparison.InvariantCultureIgnoreCase)
                ? specification.SelectedClassName
                : specification.DefaultClassName;
        }
    }
}
