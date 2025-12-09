namespace AlienFruit.Astra.Abstractions
{
    /// <summary>
    /// Abstract base class for resources used in AlienFruit.Astra.
    /// Resources represent content that can be loaded and used by the framework,
    /// such as error messages, JavaScript files, CSS files, or any other content
    /// that needs to be served dynamically.
    /// </summary>
    /// <remarks>
    /// Implementations include:
    /// - <see cref="AlienFruit.Astra.Models.EmbeddedResource"/> for embedded assembly resources
    /// - <see cref="AlienFruit.Astra.Models.InMemoryResource"/> for in-memory content
    /// - Custom implementations for database resources, file system resources, etc.
    /// </remarks>
    public abstract class Resource(string name)
    {
        /// <summary>
        /// Gets the unique name identifier for this resource.
        /// </summary>
        public string Name { get; } = name;

        /// <summary>
        /// Gets a stream containing the resource content.
        /// This method must be implemented by derived classes to provide
        /// access to the actual resource data.
        /// </summary>
        /// <returns>A <see cref="Stream"/> containing the resource content.</returns>
        public abstract Stream GetStream();

        /// <summary>
        /// Gets the resource content as a string by reading from the stream.
        /// This is a convenience method that automatically handles stream reading
        /// and encoding conversion to UTF-8 text.
        /// </summary>
        /// <returns>The resource content as a string.</returns>
        public string GetString()
        {
            using var reader = new StreamReader(GetStream());
            return reader.ReadToEnd();
        }

    }
}
