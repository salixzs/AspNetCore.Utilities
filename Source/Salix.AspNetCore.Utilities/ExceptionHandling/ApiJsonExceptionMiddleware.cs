using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Salix.AspNetCore.Utilities
{
    /// <summary>
    /// Handles API errors (HTTP code > 399) as special Error object returned from API.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("ErrorHandler")]
    public abstract class ApiJsonExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        protected readonly ILogger<ApiJsonExceptionMiddleware> _logger;
        private readonly bool _showStackTrace;

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
            _logger = logger;
            _showStackTrace = showStackTrace;
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
                    _logger.LogError(exc, "Unhandled exception occurred of type {ExceptionType} with message: \"{ExceptionMessage}\". Response started - no JSON handler is launched!", exc.GetType().Name, exc.Message);
                    throw;
                }

                ApiError errorObject = this.CreateErrorObject(exc, httpContext.Response?.StatusCode);
                errorObject.RequestedUrl = httpContext.Features.Get<IHttpRequestFeature>()?.RawTarget ?? httpContext.Request.Path.ToString();
                if (errorObject.ErrorType == ApiErrorType.DataValidationError)
                {
                    _logger.LogError(exc, "Data validation exception occurred with message: \"{ExceptionMessage}\".", exc.Message);
                    await WriteExceptionAsync(httpContext, errorObject, 400).ConfigureAwait(false);
                }
                else
                {
                    _logger.LogError(
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
        /// Should be overriden in implementing class to fill with necessary data into ApiError object.
        /// </summary>
        /// <param name="apiError"></param>
        /// <param name="validationException"></param>
        /// <returns>Updated <paramref name="apiError"/>.</returns>
#pragma warning disable IDE0060 // Remove unused parameter - will be overriden
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
                    Exception innerInnerException = exception.InnerException.InnerException;
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

            if (_showStackTrace)
            {
                errorData.StackTrace = GetStackTrace(exception);
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
            HttpResponse response = context.Response;
            response.Clear();
            response.ContentType = "application/json";
            response.StatusCode = responseCode;
            response.Headers[HeaderNames.CacheControl] = "no-cache";
            response.Headers[HeaderNames.Pragma] = "no-cache";
            response.Headers[HeaderNames.Expires] = "-1";
            response.Headers.Remove(HeaderNames.ETag);
            await response.WriteAsync(JsonConvert.SerializeObject(errorData, Formatting.Indented, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() })).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the stack trace of exception in suitable format.
        /// </summary>
        /// <param name="exception">The exception.</param>
        private static List<string> GetStackTrace(Exception exception)
        {
            var frames = new List<string>();
            if (exception == null)
            {
                return frames;
            }

            const bool needFileInfo = true;
            var stackTrace = new System.Diagnostics.StackTrace(exception, needFileInfo);
            System.Diagnostics.StackFrame[] stackFrames = stackTrace.GetFrames();
            if (stackFrames == null)
            {
                return frames;
            }

            // Common path parts (root folder etc.) is filled in loop below - to remove them later to shorten stack trace and increase readability.
            string[] commonPathParts = Array.Empty<string>();
            for (int i = 0; i < stackFrames.Length; i++)
            {
                System.Diagnostics.StackFrame frame = stackFrames[i];
                string filename = frame.GetFileName();

                // We are interested only in own code and not in Middleware, which is THIS class (Namespace).
                if (filename?.Contains("Api.Core") != false && filename?.Contains(".Tests") != false)
                {
                    // Still include all for tests
                    if (filename == null || !filename.Contains(".Tests"))
                    {
                        continue;
                    }
                }

                string method = GetMethodSignature(frame.GetMethod()) ?? "?";
                frames.Add($"at {method} in {filename ?? "??"}: line {frame.GetFileLineNumber():D}");

                // Code to remove common part from filenames (Namespace folder)
                if (commonPathParts.Length == 0)
                {
                    commonPathParts = filename.Split('\\');
                    Array.Resize(ref commonPathParts, commonPathParts.Length - 2);
                }
                else
                {
                    if (!string.IsNullOrEmpty(filename))
                    {
                        foreach (string pathPart in filename.Split('\\'))
                        {
                            if (!commonPathParts.Contains(pathPart))
                            {
                                commonPathParts = RemoveFromArray(commonPathParts, pathPart);
                            }
                        }
                    }
                }
            }

            for (int frameIndex = 0; frameIndex < frames.Count; frameIndex++)
            {
                foreach (string pathPart in commonPathParts)
                {
                    frames[frameIndex] = frames[frameIndex].Replace(pathPart + "\\", string.Empty);
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
                return null;
            }

            Type type = method.DeclaringType;
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
                    .Select(arg => TypeNameHelper.GetTypeDisplayName(arg, fullName: false, includeGenericParameterNames: true)));
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

                Type parameterType = method.GetParameters()[i].ParameterType;
                string prefix = string.Empty;
                if (method.GetParameters()[i].IsOut)
                {
                    prefix = "out ";
                }
                else if (parameterType?.IsByRef == true)
                {
                    prefix = "ref ";
                }

                string parameterTypeString = "?";
                if (parameterType != null)
                {
                    if (parameterType.IsByRef)
                    {
                        parameterType = parameterType.GetElementType();
                    }

                    parameterTypeString = TypeNameHelper.GetTypeDisplayName(parameterType, fullName: false, includeGenericParameterNames: true);
                }

                methodName += $"{prefix}{parameterTypeString} {method.GetParameters()[i].Name}";
            }

            methodName += ")";
            return methodName;
        }

        private static MethodBase ResolveStateMachineMethod(MethodBase method)
        {
            Type declaringType = method.DeclaringType;
            MethodBase retMethod = method;
            Type parentType = method.DeclaringType.DeclaringType;
            if (parentType == null)
            {
                return retMethod;
            }

            MethodInfo[] methods = parentType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            if (methods == null)
            {
                return retMethod;
            }

            foreach (MethodInfo candidateMethod in methods)
            {
                IEnumerable<StateMachineAttribute> attributes = candidateMethod.GetCustomAttributes<StateMachineAttribute>();
                if (attributes == null)
                {
                    continue;
                }

                foreach (StateMachineAttribute sma in attributes)
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
        /// Removes all instances of [itemToRemove] from array [original]
        /// Returns the new array, without modifying [original] directly.
        /// </summary>
        /// <typeparam name="T">Type of elements in array.</typeparam>
        /// <param name="original">Array to modify.</param>
        /// <param name="itemToRemove">item (its value) to remove from array.</param>
        public static T[] RemoveFromArray<T>(T[] original, T itemToRemove)
        {
            int numIdx = Array.IndexOf(original, itemToRemove);
            if (numIdx == -1)
            {
                return original;
            }

            var tmp = new List<T>(original);
            tmp.RemoveAt(numIdx);
            return tmp.ToArray();
        }
    }
}
