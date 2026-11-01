using AlienFruit.Astra.Abstractions;
using AlienFruit.Astra.Configuration;
using Microsoft.AspNetCore.Html;
using System.Collections.Concurrent;
using System.Reflection;
using System.Security.Cryptography;

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
        AstraConfiguration configuration) : IHtmlResourceRenderer
    {
        private readonly ConcurrentDictionary<string, ResourceEntry> entries = new();

        private readonly Dictionary<string, AstraResourceKind> resourceKindByExtensionMap = new(StringComparer.OrdinalIgnoreCase)
        {
            {".css", AstraResourceKind.Stylesheet },
            { ".js", AstraResourceKind.Script },
        };

        public void AddResource(string name, string path, Assembly? assembly = null)
        {
            if (this.entries.TryGetValue(name, out var existing))
            {
                ThrowIfContentDiffers(name, path, existing);
                return;
            }

            if (!resourceKindByExtensionMap.TryGetValue(Path.GetExtension(path), out var kind))
            {
                throw new ArgumentException($"Unsupported resource extension for '{name}'. Only .css and .js are allowed.");
            }

            var resourceAssembly = assembly ?? Assembly.GetExecutingAssembly();
            resourceStorage.RegisterResource(name, path, resourceAssembly);
            var version = ComputeVersionFromStorage(name);
            var entry = new ResourceEntry(name, path, kind, version);
            this.entries.TryAdd(name, entry);
        }

        public IHtmlContent RenderHeaders()
        {
            var builder = new HtmlContentBuilder();
            foreach (var entry in entries.Values.Where(x => x.Kind == AstraResourceKind.Stylesheet))
            {
                builder.AppendHtmlLine($"<link href=\"/{configuration.ResourcesRoute}/{entry.Name}{GetResourceVersion(entry)}\" rel=\"stylesheet\" type=\"text/css\" />");
            }

            foreach (var entry in entries.Values.Where(x => x.Kind == AstraResourceKind.Script))
            {
                builder.AppendHtmlLine($"<script src=\"/{configuration.ResourcesRoute}/{entry.Name}{GetResourceVersion(entry)}\"></script>");
            }

            return builder;
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

        private void ThrowIfContentDiffers(string name, string path, ResourceEntry existing)
        {
            if (!string.Equals(path, existing.Path, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"Resource '{name}' path mismatch. Expected {existing.Path}, got {path}");
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
    }
}
