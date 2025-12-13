using AlienFruit.Astra.Abstractions;
using AlienFruit.Astra.Core;
using AlienFruit.Astra.ViewBoxLinkGroup;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace AlienFruit.Astra.ViewBoxLink
{
    /// <summary>
    /// Tag helper for creating navigation links that load content into view-box containers without full page refreshes.
    /// Provides automatic active state management and AJAX-based navigation.
    /// </summary>
    [HtmlTargetElement("view-box-link")]
    public class ViewBoxLink(IHtmlResourceRenderer htmlResourceRenderer, IHttpContextAccessor httpContextAccessor) : TagHelper
    {
        /// <summary>
        /// Gets or sets the unique identifier for this view-box-link element.
        /// Used for JavaScript interaction and specification lookup.
        /// </summary>
        public required string Id { get; set; }

        /// <summary>
        /// Gets or sets the URI that this link navigates to when clicked.
        /// </summary>
        public required string Uri { get; set; }

        /// <summary>
        /// Gets or sets the ID of the view-box container where content should be loaded.
        /// </summary>
        public required string ViewBoxId { get; set; }

        /// <summary>
        /// Gets or sets the inline CSS styles for the link element.
        /// </summary>
        public string? Style { get; set; }

        /// <summary>
        /// Gets or sets the CSS classes applied by default to the link element.
        /// </summary>
        public string? DefaultClassName { get; set; }

        /// <summary>
        /// Gets or sets the CSS classes applied when the link corresponds to the current URL.
        /// </summary>
        public string? SelectedClassName { get; set; }

        /// <summary>
        /// Gets or sets the HTML tag name to use for the link element. Defaults to "a".
        /// </summary>
        public string? TagName { get; set; }

        /// <summary>
        /// Gets or sets the name of the JavaScript function to call when the link is clicked.
        /// </summary>
        public string? OnClickJsFunction { get; set; }

        /// <summary>
        /// Gets or sets whether the page should scroll to the top after navigation.
        /// Default value is true.
        /// </summary>
        public bool ScrollUp { get; set; } = true;

        /// <summary>
        /// Processes the view-box-link tag helper and generates the necessary HTML attributes and classes for navigation.
        /// Sets up the link element with proper attributes, CSS classes, and event handlers.
        /// </summary>
        /// <param name="context">The context object containing information associated with the current tag helper.</param>
        /// <param name="output">The output object that will have its TagName, Attributes, and child content set.</param>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (string.IsNullOrWhiteSpace(Id))
            {
                throw new ArgumentException("Id attribute is required");
            }
            output.Attributes.Add("id", Id);

            if (string.IsNullOrWhiteSpace(ViewBoxId))
            {
                throw new ArgumentException("ViewBoxId attribute is required");
            }

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
