namespace AlienFruit.Astra.Configuration
{
    public class AstraConfiguration
    {
        public static string Name => nameof(AstraConfiguration);

        public bool UseCompression { get; set; } = true;

        public string ResourcesRoute { get; set; } = "astra";

        /// <summary>
        /// Включает версионирование ресурсов для кэш-бастинга
        /// </summary>
        public bool EnableVersioning { get; set; } = false;

        /// <summary>
        /// Версия для ресурсов (если не указана, будет использоваться хеш содержимого)
        /// </summary>
        public string? ResourceVersion { get; set; }

        /// <summary>
        /// Время кеширования ресурсов в секундах (по умолчанию 1 год)
        /// </summary>
        public int CacheMaxAge { get; set; } = 31536000; // 1 год в секундах
    }
}
