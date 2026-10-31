using AlienFruit.Astra.Abstractions;
using AlienFruit.Astra.ViewBox;
using AlienFruit.Astra.ViewBoxLink;
using AlienFruit.Astra.ViewBoxLinkGroup;
using Scriban;
using System.Collections.Concurrent;
using System.Reflection;

namespace AlienFruit.Astra.Core
{
    /// <summary>
    /// Stateless renderer for per-instance initialization scripts.
    /// Does NOT register anything in the singleton resource registry or ResourceStorage.
    /// Templates are cached after first parse.
    /// </summary>
    internal sealed class InitScriptRenderer : IInitScriptRenderer
    {
        private readonly ConcurrentDictionary<string, Template> _templateCache = new();

        public string Render<T>(T specification) where T : ISpecification
        {
            var templatePath = specification.InitScriptTemplatePath;
            var template = GetOrParseTemplate(templatePath);
            var result = template.Render(specification, x => x.Name)
                ?? throw new InvalidOperationException("Result script cannot be null");
            return result;
        }


        private Template GetOrParseTemplate(string path)
        {
            return _templateCache.GetOrAdd(path, static p =>
            {
                var assembly = Assembly.GetExecutingAssembly();
                using var stream = assembly.GetManifestResourceStream(p)
                    ?? throw new InvalidOperationException($"Template not found: {p}");
                using var reader = new StreamReader(stream);
                var raw = reader.ReadToEnd();
                return Template.Parse(raw);
            });
        }
    }
}