using AlienFruit.Astra.Abstractions;
using AlienFruit.Astra.Models;
using System.Collections.Concurrent;
using System.Reflection;

namespace AlienFruit.Astra.Core
{
    /// <summary>
    /// Name-keyed store of registered resources; first registration wins.
    /// Reads pass content through <see cref="IResourceCompressor"/>.
    /// </summary>
    internal class ResourceStorage(IResourceCompressor resourceCompressor) : IResourceStorage
    {
        private readonly ConcurrentDictionary<string, Resource> resources = new();

        /// <inheritdoc />
        public void RegisterResource(string name, string path, Assembly assembly)
        {
            this.resources.TryAdd(name, new EmbeddedResource(name, path, assembly));
        }

        /// <inheritdoc />
        public void RegisterResource(string name, string content)
        {
            this.resources.TryAdd(name, new InMemoryResource(name, content));
        }

        /// <inheritdoc />
        public void RegisterResource(Resource resource)
        {
            this.resources.TryAdd(resource.Name, resource);
        }

        /// <inheritdoc />
        public bool Contains(string name) => this.resources.ContainsKey(name);

        /// <inheritdoc />
        public Task<Stream> OpenReadAsync(string name) => Task.FromResult(OpenRead(name));

        /// <inheritdoc />
        public Stream OpenRead(string name)
        {
            if (!resources.TryGetValue(name, out var resource))
            {
                throw new ArgumentException($"The file \"{name}\" was not registered");
            }
            return resourceCompressor.CompressToStream(resource);
        }
    }
}
