using Microsoft.AspNetCore.Builder;
using Salix.AspNetCore.Utilities;

namespace Sample.AspNet5.Api.Middleware
{
    public static class MiddlewareRegistrationExtensions
    {
        /// <summary>
        /// Adds specific unhandled error handler to return JSON-ized <see cref="ApiError" /> to caller.
        /// </summary>
        /// <param name="app">The ASP.NET application.</param>
        /// <param name="isDevelopment">True - development environment (more information can be shown).</param>
        public static IApplicationBuilder UseJsonErrorHandler(this IApplicationBuilder app, bool isDevelopment = false) => 
            app.UseMiddleware<ApiJsonErrorMiddleware>(isDevelopment);
    }
}
