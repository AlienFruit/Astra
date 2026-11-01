using Microsoft.AspNetCore.Html;
using System.Reflection;

namespace AlienFruit.Astra.Abstractions
{
    public interface IHtmlResourceRenderer
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="path"></param>
        /// <param name="resourceLocation"></param>
        /// <param name="assembly"></param>
        void AddResource(string name, string path, Assembly? assembly = null);

        // void AddJsCodeFromTemplate<T>(
        //     string resourceName,
        //     string templatePath,
        //     T specification,
        //     AstraResourceLocation resourceLocation = AstraResourceLocation.Header, Assembly? assembly = null)
        //     where T : class;

        IHtmlContent RenderHeaders();

        /// <summary>
        /// Inlines a Body-registered resource (see <see cref="AddJsCodeFromTemplate{T}"/> /
        /// <see cref="AddScriptResource"/> with <see cref="AstraResourceLocation.Body"/>).
        /// Scope: startup body content (e.g. load-check) and user-registered Body resources.
        /// Tag helpers render their per-instance init scripts through
        /// <see cref="IInitScriptRenderer"/> instead — nothing is registered or looked up here.
        /// </summary>
        /// <param name="name">Registered resource name</param>
        /// <exception cref="ArgumentException">Thrown when the name is not registered as a Body resource.</exception>
        //IHtmlContent RenderBodyResource(string name);
    }
}
