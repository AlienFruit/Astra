using Microsoft.AspNetCore.Html;
using System.Reflection;

namespace AlienFruit.Astra.Abstractions
{
    public interface IHtmlResourceRenderer
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="name">Extension can be omitted</param>
        /// <param name="path">Path to embedded resource in assembly</param>
        void AddScriptResource(string name, string path, AstraResourceLocation resourceLocation = AstraResourceLocation.Header, Assembly? assembly = null);

        /// <summary>
        ///
        /// </summary>
        /// <param name="name">Extension can be omitted</param>
        /// <param name="path">Path to embedded resource in assembly</param>
        void AddStylesheetResource(string name, string path, AstraResourceLocation resourceLocation = AstraResourceLocation.Header, Assembly? assembly = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceName">Use name with extension if compression is needed</param>
        /// <param name="jsCode">JavaScript code</param>
        void AddJsCode(string resourceName, string jsCode, AstraResourceLocation resourceLocation = AstraResourceLocation.Header, Assembly? assembly = null);

        void AddJsCodeFromTemplate<T>(
            string resourceName, 
            string templatePath, 
            T specification, 
            AstraResourceLocation resourceLocation = AstraResourceLocation.Header, Assembly? assembly = null) 
            where T : class;

        bool IsResourceExists(string name);

        IHtmlContent RenderHeaders();

        IHtmlContent RenderBodyResource(string name);

        /// <summary>
        /// Gets resource URL with versioning
        /// </summary>
        /// <param name="resourceName">Resource name</param>
        /// <returns>URL with version parameter</returns>
        string GetResourceUrl(string resourceName);
    }
}
