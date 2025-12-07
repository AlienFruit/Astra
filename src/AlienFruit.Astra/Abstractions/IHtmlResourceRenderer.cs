using Microsoft.AspNetCore.Html;
using System.Reflection;

namespace AlienFruit.Astra.Abstractions
{
    public interface IHtmlResourceRenderer
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">Можно без расширения</param>
        /// <param name="path">Путь к втроенному в сборку ресурсу</param>
        void AddScriptResource(string name, string path, AstraResourceLocation resourceLocation = AstraResourceLocation.Header, Assembly? assembly = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">Можно без расширения</param>
        /// <param name="path">Путь к втроенному в сборку ресурсу</param>
        void AddStylesheetResource(string name, string path, AstraResourceLocation resourceLocation = AstraResourceLocation.Header, Assembly? assembly = null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceName">Использовать имя с расширением, если нужна компрессия</param>
        /// <param name="jsCode">Java script код</param>
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
        /// Получает URL ресурса с версионированием
        /// </summary>
        /// <param name="resourceName">Имя ресурса</param>
        /// <returns>URL с параметром версии</returns>
        string GetResourceUrl(string resourceName);
    }
}
