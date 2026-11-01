using System.Reflection;

namespace AlienFruit.Astra.Abstractions
{
    /// <summary>
    /// Returns stream of required resources
    /// </summary>
    public interface IResourceStorage
    {
        /// <summary>
        /// Registers an embedded assembly resource under a unique name.
        /// First registration wins; re-registering an existing name is silently ignored.
        /// </summary>
        /// <param name="name">Unique name, use extension if compression is needed. Example: test.js</param>
        /// <param name="path">Full path to resource within assembly</param>
        /// <param name="assembly">Library containing the resource</param>
        void RegisterResource(string name, string path, Assembly assembly);

        /// <summary>
        /// Registers in-memory content under a unique name.
        /// First registration wins; re-registering an existing name is silently ignored.
        /// </summary>
        /// <param name="name">Use extension if compression is needed. Example: test.js</param>
        /// <param name="content">The content to serve for this resource.</param>
        void RegisterResource(string name, string content);

        /// <summary>
        /// Registers a ready-made <see cref="Resource"/> instance, keyed by <see cref="Resource.Name"/>.
        /// First registration wins; re-registering an existing name is silently ignored.
        /// </summary>
        /// <param name="resource">The resource to store.</param>
        void RegisterResource(Resource resource);

        /// <summary>
        /// Determines whether a resource with the given name has been registered.
        /// </summary>
        /// <param name="name">The resource name to look up.</param>
        /// <returns><c>true</c> if the resource exists; otherwise <c>false</c>.</returns>
        bool Contains(string name);

        /// <summary>
        /// Opens the (compressed, when a compressor is active) resource content asynchronously.
        /// </summary>
        /// <param name="name">The registered resource name.</param>
        /// <returns>A stream with the resource bytes; the caller owns and disposes it.</returns>
        /// <exception cref="ArgumentException">Thrown when the resource name is not registered.</exception>
        Task<Stream> OpenReadAsync(string name);

        /// <summary>
        /// Opens the (compressed, when a compressor is active) resource content.
        /// </summary>
        /// <param name="name">The registered resource name.</param>
        /// <returns>A stream with the resource bytes; the caller owns and disposes it.</returns>
        /// <exception cref="ArgumentException">Thrown when the resource name is not registered.</exception>
        Stream OpenRead(string name);
    }
}
