using AlienFruit.Astra.Abstractions;
using AlienFruit.Astra.Core;
using AlienFruit.Astra.ViewBoxLink;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace AlienFruit.Astra.ViewBoxLinkGroup
{
    /// <summary>
    /// Information about a ViewBoxLink element used for communication between ViewBoxLink and ViewBoxLinkGroup.
    /// </summary>
    internal class ViewBoxLinkInfo
    {
        public required string Id { get; set; }
        public required Uri Uri { get; set; }
        public required string ViewBoxId { get; set; }
    }

    /// <summary>
    /// Extension methods for TagHelperContext to support communication between ViewBoxLink and ViewBoxLinkGroup.
    /// </summary>
    internal static class TagHelperContextExtensions
    {
        private const string ViewBoxLinkGroupItemsKey = "ViewBoxLinkGroup_Items";
        private const string CurrentViewBoxLinkGroupKey = "CurrentViewBoxLinkGroup_Id";

        /// <summary>
        /// Sets the current ViewBoxLinkGroup ID in the TagHelperContext.
        /// This allows child ViewBoxLink elements to know which group they belong to.
        /// </summary>
        public static void SetCurrentViewBoxLinkGroup(this TagHelperContext context, string groupId)
        {
            context.Items[CurrentViewBoxLinkGroupKey] = groupId;
        }

        /// <summary>
        /// Gets the current ViewBoxLinkGroup ID from the TagHelperContext.
        /// </summary>
        public static string? GetCurrentViewBoxLinkGroup(this TagHelperContext context)
        {
            return context.Items.TryGetValue(CurrentViewBoxLinkGroupKey, out var groupId) ? groupId as string : null;
        }

        /// <summary>
        /// Registers a ViewBoxLink with the current group in the TagHelperContext.
        /// </summary>
        public static void RegisterViewBoxLink(this TagHelperContext context, ViewBoxLinkInfo info)
        {
            var groupId = context.GetCurrentViewBoxLinkGroup();
            if (string.IsNullOrEmpty(groupId))
            {
                return; // Not inside a ViewBoxLinkGroup
            }

            if (!context.Items.ContainsKey(ViewBoxLinkGroupItemsKey))
            {
                context.Items[ViewBoxLinkGroupItemsKey] = new Dictionary<string, List<ViewBoxLinkInfo>>();
            }

            var groupItems = (Dictionary<string, List<ViewBoxLinkInfo>>)context.Items[ViewBoxLinkGroupItemsKey]!;

            if (!groupItems.TryGetValue(groupId, out var groupLinks))
            {
                groupLinks = new List<ViewBoxLinkInfo>();
                groupItems[groupId] = groupLinks;
            }

            groupLinks.Add(info);
        }

        /// <summary>
        /// Gets all registered ViewBoxLink info for a specific group ID from the TagHelperContext.
        /// </summary>
        public static List<ViewBoxLinkInfo> GetChildViewBoxLinkInfos(this TagHelperContext context, string groupId)
        {
            if (context.Items.TryGetValue(ViewBoxLinkGroupItemsKey, out var groupItemsObj) &&
                groupItemsObj is Dictionary<string, List<ViewBoxLinkInfo>> groupItems &&
                groupItems.TryGetValue(groupId, out var groupLinks))
            {
                return groupLinks;
            }

            return new List<ViewBoxLinkInfo>();
        }
    }
}

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


        /// <summary>
        /// Processes the view-box-link-group tag helper and generates the necessary HTML attributes and classes.
        /// Collects URIs from child view-box-link elements through TagHelperContext.Items.
        /// </summary>
        /// <param name="context">The context object containing information associated with the current tag helper.</param>
        /// <param name="output">The output object that will have its TagName, Attributes, and child content set.</param>
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (string.IsNullOrWhiteSpace(Id))
            {
                throw new ArgumentException("Id attribute is required");
            }

            // Set this group as current for child ViewBoxLink elements
            context.SetCurrentViewBoxLinkGroup(Id);

            output.Attributes.Add("id", Id);

            // Collect URIs from child ViewBoxLink elements registered in context
            var childLinkInfos = context.Items.GetChildViewBoxLinkInfos(Id);
            var uris = childLinkInfos.Select(x => x.Uri).ToList();

            if (!uris.Any())
            {
                throw new ArgumentException("No URIs found. Include view-box-link elements as children.");
            }

            // Determine ViewBoxId from child links
            var actualViewBoxId = childLinkInfos.FirstOrDefault()?.ViewBoxId;
            if (string.IsNullOrWhiteSpace(actualViewBoxId))
            {
                throw new ArgumentException("ViewBoxId could not be determined. Ensure child ViewBoxLink elements are properly configured.");
            }

            if (!string.IsNullOrWhiteSpace(Style))
            {
                output.Attributes.SetAttribute("style", Style);
            }

            output.TagName = string.IsNullOrWhiteSpace(TagName) ? "div" : TagName;

            var specification = new ViewBoxLinkGroupSpecification
            {
                Id = Id,
                ViewBoxId = actualViewBoxId,
                Uris = uris,
                SelectedClassName = SelectedClassName,
                DefaultClassName = DefaultClassName,
                TagName = TagName
            };

            if (!string.IsNullOrWhiteSpace(DefaultClassName))
            {
                output.Attributes.SetAttribute("class", GetClassName(httpContextAccessor.HttpContext, specification));
            }

            htmlResourceRenderer.AddJsCodeFromTemplate(
                $"viewbox-link-group-init-{Id}",
                ViewBoxLinkGroupSpecification.InitScriptTemplatePath,
                specification, AstraResourceLocation.Body);

            output.PostContent.AppendHtml(htmlResourceRenderer.RenderBodyResource($"viewbox-link-group-init-{Id}"));
        }


        /// <summary>
        /// Determines the CSS class name based on whether any of the monitored URIs match the current request path.
        /// </summary>
        private string GetClassName(HttpContext? context, ViewBoxLinkGroupSpecification specification)
        {
            if (string.IsNullOrWhiteSpace(specification.SelectedClassName))
            {
                return string.IsNullOrWhiteSpace(specification.DefaultClassName) ? string.Empty : specification.DefaultClassName;
            }

            if (context?.Request?.Path == null)
            {
                throw new InvalidOperationException("HttpContext is null");
            }

            var currentPath = context.Request.Path.ToString();
            var isAnyUriActive = specification.Uris.Any(uri =>
                currentPath.Equals(uri.ToString(), StringComparison.InvariantCultureIgnoreCase));

            return isAnyUriActive
                ? specification.SelectedClassName ?? string.Empty
                : specification.DefaultClassName ?? string.Empty;
        }
    }
}
