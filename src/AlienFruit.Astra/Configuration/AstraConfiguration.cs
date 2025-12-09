namespace AlienFruit.Astra.Configuration
{
    /// <summary>
    /// Configuration options for AlienFruit.Astra library behavior.
    /// Controls resource compression, versioning, caching, and routing settings.
    /// </summary>
    public class AstraConfiguration
    {
        /// <summary>
        /// Gets the configuration section name used for appsettings.json binding.
        /// </summary>
        public static string Name => nameof(AstraConfiguration);

        /// <summary>
        /// Gets or sets whether to enable GZIP compression for resources.
        /// Compression reduces bandwidth usage but increases CPU load. Default is true.
        /// </summary>
        public bool UseCompression { get; set; } = true;

        /// <summary>
        /// Gets or sets the route path for serving Astra resources (CSS/JS files).
        /// Default value is "astra".
        /// </summary>
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
