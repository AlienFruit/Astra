namespace AlienFruit.Astra.Core
{
    public class MimeTypeMapper
    {
        private const string defaultContentType = "application/octet-stream";

        private static readonly IDictionary<string, string> mappings = new Dictionary<string, string>
        {
            { ".js", "application/x-javascript" },
            { ".css", "text/css" }
        };

        public static string GetMimeType(string fileName)
        {
            var extension = Path.GetExtension(fileName);
            return mappings.TryGetValue(extension, out var mimeType)
                ? mimeType : defaultContentType;
        }
    }
}
