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

        public string Render<T>(T specification) where T : class
        {
            var specType = typeof(T);
            var templatePath = GetTemplatePath(specType);
            var template = GetOrParseTemplate(templatePath);
            var result = template.Render(specification, x => x.Name)
                ?? throw new InvalidOperationException("Result script cannot be null");

            return result;
        }

        private static string GetTemplatePath(Type specType)
        {
            if (specType == typeof(ViewBoxSpecification))
                return ViewBoxSpecification.InitScriptTemplatePath;

            if (specType == typeof(ViewBoxLinkSpecification))
                return ViewBoxLinkSpecification.InitScriptTemplatePath;

            if (specType == typeof(ViewBoxLinkGroupSpecification))
                return ViewBoxLinkGroupSpecification.InitScriptTemplatePath;

            throw new ArgumentException($"No init script template registered for specification type '{specType.FullName}'.");
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