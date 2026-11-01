using AlienFruit.Astra.Abstractions;

namespace AlienFruit.Astra.Core.ResourceCompressors
{
    /// <summary>
    /// Pass-through compressor used when <c>UseCompression</c> is disabled.
    /// </summary>
    internal class StubCompressor : IResourceCompressor
    {
        /// <inheritdoc />
        public Stream CompressToStream(Resource resource) => resource.GetStream();

        /// <inheritdoc />
        public string CompressToString(Resource resource) => resource.GetString();
    }
}
