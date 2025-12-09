using AlienFruit.Astra.Abstractions;
using System.Text;

namespace AlienFruit.Astra.Models
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryResource"/> class with the specified name and HTML content.
    /// Creates an in-memory resource containing HTML content for error messages or other UI elements.
    /// This resource type is useful for simple text-based content that doesn't need to be stored as embedded resources.
    /// </summary>
    /// <param name="name">A unique identifier for the resource.</param>
    /// <param name="content">The HTML content to be served by this resource.</param>
    public class InMemoryResource(string name, string content) : Resource(name)
    {
        public override Stream GetStream() 
        {
            var resultStream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            return resultStream;
        }
    }
}
