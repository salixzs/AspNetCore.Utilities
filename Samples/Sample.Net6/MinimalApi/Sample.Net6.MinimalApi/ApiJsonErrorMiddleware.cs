using Salix.AspNetCore.Utilities;

namespace Sample.Net6.MinimalApi
{
    public class ApiJsonErrorMiddleware : ApiJsonExceptionMiddleware
    {
        public ApiJsonErrorMiddleware(RequestDelegate next, ILogger<ApiJsonExceptionMiddleware> logger, bool showStackTrace = true)
            : base(next, logger, showStackTrace)
        {
        }
    }
}
