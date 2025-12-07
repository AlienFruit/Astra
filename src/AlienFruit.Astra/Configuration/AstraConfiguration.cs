namespace AlienFruit.Astra.Configuration
{
    public class AstraConfiguration
    {
        public static string Name => nameof(AstraConfiguration);

        public bool UseCompression { get; set; } = true;

        public string ResourcesRoute { get; set; } = "astra";

        /// <summary>
        /// Enables resource versioning for cache busting
        /// </summary>
        public bool EnableVersioning { get; set; } = false;

        /// <summary>
        /// Version for resources (if not specified, content hash will be used)
        /// </summary>
        public string? ResourceVersion { get; set; }

        /// <summary>
        /// Resource cache time in seconds (default is 1 year)
        /// </summary>
        public int CacheMaxAge { get; set; } = 31536000; // 1 year in seconds
    }
}
