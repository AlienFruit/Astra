using AlienFruit.Astra.Abstractions;
using System.Reflection;

namespace AlienFruit.Astra.Models
{
    /// <summary>
    /// A resource served from an embedded (manifest) resource inside a .NET assembly.
    /// Useful for static content bundled with the application, such as JS files or HTML templates.
    /// </summary>
    /// <param name="name">A unique identifier for the resource.</param>
    /// <param name="path">The manifest resource name path to the embedded resource (e.g., "Namespace.Folder.FileName.html").</param>
    /// <param name="assembly">The assembly containing the embedded resource.</param>
    public class EmbeddedResource(string name, string path, Assembly assembly) : Resource(name)
    {
        /// <summary>
        /// Opens the embedded manifest resource stream.
        /// </summary>
        /// <returns>A stream with the embedded content.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when no resource exists at the configured path.</exception>
        public override Stream GetStream() 
            => assembly.GetManifestResourceStream(path)
            ?? throw new KeyNotFoundException($"There is no resource with path: {path}");
    }
}
