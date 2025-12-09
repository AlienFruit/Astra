using AlienFruit.Astra.Abstractions;
using System.Reflection;

namespace AlienFruit.Astra.Models
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmbeddedResource"/> class with the specified name, embedded resource path, and assembly.
    /// Creates a resource that serves content from embedded resources within .NET assemblies.
    /// This resource type is useful for complex HTML templates or static content that should be bundled with the application.
    /// </summary>
    /// <param name="name">A unique identifier for the resource.</param>
    /// <param name="path">The manifest resource name path to the embedded resource (e.g., "Namespace.Folder.FileName.html").</param>
    /// <param name="assembly">The assembly containing the embedded resource.</param>
    public class EmbeddedResource(string name, string path, Assembly assembly) : Resource(name)
    {
        public override Stream GetStream() 
            => assembly.GetManifestResourceStream(path)
            ?? throw new KeyNotFoundException($"There is no resource with path: {path}");
    }
}
