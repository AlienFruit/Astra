using AlienFruit.Astra.Abstractions;
using AlienFruit.Astra.Configuration;
using AlienFruit.Astra.Core;
using AlienFruit.Astra.Core.ResourceCompressors;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AlienFruit.Astra.DependencyInjection
{
    public static class AstraRegistration
    {
        private const string CacheControlHeader = "Cache-Control";
        private const string ExpiresHeader = "Expires";

        public static WebApplicationBuilder AddAstra(this WebApplicationBuilder builder)
        {
            var section = builder.Configuration.GetSection(AstraConfiguration.Name);
            builder.Services.Configure<AstraConfiguration>(section);

            var configuration = section.Get<AstraConfiguration>() ?? new AstraConfiguration();

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

        public static IEndpointRouteBuilder UseAstra(this IEndpointRouteBuilder app)
        {
            var configuration = app.ServiceProvider.GetService<IOptions<AstraConfiguration>>();

            app.MapGet($"{configuration.Value.ResourcesRoute}/{{name}}", async (string name, IResourceStorage storage, HttpContext context) =>
            {
                var resourceName = name.Split('?')[0];
                
                if (!storage.Contains(resourceName))
                {
                    return Results.NotFound();
                }
                var contentType = MimeTypeMapper.GetMimeType(resourceName);
                var stream = await storage.OpenReadAsync(resourceName);
                
                if (configuration.Value.CacheMaxAge > 0)
                {
                    if (!context.Response.Headers.ContainsKey(CacheControlHeader))
                    {
                        context.Response.Headers.Add(CacheControlHeader, $"public, max-age={configuration.Value.CacheMaxAge}");
                    }
                    if (!context.Response.Headers.ContainsKey(ExpiresHeader))
                    {
                        context.Response.Headers.Add(ExpiresHeader, DateTime.UtcNow.AddSeconds(configuration.Value.CacheMaxAge).ToString("R"));
                    }
                }
                
                return Results.File(stream, contentType);
            });
            return app;
        }
    }
}