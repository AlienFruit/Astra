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

        IHtmlContent RenderHeaders();
    }
}
