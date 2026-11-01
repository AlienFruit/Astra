namespace AlienFruit.Astra.Abstractions
{
    /// <summary>
    /// Minifies/normalizes resource content (JS, CSS, HTML) before it is served.
    /// Non-text or unknown extensions pass through unchanged.
    /// </summary>
    public interface IResourceCompressor
    {
        /// <summary>
        /// Compresses the resource and returns the result as a stream.
        /// </summary>
        /// <param name="resource">The resource whose content is compressed.</param>
        /// <returns>A stream with the compressed bytes; the caller owns and disposes it.</returns>
        Stream CompressToStream(Resource resource);

        /// <summary>
        /// Compresses the resource and returns the result as a string.
        /// </summary>
        /// <param name="resource">The resource whose content is compressed.</param>
        /// <returns>The compressed content.</returns>
        string CompressToString(Resource resource);
    }
}
