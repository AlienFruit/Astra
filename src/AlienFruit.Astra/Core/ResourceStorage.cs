using AlienFruit.Astra.Abstractions;
using AlienFruit.Astra.Models;
using System.Collections.Concurrent;
using System.Reflection;
using System.Xml.Linq;

namespace AlienFruit.Astra.Core
{
    internal class ResourceStorage(IResourceCompressor resourceCompressor) : IResourceStorage
    {
        private readonly ConcurrentDictionary<string, Resource> resources = new();

        public void RegisterResource(string name, string path, Assembly assembly)
        {
            this.resources.TryAdd(name, new EmbeddedResource(name, path, assembly));
        }

        public void RegisterResource(string name, string content)
        {
            this.resources.TryAdd(name, new InMemoryResource(name, content));
        }

        public void RegisterResource(Resource resource)
        {
            this.resources.TryAdd(resource.Name, resource);
        }

        public bool Contains(string name) => this.resources.ContainsKey(name);

        public Task<Stream> OpenReadAsync(string name) => Task.FromResult(OpenRead(name));

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
