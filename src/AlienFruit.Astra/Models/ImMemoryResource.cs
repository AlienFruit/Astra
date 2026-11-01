using AlienFruit.Astra.Abstractions;
using System.Text;

namespace AlienFruit.Astra.Models
{
    /// <summary>
    /// An in-memory resource serving string content (e.g. error-message HTML) that does not
    /// need to be stored as an embedded assembly file.
    /// </summary>
    /// <param name="name">A unique identifier for the resource.</param>
    /// <param name="content">The content to be served by this resource.</param>
    public class InMemoryResource(string name, string content) : Resource(name)
    {
        /// <summary>
        /// Returns the content as a fresh UTF-8 stream on each call.
        /// </summary>
        /// <returns>A stream with the in-memory content.</returns>
        public override Stream GetStream() 
        {
            var resultStream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            return resultStream;
        }
    }
}
