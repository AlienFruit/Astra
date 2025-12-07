using AlienFruit.Astra.Abstractions;
using AlienFruit.Astra.Configuration;
using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.Options;
using Scriban;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace AlienFruit.Astra.Core
{
    internal class HtmlResourceRenderer(
        IResourceStorage resourceStorage,
        IOptions<AstraConfiguration> options) : IHtmlResourceRenderer
    {
        private readonly ConcurrentDictionary<string, AstraResourceLocation> resourceStyles = new();
        private readonly ConcurrentDictionary<string, AstraResourceLocation> resourceScripts = new();
        private readonly ConcurrentDictionary<string, AstraResourceLocation> resourceHtml = new();
        private readonly ConcurrentDictionary<string, string> resourceHashes = new();

        public void AddStylesheetResource(string name, string path, AstraResourceLocation resourceLocation = AstraResourceLocation.Header, Assembly? assembly = null)
        {
            if (resourceStyles.TryAdd(name, resourceLocation))
            {
                resourceStorage.RegisterResource(name, path, assembly ?? Assembly.GetExecutingAssembly());
                CalculateResourceHash(name);
            }
        }

        public void AddScriptResource(string name, string path, AstraResourceLocation resourceLocation = AstraResourceLocation.Header, Assembly? assembly = null)
        {
            if (resourceScripts.TryAdd(name, resourceLocation))
            {
                resourceStorage.RegisterResource(name, path, assembly ?? Assembly.GetExecutingAssembly());
                CalculateResourceHash(name);
            }
        }

        public void AddJsCode(string resourceName, string jsCode, AstraResourceLocation resourceLocation = AstraResourceLocation.Header, Assembly? assembly = null)
        {
            if (resourceScripts.TryAdd(resourceName, resourceLocation))
            {
                resourceStorage.RegisterResource(resourceName, jsCode);
                CalculateResourceHash(resourceName, jsCode);
            }
        }

        public void AddJsCodeFromTemplate<T>(
            string resourceName, 
            string templatePath, 
            T specification, 
            AstraResourceLocation resourceLocation = AstraResourceLocation.Header, Assembly? assembly = null) 
            where T : class
        {
            if (resourceScripts.TryAdd(resourceName, resourceLocation))
            {
                var resourceAssembly = assembly ?? Assembly.GetExecutingAssembly();
                using var stream = resourceAssembly.GetManifestResourceStream(templatePath)
                    ?? throw new InvalidOperationException($"There is no resource with path: {templatePath}");

                using var reader = new StreamReader(stream);
                var rawTemplate = reader.ReadToEnd();
                var template = Template.Parse(rawTemplate);
                var scriptContent = template.Render(specification, x => x.Name)
                    ?? throw new InvalidOperationException("Result script cannot be null");
                resourceStorage.RegisterResource(resourceName, scriptContent);

                CalculateResourceHash(resourceName, scriptContent);
            }
        }

        public bool IsResourceExists(string name)
            => resourceScripts.ContainsKey(name) || resourceStyles.ContainsKey(name);

        public IHtmlContent RenderHeaders()
        {
            var builder = new HtmlContentBuilder();
            foreach (var style in this.resourceStyles.Where(x => x.Value == AstraResourceLocation.Header))
            {
                var version = GetResourceVersion(style.Key);
                builder.AppendHtmlLine($"<link href=\"/{options.Value.ResourcesRoute}/{style.Key}{version}\" rel=\"stylesheet\" type=\"text/css\" />");
            }

            foreach (var script in this.resourceScripts.Where(x => x.Value == AstraResourceLocation.Header))
            {
                var version = GetResourceVersion(script.Key);
                builder.AppendHtmlLine($"<script src=\"/{options.Value.ResourcesRoute}/{script.Key}{version}\"></script>");
            }

            return builder;
        }

        public IHtmlContent RenderBodyResource(string name)
        {
            var builder = new HtmlContentBuilder();
            if (this.resourceStyles.TryGetValue(name, out var resourceLocation) && resourceLocation == AstraResourceLocation.Body)
            {
                return builder
                    .AppendHtmlLine("<style>")
                    .AppendHtmlLine(GetResourceContent(name))
                    .AppendHtmlLine("</style>");
            }

            if (this.resourceScripts.TryGetValue(name, out resourceLocation) && resourceLocation == AstraResourceLocation.Body)
            {
                return builder
                    .AppendHtmlLine("<script>")
                    .AppendHtmlLine(GetResourceContent(name))
                    .AppendHtmlLine("</script>");
            }

            throw new ArgumentException($"resource {name} was not registered as body resource");
        }

        public string GetResourceUrl(string resourceName)
        {
            var version = GetResourceVersion(resourceName);
            return $"/{options.Value.ResourcesRoute}/{resourceName}{version}";
        }

        private string GetResourceContent(string resourceName)
        {
            if (resourceStorage.Contains(resourceName) == false)
            {
                throw new ArgumentException($"The resource \"{resourceName}\" was not registered");
            }

            using var stream = resourceStorage.OpenRead(resourceName);
            using var streamReader = new StreamReader(stream);
            return streamReader.ReadToEnd();
        }

        private string GetResourceVersion(string resourceName)
        {
            if (!options.Value.EnableVersioning)
            {
                return string.Empty;
            }

            // If global version is specified, use it
            if (!string.IsNullOrEmpty(options.Value.ResourceVersion))
            {
                return $"?v={options.Value.ResourceVersion}";
            }

            // Otherwise use content hash
            if (resourceHashes.TryGetValue(resourceName, out var hash))
            {
                return $"?v={hash}";
            }

            return string.Empty;
        }

        private void CalculateResourceHash(string resourceName, string? content = null)
        {
            if (!options.Value.EnableVersioning || string.IsNullOrEmpty(options.Value.ResourceVersion) == false)
            {
                return;
            }

            try
            {
                string contentToHash = content ?? GetResourceContent(resourceName);
                var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(contentToHash));
                var hash = Convert.ToBase64String(hashBytes).Replace("+", "-").Replace("/", "_").Replace("=", "")[..32];
                resourceHashes.TryAdd(resourceName, hash);
            }
            catch
            {
                // If hash calculation failed, use timestamp
                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                resourceHashes.TryAdd(resourceName, timestamp.ToString());
            }
        }
    }
}
