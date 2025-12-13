using AlienFruit.Astra.Abstractions;
using AlienFruit.Astra.Core;
using AlienFruit.Astra.ViewBoxLink;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace AlienFruit.Astra.ViewBoxLinkGroup
{
    /// <summary>
    /// Tag helper for creating groups of view-box-link elements that change style when any of the linked elements is active.
    /// Monitors multiple view-box-link elements and applies active styling when at least one is selected.
    /// </summary>
    [HtmlTargetElement("view-box-link-group")]
    public class ViewBoxLinkGroup(IHtmlResourceRenderer htmlResourceRenderer, IHttpContextAccessor httpContextAccessor) : TagHelper
    {
        /// <summary>
        /// Gets or sets the unique identifier for this view-box-link-group element.
        /// Used for JavaScript interaction and specification lookup.
        /// </summary>
        public required string Id { get; set; }

        public required string ViewBoxId { get; set; }

        /// <summary>
        /// Gets or sets the inline CSS styles for the group element.
        /// </summary>
        public string? Style { get; set; }

        /// <summary>
        /// Gets or sets the CSS classes applied by default to the group element.
        /// </summary>
        public string? DefaultClassName { get; set; }

        /// <summary>
        /// Gets or sets the CSS classes applied when at least one of the monitored view-box-link elements is active.
        /// </summary>
        public string? SelectedClassName { get; set; }

        /// <summary>
        /// Gets or sets the HTML tag name to use for the group element. Defaults to "div".
        /// </summary>
        public string? TagName { get; set; }

        public required string[] UriToActivete { get; set; }

        /// <summary>
        /// Processes the view-box-link-group tag helper and generates the necessary HTML attributes and classes.
        /// Collects URIs from child view-box-link elements or from the ViewBoxLinkIds attribute.
        /// </summary>
        /// <param name="context">The context object containing information associated with the current tag helper.</param>
        /// <param name="output">The output object that will have its TagName, Attributes, and child content set.</param>
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
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

            if (UriToActivete == null || UriToActivete.Length == 0)
            {
                throw new ArgumentException("UriToActivete attribute must contain at least one URI");
            }

            if (string.IsNullOrWhiteSpace(Style) == false)
            {
                output.Attributes.SetAttribute("style", Style);
            }

            output.TagName = string.IsNullOrWhiteSpace(TagName) ? "div" : TagName;

            var specification = new ViewBoxLinkGroupSpecification
            {
                Id = Id,
                ViewBoxId = ViewBoxId,
                SelectedClassName = SelectedClassName,
                DefaultClassName = DefaultClassName,
                UriToActivete = UriToActivete
            };

            if (string.IsNullOrWhiteSpace(DefaultClassName) == false)
            {
                output.Attributes.SetAttribute("class", ViewBoxLinkGroup.GetClassName(httpContextAccessor.HttpContext, specification));
            }
        }

        private static string GetClassName(HttpContext? context, ViewBoxLinkGroupSpecification specification)
        {
            if (string.IsNullOrWhiteSpace(specification.SelectedClassName))
            {
                return string.IsNullOrWhiteSpace(specification.DefaultClassName) ? string.Empty : specification.DefaultClassName;
            }

            if (context?.Request?.Path == null)
            {
                throw new InvalidOperationException("HttpContext is null");
            }

            return specification.UriToActivete.Contains(context.Request.Path.ToString(), StringComparer.InvariantCultureIgnoreCase)
               ? specification.SelectedClassName ?? string.Empty
               : specification.DefaultClassName ?? string.Empty;
        }
    }
}