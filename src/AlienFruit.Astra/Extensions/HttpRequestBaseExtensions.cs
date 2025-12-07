using Microsoft.AspNetCore.Http;

namespace AlienFruit.Astra.Extensions
{
    public static class HttpRequestBaseExtensions
    {
        public static bool IsAjaxRequest(this HttpRequest request)
        {
            return request.Headers.ContainsKey("Ajax-Request");
        }
    }
}
