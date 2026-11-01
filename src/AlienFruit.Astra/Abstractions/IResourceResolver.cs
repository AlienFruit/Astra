using Microsoft.AspNetCore.Html;
using System.Reflection;

namespace AlienFruit.Astra.Abstractions
{
    /// <summary>
    /// Registers styles/scripts by URI or as embedded assembly resources and renders
    /// them into HTML. The library ships no implementation and does not register it
    /// in DI; the active startup-asset API is <see cref="IHtmlResourceRenderer"/>.
    /// </summary>
    public interface IResourceResolver
    {
        /// <summary>
        /// Registers an external stylesheet by URI, emitted as a <c>link</c> tag.
        /// </summary>
        /// <param name="styleSheetUri">Absolute or relative URI of the stylesheet.</param>
        void RegisterStyle(Uri styleSheetUri);

        /// <summary>
        /// Registers an external script by URI, emitted as a <c>script src</c> tag.
        /// </summary>
        /// <param name="scriptUri">Absolute or relative URI of the script.</param>
        void RegisterScript(Uri scriptUri);

        /// <summary>
        /// Registers an inline script body under a name, emitted in the script-body section.
        /// </summary>
        /// <param name="name">Unique name of the script.</param>
        /// <param name="scriptBody">Raw JavaScript content to inline.</param>
        void RegisterScript(string name, string scriptBody);

        /// <summary>
        /// Registers an embedded assembly script resource under its manifest name.
        /// </summary>
        /// <param name="resourceScriptName">Full manifest resource name of the script.</param>
        /// <param name="assembly">Assembly containing the resource.</param>
        void RegisterResourceScript(string resourceScriptName, Assembly assembly);

        /// <summary>
        /// Registers an embedded assembly stylesheet resource under its manifest name.
        /// </summary>
        /// <param name="resourceStyleName">Full manifest resource name of the stylesheet.</param>
        /// <param name="assembly">Assembly containing the resource.</param>
        void RegisterResourceStyle(string resourceStyleName, Assembly assembly);

        /// <summary>
        /// Renders all registered resources (headers and inline script bodies).
        /// </summary>
        /// <returns>An <see cref="IHtmlContent"/> containing the rendered markup.</returns>
        IHtmlContent Render();

        /// <summary>
        /// Renders only the inline script bodies registered via
        /// <see cref="RegisterScript(string, string)"/>.
        /// </summary>
        /// <returns>An <see cref="IHtmlContent"/> containing the inline <c>script</c> tags.</returns>
        IHtmlContent RenderScriptBodies();

        /// <summary>
        /// Renders only the header tags: stylesheets and URL-referenced scripts.
        /// </summary>
        /// <returns>An <see cref="IHtmlContent"/> containing the <c>link</c> and <c>script</c> tags.</returns>
        IHtmlContent RenderHeader();
    }
}
