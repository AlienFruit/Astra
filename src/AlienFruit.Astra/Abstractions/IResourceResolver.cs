using Microsoft.AspNetCore.Html;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AlienFruit.Astra.Abstractions
{
    public interface IResourceResolver
    {
        void RegisterStyle(Uri styleSheetUri);

        void RegisterScript(Uri scriptUri);

        void RegisterScript(string name, string scriptBody);

        void RegisterResourceScript(string resourceScriptName, Assembly assembly);

        void RegisterResourceStyle(string resourceStyleName, Assembly assembly);

        IHtmlContent Render();

        IHtmlContent RenderScriptBodies();

        IHtmlContent RenderHeader();
    }
}
