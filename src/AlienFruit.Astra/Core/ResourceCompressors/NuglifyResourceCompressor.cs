using AlienFruit.Astra.Abstractions;
using NUglify;
using System.Text;

namespace AlienFruit.Astra.Core.ResourceCompressors
{
    public class NuglifyResourceCompressor : IResourceCompressor
    {
        public Stream CompressToStream(Abstractions.Resource resource)
        {
            var mimeType = GetFileMimType(resource.Name);
            if (mimeType == MimeType.Unknown)
            {
                return resource.GetStream();
            }

            var content = Compress(resource.GetString(), mimeType);
            var resultStream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            return resultStream;
        }

        public string CompressToString(Abstractions.Resource resource)
        {
            var mimeType = GetFileMimType(resource.Name);
            if (mimeType == MimeType.Unknown)
            {
                return resource.GetString();
            }

            var result = Compress(resource.GetString(), mimeType);
            return result;
        }

        private static string Compress(string input, MimeType mimeType)
        {
            switch (mimeType)
            {
                case MimeType.Css:
                    return Uglify.Css(input).Code;
                case MimeType.Js:
                    return Uglify.Js(input).Code;
                case MimeType.Html:
                    return Uglify.Html(input).Code;
                default:
                    return input;
            }
        }

        private static MimeType GetFileMimType(string fileName)
        {
            if (fileName.EndsWith(".js", StringComparison.OrdinalIgnoreCase))
                return MimeType.Js;

            if (fileName.EndsWith(".css", StringComparison.OrdinalIgnoreCase))
                return MimeType.Css;

            if (fileName.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
                return MimeType.Html;

            return MimeType.Unknown;
        }

        private enum MimeType
        {
            Unknown,
            Css,
            Js,
            Html
        }
    }
}
