using AlienFruit.Astra.Abstractions;
using System.Text;

namespace AlienFruit.Astra.Models
{
    public class InMemoryResource(string name, string content) : Resource(name)
    {
        public override Stream GetStream() 
        {
            var resultStream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            return resultStream;
        }
    }
}
