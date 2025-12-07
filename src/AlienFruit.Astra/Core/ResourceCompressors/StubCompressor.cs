using AlienFruit.Astra.Abstractions;

namespace AlienFruit.Astra.Core.ResourceCompressors
{
    internal class StubCompressor : IResourceCompressor
    {
        public Stream CompressToStream(Resource resource) => resource.GetStream();

        public string CompressToString(Resource resource) => resource.GetString();
    }
}
