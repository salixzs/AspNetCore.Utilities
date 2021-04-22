using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConfigurationValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace Salix.AspNetCore.Utilities
{
    public class ConfigurationValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IEnumerable<IValidatableConfiguration> _validatableConfigurations;
        private readonly ILogger<ConfigurationValidationMiddleware> _logger;

        public ConfigurationValidationMiddleware(RequestDelegate next, IEnumerable<IValidatableConfiguration> validatableConfigurations, ILogger<ConfigurationValidationMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _validatableConfigurations = validatableConfigurations;
            _logger = logger;
        }

        /// <summary>
        /// Overridden method which gets invoked by HTTP middleware stack.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <exception cref="ArgumentNullException">HTTP Context does not exist (never happens).</exception>
#pragma warning disable RCS1046 // Suffix Async is not expected by ASP.NET Core implementation
        public async Task Invoke(HttpContext httpContext)
#pragma warning restore RCS1046
        {
            _logger.LogDebug($"Validating configuration of {_validatableConfigurations.Count()} objects.");
            var failures = new List<ConfigurationValidationItem>();
            foreach (IValidatableConfiguration validatableObject in _validatableConfigurations)
            {
                failures.AddRange(validatableObject.Validate());
            }

            if (failures.Count == 0)
            {
                _logger.LogDebug("All configurations are valid.");
                await _next(httpContext);
                return;
            }

            // Put configuration validation failures in log.
            _logger.LogError($"Found {failures.Count} problems in configuration.");
            foreach (ConfigurationValidationItem failure in failures)
            {
                _logger.LogError($"Configuration section {failure.ConfigurationSection}, item {failure.ConfigurationItem}: {failure.ValidationMessage}");
            }

            // Now getting page template and pushing it to response
            string errorPage = Pages.Html.config_errors;
            var errorTable = new StringBuilder();
            foreach (ConfigurationValidationItem failure in failures)
            {
                errorTable.Append($"<tr><td>{failure.ConfigurationSection}</td><td>{failure.ConfigurationItem}</td><td>{failure.ValidationMessage}</td></tr>");
            }

            errorPage = errorPage.Replace("{Validations}", errorTable.ToString());
            HttpResponse response = httpContext.Response;
            response.Clear();
            response.ContentType = "text/html";
            response.StatusCode = 500;
            response.Headers[HeaderNames.CacheControl] = "no-cache";
            response.Headers[HeaderNames.Pragma] = "no-cache";
            response.Headers[HeaderNames.Expires] = "-1";
            response.Headers.Remove(HeaderNames.ETag);
            await response.WriteAsync(errorPage).ConfigureAwait(false);
        }
    }
}
