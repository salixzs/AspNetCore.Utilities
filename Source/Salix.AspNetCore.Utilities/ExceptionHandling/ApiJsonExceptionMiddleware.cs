using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Salix.AspNetCore.Utilities.ExceptionHandling;

namespace Salix.AspNetCore.Utilities;

/// <summary>
/// Handles API errors (HTTP code > 399) as special Error object returned from API.
/// </summary>
[DebuggerDisplay("Global Error Handler")]
public abstract class ApiJsonExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ApiJsonExceptionOptions _options;

    protected ILogger<ApiJsonExceptionMiddleware> Logger { get; }

    /// <summary>
    /// Middleware for intercepting unhandled exceptions and returning error object with appropriate status code.
    /// </summary>
    /// <param name="next">The next configured middleware in chain (setup in Startup.cs).</param>
    /// <param name="logger">The logger.</param>
    /// <param name="showStackTrace">
    /// Shows stack trace records.
    /// Usually this should be taken from IsDevelopment from Api.Hosting environment.
    /// Pass constant "true" to show stack trace always.
    /// </param>
    /// <exception cref="ArgumentNullException">Next step is not defined (should not ever happen).</exception>
    public ApiJsonExceptionMiddleware(RequestDelegate next, ILogger<ApiJsonExceptionMiddleware> logger, bool showStackTrace = false)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        this.Logger = logger;
        _options = new ApiJsonExceptionOptions
        {
            ShowStackTrace = showStackTrace
        };
        _options.OmitSources.Add("ApiJsonExceptionMiddleware.cs");
    }


    /// <summary>
    /// Middleware for intercepting unhandled exceptions and returning error object with appropriate status code.
    /// </summary>
    /// <param name="next">The next configured middleware in chain (setup in Startup.cs).</param>
    /// <param name="logger">The logger.</param>
    /// <param name="options">Set options for Error Handling.</param>
    /// <exception cref="ArgumentNullException">Next step is not defined (should not ever happen).</exception>
    public ApiJsonExceptionMiddleware(RequestDelegate next, ILogger<ApiJsonExceptionMiddleware> logger, ApiJsonExceptionOptions options)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        this.Logger = logger;
        _options = options;
        _options.OmitSources.Add("ApiJsonExceptionMiddleware.cs");
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
        try
        {
            await _next(httpContext);
        }
        catch (Exception exc)
        {
            // We can't do anything if the response has already started, just abort.
            if (httpContext.Response.HasStarted)
            {
                this.Logger.LogError(exc, "Unhandled exception occurred of type {ExceptionType} with message: \"{ExceptionMessage}\". Response started - no JSON handler is launched!", exc.GetType().Name, exc.Message);
                throw;
            }

            var errorObject = this.CreateErrorObject(exc, httpContext.Response?.StatusCode);
            errorObject.RequestedUrl = httpContext.Features.Get<IHttpRequestFeature>()?.RawTarget ?? httpContext.Request?.Path.ToString();
            if (errorObject.ErrorType == ApiErrorType.DataValidationError)
            {
                this.Logger.LogError(exc, "Data validation exception occurred with message: \"{ExceptionMessage}\".", exc.Message);
                await WriteExceptionAsync(httpContext, errorObject, 400).ConfigureAwait(false);
            }
            else
            {
                this.Logger.LogError(
                    exc,
                    "Unhandled exception occurred of type {ExceptionType} with message: \"{ExceptionMessage}\".",
                    exc.GetType().Name,
                    exc.Message);
                await WriteExceptionAsync(httpContext, errorObject, errorObject.Status > 399 ? errorObject.Status : 500
                ).ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// Handler for Data Validation exception in API.
    /// Should be overridden in implementing class to fill with necessary data into ApiError object.
    /// </summary>
    /// <param name="apiError">Prepared API Error (as reference) to append any handled error information.</param>
    /// <param name="exception">Exception thrown in application.</param>
    /// <returns>Updated <paramref name="apiError"/>.</returns>
#pragma warning disable IDE0060 // Remove unused parameter - will be overridden.
    protected virtual ApiError HandleSpecialException(ApiError apiError, Exception exception) => apiError;

    /// <summary>
    /// Creates the error data object from exception.
    /// </summary>
    /// <param name="exception">The exception which caused problems.</param>
    /// <param name="statusCode">The initial response status code.</param>
    private ApiError CreateErrorObject(Exception exception, int? statusCode)
    {
        var errorData = new ApiError
        {
            Title = exception.Message,
            ExceptionType = exception.GetType().Name,
            ErrorType = ApiErrorType.ServerError,
            Status = statusCode.HasValue && statusCode.Value > 399 ? statusCode.Value : 500
        };

        // All special exceptions are to be handled by overriding class.
        errorData = this.HandleSpecialException(errorData, exception);

        if (exception.InnerException != null)
        {
            errorData.InnerException = $"({exception.InnerException.GetType().Name}) {exception.InnerException.Message}";
            if (exception.InnerException?.InnerException != null)
            {
                var innerInnerException = exception.InnerException.InnerException;
                var innerInnerExceptionMessages = new List<string>();
                do
                {
                    innerInnerExceptionMessages.Add($"({innerInnerException.GetType().Name}) {innerInnerException.Message}");
                    innerInnerException = innerInnerException.InnerException;
                }
                while (innerInnerException != null);
                errorData.InnerInnerException = string.Join("; ", innerInnerExceptionMessages);
            }
        }

        if (_options.ShowStackTrace)
        {
            // As there can be any kind of errors retrieving stack trace
            try
            {
                errorData.StackTrace = GetStackTrace(exception, _options.OmitSources);
            }
            catch (Exception ex)
            {
                errorData.StackTrace = new List<string> { "Error getting original stack trace: " + ex.Message };
            }
        }

        return errorData;
    }

    /// <summary>
    /// Writes the exception as JSON object to requester back asynchronously.
    /// </summary>
    /// <param name="context">The HTTP context (where Request and Response resides).</param>
    /// <param name="errorData">The prepared error data object.</param>
    /// <param name="responseCode">HTTP Response code to use for error.</param>
    private static async Task WriteExceptionAsync(HttpContext context, ApiError errorData, int responseCode = 500)
    {
        var response = context.Response;
        response.Clear();
        response.ContentType = "application/json";
        response.StatusCode = responseCode;
        response.Headers[HeaderNames.CacheControl] = "no-cache";
        response.Headers[HeaderNames.Pragma] = "no-cache";
        response.Headers[HeaderNames.Expires] = "-1";
        response.Headers.Remove(HeaderNames.ETag);
        await response.WriteAsync(JsonSerializer.Serialize(errorData, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true }));
    }

    /// <summary>
    /// Gets the stack trace of exception in suitable format.
    /// </summary>
    /// <param name="exception">The exception.</param>
    private static List<string> GetStackTrace(Exception exception, HashSet<string> omitContaining)
    {
        var frames = new List<string>();
        if (exception == null)
        {
            return frames;
        }

        var stackTrace = new StackTrace(exception, true);
        var stackFrames = stackTrace.GetFrames();
        if (stackFrames == null)
        {
            return frames;
        }

        // Common path parts (root folder etc.) is filled in loop below -
        // to remove them later to shorten stack trace and increase readability.
        var folderNameOccurrences = new Dictionary<string, FolderOccurrenceCount>();
        foreach (var frame in stackFrames)
        {
            string filePath = frame.GetFileName();
            string method = GetMethodSignature(frame.GetMethod());
            if (string.IsNullOrEmpty(filePath) || string.IsNullOrEmpty(method))
            {
                continue;
            }

            // Skip frames containing given omit strings
            if (omitContaining.Any(omit => filePath.IndexOf(omit, StringComparison.OrdinalIgnoreCase) > 0))
            {
                continue;
            }

            frames.Add($"at {method} in {filePath}: line {frame.GetFileLineNumber():D}");
            if (string.IsNullOrEmpty(filePath))
            {
                continue;
            }

            // Experience shows these can be both \ and / in single StackTrace
            char directorySeparatorChar = '\\';
            if (filePath.IndexOf(directorySeparatorChar) < 0)
            {
                directorySeparatorChar = '/';
                if (filePath.IndexOf(directorySeparatorChar) < 0)
                {
                    // Only file name
                    continue;
                }
            }

            var filepathFolders = filePath.Split(directorySeparatorChar).ToList();
            if (filepathFolders.Count < 3)
            {
                continue;
            }

            // Remove filename and 1 folder before it from entire file path (so they are preserved)
            filepathFolders.RemoveRange(filepathFolders.Count - 2, 2);
            var nonEmptyFolderList = filepathFolders.Where(f => !string.IsNullOrWhiteSpace(f)).ToList();
            foreach (string folderName in nonEmptyFolderList)
            {
                if (folderNameOccurrences.ContainsKey(folderName))
                {
                    folderNameOccurrences[folderName].Occurrences++;
                }
                else
                {
                    folderNameOccurrences.Add(folderName, new FolderOccurrenceCount(directorySeparatorChar));
                }
            }
        }

        // Remove folders, which are encountered only once (so they are left in stacktrace)
        var singleUseFolders = folderNameOccurrences
            .Where(o => o.Value.Occurrences <= 1)
            .ToList();
        foreach (var item in singleUseFolders)
        {
            folderNameOccurrences.Remove(item.Key);
        }

        for (var frameIndex = 0; frameIndex < frames.Count; frameIndex++)
        {
            foreach (var pathPart in folderNameOccurrences)
            {
                frames[frameIndex] = frames[frameIndex]
                    .Replace(pathPart.Key + pathPart.Value.DirectorySeparatorChar,
                        string.Empty);
            }
        }

        return frames;
    }

    /// <summary>
    /// Returns Method signature or null, if method is not passed in.
    /// </summary>
    /// <param name="method">Method information from reflection.</param>
    private static string GetMethodSignature(MethodBase method)
    {
        if (method == null)
        {
            return "?";
        }

        var type = method.DeclaringType;
        string methodName = method.Name;

        if (type?.IsDefined(typeof(CompilerGeneratedAttribute)) == true
            && (typeof(IAsyncStateMachine).IsAssignableFrom(type) || typeof(IEnumerator).IsAssignableFrom(type)))
        {
            method = ResolveStateMachineMethod(method);
            methodName = method.Name;
        }

        // Method name
        if (method.IsGenericMethod)
        {
            string genericArguments = string.Join(", ", method.GetGenericArguments()
                .Select(arg => TypeNameHelper.GetTypeDisplayName(arg, false, true)));
            methodName += "<" + genericArguments + ">";
        }

        // Method parameters
        methodName += "(";
        for (int i = 0; i < method.GetParameters().Length; i++)
        {
            if (i > 0)
            {
                methodName += ", ";
            }

            var parameterType = method.GetParameters()[i].ParameterType;
            string prefix = string.Empty;
            if (method.GetParameters()[i].IsOut)
            {
                prefix = "out ";
            }
            else if (parameterType.IsByRef)
            {
                prefix = "ref ";
            }

            if (parameterType.IsByRef)
            {
                parameterType = parameterType.GetElementType();
            }

            string parameterTypeString = TypeNameHelper.GetTypeDisplayName(parameterType, false, true);
            methodName += $"{prefix}{parameterTypeString} {method.GetParameters()[i].Name}";
        }

        methodName += ")";
        return methodName;
    }

    private static MethodBase ResolveStateMachineMethod(MethodBase method)
    {
        var declaringType = method.DeclaringType;
        var retMethod = method;
        var parentType = method.DeclaringType.DeclaringType;
        if (parentType == null)
        {
            return retMethod;
        }

        var methods = parentType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        foreach (var candidateMethod in methods)
        {
            var attributes = candidateMethod.GetCustomAttributes<StateMachineAttribute>();
            foreach (var sma in attributes)
            {
                if (sma.StateMachineType == declaringType)
                {
                    return candidateMethod;
                }
            }
        }

        return retMethod;
    }

    /// <summary>
    /// Internal DTO for repeating folder counting
    /// </summary>
    private class FolderOccurrenceCount
    {
        /// <summary>
        /// Holds count of folder occurrences in stack trace filePaths.
        /// </summary>
        public int Occurrences { get; set; } = 1;

        /// <summary>
        /// Stores directory separator char, used in this particular path (could be / or \).
        /// </summary>
        public char DirectorySeparatorChar { get; }

        public FolderOccurrenceCount(char directorySeparatorChar) =>
            this.DirectorySeparatorChar = directorySeparatorChar;
    }
}
