using Microsoft.AspNetCore.Html;
using System.Reflection;

namespace AlienFruit.Astra.Abstractions
{
    /// <summary>
    /// Startup-asset registry: registered CSS/JS resources are emitted by
    /// <see cref="RenderHeaders"/> as URL-served <c>link</c> / <c>script</c> tags
    /// and delivered from the Astra resources endpoint.
    /// </summary>
    public interface IHtmlResourceRenderer
    {
        /// <summary>
        /// Registers a startup asset (embedded CSS/JS resource) under a unique name.
        /// Re-registering the same name with the same path is a no-op; a conflicting
        /// registration (different path) throws.
        /// </summary>
        /// <param name="name">Unique registry key; the extension (.css or .js) selects the rendered element.</param>
        /// <param name="path">Full manifest resource name of the asset within the assembly.</param>
        /// <param name="assembly">Assembly containing the resource; defaults to the Astra assembly.</param>
        /// <exception cref="ArgumentException">Thrown for an unsupported resource extension.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the name is already registered with a different path.</exception>
        void AddResource(string name, string path, Assembly? assembly = null);

        /// <summary>
        /// Renders HTML tags for all registered resources: stylesheets first, then scripts,
        /// each URL suffixed with a cache-busting version when versioning is enabled.
        /// </summary>
        /// <returns>An <see cref="IHtmlContent"/> containing the <c>link</c> and <c>script</c> tags.</returns>
        IHtmlContent RenderHeaders();
    }
}
