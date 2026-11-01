using Microsoft.AspNetCore.Http;

namespace AlienFruit.Astra.Extensions
{
    /// <summary>
    /// Request helpers for the Astra AJAX navigation protocol.
    /// </summary>
    public static class HttpRequestBaseExtensions
    {
        /// <summary>
        /// Determines whether the request was issued by the Astra client-side loader.
        /// Astra marks AJAX navigations with the presence of the "Ajax-Request" header;
        /// the header value is ignored.
        /// </summary>
        /// <param name="request">The HTTP request to inspect.</param>
        /// <returns><c>true</c> if the request carries the "Ajax-Request" header.</returns>
        public static bool IsAjaxRequest(this HttpRequest request)
        {
            return request.Headers.ContainsKey("Ajax-Request");
        }
    }
}
