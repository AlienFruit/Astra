using AlienFruit.Astra.Abstractions;
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
        private readonly ConcurrentDictionary<string, string> _resourceCache = new();
        private readonly ConcurrentDictionary<string, Template> _templateCache = new();

        public string Render<T>(T specification) where T : ISpecification
        {
            var template = GetTemplate(specification.InitScriptTemplatePath);
            var result = template.Render(specification, x => x.Name)
                ?? throw new InvalidOperationException("Result script cannot be null");
            return result;
        }

        public string GetResource(string path)
        {
            return _resourceCache.GetOrAdd(path, static p =>
            {
                var assembly = Assembly.GetExecutingAssembly();
                using var stream = assembly.GetManifestResourceStream(p)
                    ?? throw new InvalidOperationException($"Resource not found: {p}");
                using var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            });
        }

        private Template GetTemplate(string path)
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