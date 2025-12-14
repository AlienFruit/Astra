using System.Globalization;

namespace AlienFruit.Astra.Demo.Middleware
{
    public class LowercaseUrlMiddleware
    {
        private readonly RequestDelegate _next;

        public LowercaseUrlMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Проверяем, содержит ли URL заглавные буквы
            var path = context.Request.Path.Value;
            var query = context.Request.QueryString.Value;

            if (!string.IsNullOrEmpty(path))
            {
                var lowercasePath = path.ToLowerInvariant();
                if (path != lowercasePath)
                {
                    // Строим новый URL с lowercase путем
                    var newUrl = lowercasePath + query;
                    context.Response.Redirect(newUrl, permanent: true);
                    return;
                }
            }

            await _next(context);
        }
    }

    public static class LowercaseUrlMiddlewareExtensions
    {
        public static IApplicationBuilder UseLowercaseUrl(this IApplicationBuilder app)
        {
            return app.UseMiddleware<LowercaseUrlMiddleware>();
        }
    }
}