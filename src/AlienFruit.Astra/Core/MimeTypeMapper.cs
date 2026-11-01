namespace AlienFruit.Astra.Core
{
    /// <summary>
    /// Maps file extensions to MIME content types for resources served by Astra.
    /// </summary>
    public class MimeTypeMapper
    {
        private const string defaultContentType = "application/octet-stream";

        private static readonly IDictionary<string, string> mappings = new Dictionary<string, string>
        {
            { ".js", "application/x-javascript" },
            { ".css", "text/css" }
        };

        /// <summary>
        /// Gets the MIME type for a file name based on its extension.
        /// </summary>
        /// <param name="fileName">The file name (with extension) to inspect.</param>
        /// <returns>The mapped MIME type, or <c>application/octet-stream</c> for unknown extensions.</returns>
        public static string GetMimeType(string fileName)
        {
            var extension = Path.GetExtension(fileName);
            return mappings.TryGetValue(extension, out var mimeType)
                ? mimeType : defaultContentType;
        }
    }
}
