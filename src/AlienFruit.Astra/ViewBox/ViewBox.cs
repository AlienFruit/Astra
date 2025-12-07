using AlienFruit.Astra.Abstractions;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace AlienFruit.Astra.ViewBox
{
    [HtmlTargetElement("view-box")]
    public class ViewBox(IHtmlResourceRenderer htmlResourceRenderer, IResourceCompressor resourceCompressor) : TagHelper
    {
        private const char space = ' ';

        public required string Id { get; set; }
        public string? OnStartLoadingJsFunction { get; set; }
        public string? OnFinishLoadingJsFunction { get; set; }
        public string? OnScriptsExecutedJsFunction { get; set; }
        public int StartLoadingEventDelay { get; set; } = 100;
        public string? Class { get; set; }
        public string? Style { get; set; }
        public string? Role { get; set; }
        public string? TagName { get; set; }
        public bool ChangingBrowserAddressEnable { get; set; } = true;
        public string? ParentViewBoxId { get; set; }
        public required Resource ConnectionErrorMessageResource { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (string.IsNullOrWhiteSpace(Id))
            {
                throw new ArgumentException($"Attribute {nameof(Id)} is required");
            }
            output.Attributes.Add("id", Id);

            if (ConnectionErrorMessageResource == null)
            {
                throw new ArgumentException($"Attribute {nameof(ConnectionErrorMessageResource)} is required");
            }

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
                OnScriptsExecutedJsFunction = OnScriptsExecutedJsFunction,
                ChangingBrowserAddressEnable = ChangingBrowserAddressEnable,
                ParrentViewBoxId = ParentViewBoxId,
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
            var content = resourceCompressor.CompressToString(this.ConnectionErrorMessageResource);
            return content.Replace('\r', space).Replace('\n', space);
        }
    }
}
