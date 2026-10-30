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
    /// <see cref="IResourceStorage"/> is kept as the serving mirror for the
    /// <c>/astra/{name}</c> endpoint: Header registrations proxy into storage.
    /// </summary>
    internal class HtmlResourceRenderer(
        IResourceStorage resourceStorage,
        AstraConfiguration configuration,
        ILogger<HtmlResourceRenderer>? logger = null) : IHtmlResourceRenderer
    {
        private readonly ConcurrentDictionary<string, ResourceEntry> entries = new();

        private readonly Dictionary<string, AstraResourceKind> resourceVersionCache = new(StringComparer.OrdinalIgnoreCase)
        {
            {".css", AstraResourceKind.Stylesheet },
            { ".js", AstraResourceKind.Script },
        };

        public void AddResource(string name, string path, AstraResourceLocation resourceLocation, Assembly? assembly = null)
        {

            if (this.entries.TryGetValue(name, out var existing))
            {
                //EnsureSameRegistration(existing, kind, resourceLocation);
                return;
            }

            if (!resourceVersionCache.TryGetValue(Path.GetExtension(name), out var kind))
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
                return;
            }

            if (!resourceVersionCache.TryGetValue(Path.GetExtension(resourceName), out var kind))
            {
                throw new ArgumentException($"Unsupported resource extension for '{resourceName}'. Only .css and .js are allowed.");
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

        /// <summary>
        /// Body content resolves from the entry's own source — the same Resource the old
        /// storage round-trip built, so compressor behavior (extension-driven Nuglify)
        /// is preserved byte-for-byte.
        /// </summary>
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

            // If global version is specified, use it
            if (!string.IsNullOrEmpty(configuration.ResourceVersion))
            {
                return $"?v={configuration.ResourceVersion}";
            }

            return string.IsNullOrEmpty(entry.Version) ? string.Empty : $"?v={entry.Version}";
        }


        /// <summary>
        /// Hash source for embedded (path-registered) resources is storage.OpenRead — the
        /// compressed bytes, exactly as served by the /astra endpoint. A per-node timestamp
        /// fallback would differ across nodes and thrash caches, so on failure the resource
        /// is served WITHOUT a version parameter (logged once; Lazy caches the empty result).
        /// </summary>
        private string ComputeVersionFromStorage(string resourceName)
        {
            try
            {
                var content = ResolveContent(resourceName);
                return ComputeVersion(content);     
            }
            catch (Exception ex)
            {
                logger?.LogError(ex,
                    "Failed to calculate the content version hash for resource '{ResourceName}'. It will be served without a version parameter.",
                    resourceName);
                return string.Empty;
            }
        }

        private static string ComputeVersion(string content)
        {
            var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(content));
            return Convert.ToBase64String(hashBytes).Replace("+", "-").Replace("/", "_").Replace("=", "")[..32];
        }
    }
}
