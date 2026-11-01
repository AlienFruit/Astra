using AlienFruit.Astra.Abstractions;
using AlienFruit.Astra.Extensions;
using AlienFruit.Astra.ViewBox;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;

namespace AlienFruit.Astra.Core
{
    /// <summary>
    /// The main entry point of AlienFruit.Astra: renders HTML headers with startup assets
    /// (<c>viewbox.js</c>, stylesheets, scripts) and provides layout routing for AJAX requests.
    /// View-boxes and view-box-links are configured exclusively through the
    /// <c>&lt;view-box&gt;</c>, <c>&lt;view-box-link&gt;</c> and <c>&lt;view-box-link-group&gt;</c>
    /// tag helpers, which render their init scripts inline per instance.
    /// </summary>
    public class AstraEngine
    {
        private readonly IHtmlResourceRenderer htmlResourceRenderer;
        private readonly IInitScriptRenderer initScriptRenderer;
        private const string IncompleteLoadCheckScriptPath = "AlienFruit.Astra.Core.Resources.load-check.js";

        /// <summary>
        /// Initializes a new instance of the <see cref="AstraEngine"/> class.
        /// </summary>
        /// <param name="htmlResourceRenderer">The HTML resource renderer used for managing scripts, stylesheets, and other resources.</param>
        public AstraEngine(IHtmlResourceRenderer htmlResourceRenderer, IInitScriptRenderer initScriptRenderer)
        {
            this.htmlResourceRenderer = htmlResourceRenderer;
            this.initScriptRenderer = initScriptRenderer;
            this.htmlResourceRenderer.AddResource("viewbox.js", ViewBoxSpecification.JsResourcePath);
        }

        /// <summary>
        /// Renders all necessary HTML headers including scripts, stylesheets, and meta tags required for Astra functionality.
        /// This should be called in the HTML head section of the layout.
        /// </summary>
        /// <returns>An <see cref="IHtmlContent"/> containing all the rendered headers.</returns>
        public IHtmlContent RenderHeaders()
        {
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
        public IHtmlContent IncompleteLoadCheck()
        {
            var scriptContent = this.initScriptRenderer.GetResource(IncompleteLoadCheckScriptPath);
            return new HtmlContentBuilder()
                .AppendHtmlLine("<script>")
                .AppendHtmlLine(scriptContent)
                .AppendHtmlLine("</script>");
        }
        //this.initScriptRenderer.GetResource(IncompleteLoadCheckScriptPath);//htmlResourceRenderer.RenderBodyResource("load-check.js");

        /// <summary>
        /// Determines the appropriate layout based on the request type.
        /// Returns null for AJAX requests (no layout) or the default layout for regular requests.
        /// </summary>
        /// <param name="context">The current HTTP context.</param>
        /// <param name="defaultLayout">The default layout path to use for non-AJAX requests.</param>
        /// <returns>The layout path or null for AJAX requests.</returns>
        public string? RouteLayout(HttpContext context, string defaultLayout)
            => context.Request.IsAjaxRequest() ? null : defaultLayout;
    }
}
