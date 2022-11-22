using System.Text;
using ConfigurationValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace Salix.AspNetCore.Utilities;

/// <summary>
/// Middleware to display configuration validation results as error page.
/// </summary>
public class ConfigurationValidationMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>
    /// Middleware to display configuration validation results as error page.
    /// </summary>
    public ConfigurationValidationMiddleware(RequestDelegate next) =>
        _next = next ?? throw new ArgumentNullException(nameof(next));

    /// <summary>
    /// Overridden method which gets invoked by HTTP middleware stack.
    /// </summary>
    /// <param name="httpContext">The HTTP context.</param>
    /// <param name="validatableConfigurations">Validatable configuration itens collection.</param>
    /// <param name="logger">Logger instance.</param>
    /// <exception cref="ArgumentNullException">HTTP Context does not exist (never happens).</exception>
#pragma warning disable RCS1046 // Suffix Async is not expected by ASP.NET Core implementation
    public async Task Invoke(HttpContext httpContext, IEnumerable<IValidatableConfiguration> validatableConfigurations, ILogger<ConfigurationValidationMiddleware> logger)
#pragma warning restore RCS1046
    {
        logger.LogDebug($"Validating configuration of {validatableConfigurations.Count()} objects.");
        var failures = new List<ConfigurationValidationItem>();
        foreach (var validatableObject in validatableConfigurations)
        {
            failures.AddRange(validatableObject.Validate());
        }

        if (failures.Count == 0)
        {
            logger.LogDebug("All configurations are valid.");
            await _next(httpContext);
            return;
        }

        // Put configuration validation failures in log.
        logger.LogError($"Found {failures.Count} problems in configuration.");
        foreach (var failure in failures)
        {
            logger.LogError($"Configuration section {failure.ConfigurationSection}, item {failure.ConfigurationItem}: {failure.ValidationMessage}");
        }

        // Now getting page template and pushing it to response
        string errorPage = Pages.Html.config_errors;
        var errorTable = new StringBuilder();
        foreach (var failure in failures)
        {
            errorTable.Append($"<tr><td>{failure.ConfigurationSection}</td><td>{failure.ConfigurationItem}</td><td>{failure.ValidationMessage}</td></tr>");
        }

        errorPage = errorPage.Replace("{Validations}", errorTable.ToString());
        var response = httpContext.Response;
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
