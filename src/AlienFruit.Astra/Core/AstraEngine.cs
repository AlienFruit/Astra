using AlienFruit.Astra.Abstractions;
using AlienFruit.Astra.Extensions;
using AlienFruit.Astra.ViewBox;
using AlienFruit.Astra.ViewBoxLink;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using System.Collections.Concurrent;

namespace AlienFruit.Astra.Core
{
    public class AstraEngine
    {
        private readonly IHtmlResourceRenderer htmlResourceRenderer;
        private readonly ConcurrentDictionary<string, ViewBoxLinkSpecification> linkSpecifications = new();
        private const string loadCheckScriptPath = "AlienFruit.Astra.Core.Resources.load-check.js";

        public AstraEngine(IHtmlResourceRenderer htmlResourceRenderer)
        {
            this.htmlResourceRenderer = htmlResourceRenderer;
            this.htmlResourceRenderer.AddScriptResource("load-check.js", loadCheckScriptPath, AstraResourceLocation.Body);
        }

        public AstraEngine AddViewBox(ViewBoxSpecification specification)
        {
            htmlResourceRenderer.AddJsCodeFromTemplate(
                $"viewbox-init-{specification.Id}.js",
                ViewBoxSpecification.InitScriptTemplatePath,
                specification);
            return this;
        }

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

        public IHtmlContent RenderHeaders()
        {
            htmlResourceRenderer.AddScriptResource("viewbox.js", ViewBoxSpecification.JsResourcePath);
           

            var result = new HtmlContentBuilder();
            result.AppendHtml(htmlResourceRenderer.RenderHeaders());
            result.AppendHtmlLine("<meta id=\"load-check\">");
            return result;
        }

        public IHtmlContent LoadCheck() => htmlResourceRenderer.RenderBodyResource("load-check.js");

        public string? RouteLayout(HttpContext context, string defaultLayout)
        {
            return context.Request.IsAjaxRequest() ? null : defaultLayout;
        }

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
