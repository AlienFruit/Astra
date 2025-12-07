using AlienFruit.Astra.Abstractions;
using AlienFruit.Astra.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace AlienFruit.Astra.ViewBoxLink
{

    [HtmlTargetElement("view-box-link")]
    public class ViewBoxLink(IHtmlResourceRenderer htmlResourceRenderer, IHttpContextAccessor httpContextAccessor) : TagHelper
    {
        public required string Id { get; set; }
        public required string Uri { get; set; }
        public required string ViewBoxId { get; set; }
        public string? Style { get; set; }
        public string? DefaultClassName { get; set; }
        public string? SelectedClassName { get; set; }
        public string? TagName { get; set; }
        public string? OnClickJsFunction { get; set; }
        public bool ScrollUp { get; set; } = true;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (string.IsNullOrWhiteSpace(Id))
            {
                throw new ArgumentException("Id attribute is required");
            }
            output.Attributes.Add("id", Id);

            if (string.IsNullOrWhiteSpace(Uri))
            {
                throw new ArgumentException("Uri attribute is required");
            }
            output.Attributes.SetAttribute("href", Uri);

            if (string.IsNullOrWhiteSpace(Style) == false)
            {
                output.Attributes.SetAttribute("style", Style);
            }

            output.TagName = string.IsNullOrWhiteSpace(TagName) ? "a" : TagName;

            var specification = new ViewBoxLinkSpecification
            {
                Id = Id,
                Uri = new Uri(Uri, UriKind.RelativeOrAbsolute),
                ViewBoxId = ViewBoxId,
                SelectedClassName = SelectedClassName,
                DefaultClassName = DefaultClassName,
                OnClick = OnClickJsFunction,
                ScrollUp = ScrollUp
            };

            if (string.IsNullOrWhiteSpace(DefaultClassName) == false)
            {
                output.Attributes.SetAttribute("class", GetClassName(httpContextAccessor.HttpContext, specification));
            }

            htmlResourceRenderer.AddJsCodeFromTemplate(
                $"viewbox-link-init-{Id}", 
                ViewBoxLinkSpecification.InitScriptTemplatePath, 
                specification, AstraResourceLocation.Body);

            output.PostContent.AppendHtml(htmlResourceRenderer.RenderBodyResource($"viewbox-link-init-{Id}"));
        }

        private string GetClassName(HttpContext? context, ViewBoxLinkSpecification specification)
        {
            if (string.IsNullOrWhiteSpace(specification.SelectedClassName))
            { 
                return string.IsNullOrWhiteSpace(specification.DefaultClassName) ? string.Empty : specification.DefaultClassName;
            }

            if (context?.Request?.Path == null)
            {
                throw new InvalidOperationException("HttpContext is null");
            }
            
            return context.Request.Path.ToString()
               .Equals(specification.Uri.ToString(), StringComparison.InvariantCultureIgnoreCase)
               ? specification.SelectedClassName ?? string.Empty
               : specification.DefaultClassName ?? string.Empty;
        }
    }
}
