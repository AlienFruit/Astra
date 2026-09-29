using AlienFruit.Astra.Abstractions;
using AlienFruit.Astra.Configuration;
using AlienFruit.Astra.Core;
using AlienFruit.Astra.Core.ResourceCompressors;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AlienFruit.Astra.DependencyInjection
{
    /// <summary>
    /// Provides extension methods for registering AlienFruit.Astra services in ASP.NET Core applications.
    /// Contains methods for adding services to the dependency injection container and configuring endpoints.
    /// </summary>
    public static class AstraRegistration
    {
        private const string CacheControlHeader = "Cache-Control";
        private const string ExpiresHeader = "Expires";

        /// <summary>
        /// Adds AlienFruit.Astra services to the dependency injection container.
        /// Registers all necessary services including resource management, compression, and the main AstraEngine.
        /// </summary>
        /// <param name="builder">The web application builder to add services to.</param>
        /// <param name="configureOptions">Optional action to configure Astra options. If not provided, configuration will be read from appsettings.json.</param>
        /// <returns>The modified web application builder for method chaining.</returns>
        public static WebApplicationBuilder AddAstra(this WebApplicationBuilder builder, Action<AstraConfiguration>? configureOptions = null)
        {
            var section = builder.Configuration.GetSection(AstraConfiguration.Name);
            var configuration = section.Get<AstraConfiguration>() ?? new AstraConfiguration();
            configureOptions?.Invoke(configuration);
            builder.Services.AddSingleton(configuration);

            if (configuration.UseCompression)
            {
                builder.Services.AddSingleton<IResourceCompressor, NuglifyResourceCompressor>();
            }
            else
            {
                builder.Services.AddSingleton<IResourceCompressor, StubCompressor>();
            }

            builder.Services
                .AddSingleton<IResourceStorage, ResourceStorage>()
                .AddSingleton<IHtmlResourceRenderer, HtmlResourceRenderer>()
                .AddSingleton<AstraEngine>()
                .AddHttpContextAccessor();

            return builder;
        }

        /// <summary>
        /// Configures the endpoint routing for serving Astra resources (CSS/JS files).
        /// Maps GET requests to the configured resources route for serving compressed and cached resources.
        /// </summary>
        /// <param name="app">The endpoint route builder to configure.</param>
        /// <returns>The modified endpoint route builder for method chaining.</returns>
        /// <exception cref="InvalidOperationException">Thrown when AstraConfiguration is not registered in the service container.</exception>
        public static IEndpointRouteBuilder UseAstra(this IEndpointRouteBuilder app)
        {
            var configuration = app.ServiceProvider.GetService<AstraConfiguration>()
                ?? throw new InvalidOperationException("AstraConfiguration is not registered. Please make sure to call AddAstra in the service configuration.");

            app.MapGet($"{configuration.ResourcesRoute}/{{name}}", async (string name, IResourceStorage storage, HttpContext context) =>
            {
                var resourceName = name.Split('?')[0];
                
                if (!storage.Contains(resourceName))
                {
                    return Results.NotFound(); 
                }
                var contentType = MimeTypeMapper.GetMimeType(resourceName);
                var stream = await storage.OpenReadAsync(resourceName);
                
                if (configuration.CacheMaxAge > 0)
                {
                    context.Response.Headers.TryAdd(CacheControlHeader, $"public, max-age={configuration.CacheMaxAge}");
                    context.Response.Headers.TryAdd(ExpiresHeader, DateTime.UtcNow.AddSeconds(configuration.CacheMaxAge).ToString("R"));
                }
                
                return Results.File(stream, contentType);
            });
            return app;
        }
    }
}