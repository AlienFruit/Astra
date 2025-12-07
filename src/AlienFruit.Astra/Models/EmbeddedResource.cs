using AlienFruit.Astra.Abstractions;
using System.Reflection;

namespace AlienFruit.Astra.Models
{
    public class EmbeddedResource(string name, string path, Assembly assembly) : Resource(name)
    {
        public override Stream GetStream() 
            => assembly.GetManifestResourceStream(path)
            ?? throw new KeyNotFoundException($"There is no resource with path: {path}");
    }
}
