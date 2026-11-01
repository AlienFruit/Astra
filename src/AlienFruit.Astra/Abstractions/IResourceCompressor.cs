namespace AlienFruit.Astra.Abstractions
{
    public interface IResourceCompressor
    {
        Stream CompressToStream(Resource resource);

        string CompressToString(Resource resource);
    }
}
