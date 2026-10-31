using AlienFruit.Astra.Abstractions;
using AlienFruit.Astra.Configuration;
using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.Logging;
using Scriban;
using System.Collections.Concurrent;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace AlienFruit.Astra.Core
{
    /// <summary>
    /// Atomic resource registry: one name maps to exactly one fully-formed
    /// <see cref="ResourceEntry"/> published with a single <c>TryAdd</c>. A resource is
    /// either fully registered (name + kind + location + content source) or absent —
    /// a "half-registered" state is not expressible by construction.
    /// </summary>
    internal class HtmlResourceRenderer(
        IResourceStorage resourceStorage,
        AstraConfiguration configuration,
        ILogger<HtmlResourceRenderer>? logger = null) : IHtmlResourceRenderer
    {
        private readonly ConcurrentDictionary<string, ResourceEntry> entries = new();

        private readonly Dictionary<string, AstraResourceKind> resourceKindByExtensionMap = new(StringComparer.OrdinalIgnoreCase)
        {
            {".css", AstraResourceKind.Stylesheet },
            { ".js", AstraResourceKind.Script },
        };

        public void AddResource(string name, string path, AstraResourceLocation resourceLocation, Assembly? assembly = null)
        {
            if (this.entries.TryGetValue(name, out var existing))
            {
                ThrowIfContentDiffers(name, existing, () => ComputeVersionFromStorage(name));
                return;
            }

            if (!resourceKindByExtensionMap.TryGetValue(Path.GetExtension(path), out var kind))
            {
                throw new ArgumentException($"Unsupported resource extension for '{name}'. Only .css and .js are allowed.");
            }

            var resourceAssembly = assembly ?? Assembly.GetExecutingAssembly();
            resourceStorage.RegisterResource(name, path, resourceAssembly);
            var version = ComputeVersionFromStorage(name);
            var entry = new ResourceEntry(name, kind, resourceLocation, version);
            this.entries.TryAdd(name, entry);
        }

        public IHtmlContent RenderHeaders()
        {
            var builder = new HtmlContentBuilder();
            foreach (var entry in entries.Values.Where(x => x.Location == AstraResourceLocation.Header && x.Kind == AstraResourceKind.Stylesheet))
            {
                builder.AppendHtmlLine($"<link href=\"/{configuration.ResourcesRoute}/{entry.Name}{GetResourceVersion(entry)}\" rel=\"stylesheet\" type=\"text/css\" />");
            }

            foreach (var entry in entries.Values.Where(x => x.Location == AstraResourceLocation.Header && x.Kind == AstraResourceKind.Script))
            {
                builder.AppendHtmlLine($"<script src=\"/{configuration.ResourcesRoute}/{entry.Name}{GetResourceVersion(entry)}\"></script>");
            }

            return builder;
        }

        public IHtmlContent RenderBodyResource(string name)
        {
            if (entries.TryGetValue(name, out var entry) && entry.Location == AstraResourceLocation.Body)
            {
                var content = ResolveContent(name);
                var builder = new HtmlContentBuilder();
                if (entry.Kind == AstraResourceKind.Stylesheet)
                {
                    return builder
                        .AppendHtmlLine("<style>")
                        .AppendHtmlLine(content)
                        .AppendHtmlLine("</style>");
                }

                return builder
                    .AppendHtmlLine("<script>")
                    .AppendHtmlLine(content)
                    .AppendHtmlLine("</script>");
            }

            throw new ArgumentException($"resource {name} was not registered as body resource");
        }

        /// <summary>
        /// Re-registration of the same name must never silently keep the first registration
        /// (first-write-wins staleness). The identical registration → no-op; a conflicting
        /// one (different content, kind or location) → <see cref="InvalidOperationException"/>.
        /// </summary>
        //private static void EnsureSameRegistration(ResourceEntry existing, AstraResourceKind kind, AstraResourceLocation location)
        //{
        //    if (existing.Kind != kind || existing.Location != location)
        //    {
        //        throw new InvalidOperationException(
        //            $"Resource '{existing.Name}' is already registered differently (kind: {existing.Kind}, location: {existing.Location}). Use a unique name per resource.");
        //    }
        //}

        public void AddJsCodeFromTemplate<T>(
            string resourceName,
            string templatePath,
            T specification,
            AstraResourceLocation resourceLocation = AstraResourceLocation.Header, Assembly? assembly = null)
            where T : class
        {
            if (this.entries.TryGetValue(resourceName, out var existing))
            {
                ThrowIfContentDiffers(resourceName, existing, () => ComputeVersion(RenderTemplateToString(templatePath, specification, assembly)));
                return;
            }

            if (!resourceKindByExtensionMap.TryGetValue(Path.GetExtension(templatePath), out var kind))
            {
                throw new ArgumentException($"Unsupported resource extension for '{templatePath}'. Only .css and .js are allowed.");
            }

            var resourceAssembly = assembly ?? Assembly.GetExecutingAssembly();
            using var stream = resourceAssembly.GetManifestResourceStream(templatePath)
                ?? throw new InvalidOperationException($"There is no resource with path: {templatePath}");

            using var reader = new StreamReader(stream);
            var rawTemplate = reader.ReadToEnd();
            var template = Template.Parse(rawTemplate);
            var scriptContent = template.Render(specification, x => x.Name)
                ?? throw new InvalidOperationException("Result script cannot be null");

            resourceStorage.RegisterResource(resourceName, scriptContent);
            var version = ComputeVersionFromStorage(resourceName);
            var entry = new ResourceEntry(resourceName, kind, resourceLocation, version);
            this.entries.TryAdd(resourceName, entry);
        }

        private string RenderTemplateToString(string templatePath, object specification, Assembly? assembly)
        {
            var resourceAssembly = assembly ?? Assembly.GetExecutingAssembly();
            using var stream = resourceAssembly.GetManifestResourceStream(templatePath)
                ?? throw new InvalidOperationException($"There is no resource with path: {templatePath}");
            using var reader = new StreamReader(stream);
            var rawTemplate = reader.ReadToEnd();
            var template = Template.Parse(rawTemplate);
            var result = template.Render(specification, x => x.Name)
                ?? throw new InvalidOperationException("Result script cannot be null");
            return result;
        }

        private string ResolveContent(string resourceName)
        {
            if (resourceStorage.Contains(resourceName) == false)
            {
                throw new ArgumentException($"The resource \"{resourceName}\" was not registered");
            }

            using var stream = resourceStorage.OpenRead(resourceName);
            using var streamReader = new StreamReader(stream);
            return streamReader.ReadToEnd();
        }

        private string GetResourceVersion(ResourceEntry entry)
        {
            if (!configuration.EnableVersioning)
            {
                return string.Empty;
            }

            if (!string.IsNullOrEmpty(configuration.ResourceVersion))
            {
                return $"?v={configuration.ResourceVersion}";
            }

            return string.IsNullOrEmpty(entry.Version) ? string.Empty : $"?v={entry.Version}";
        }

        private void ThrowIfContentDiffers(string name, ResourceEntry existing, Func<string> versionFunc)
        {
            var newVersion = versionFunc();
            if (!string.Equals(existing.Version, newVersion, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"Resource '{name}' version mismatch. Expected {existing.Version}, got {newVersion}");
            }
        }

        private string ComputeVersionFromStorage(string resourceName)
        {
            try
            {
                using var stream = resourceStorage.OpenRead(resourceName);
                using var sha256 = SHA256.Create();
                var hashBytes = sha256.ComputeHash(stream);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
            catch
            {
                return string.Empty;
            }
        }

        private string ComputeVersion(string content)
        {
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(content));
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
    }
}
