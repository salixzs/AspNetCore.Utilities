using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Salix.AspNetCore.Utilities;
using Sample.AspNet5.Logic;
using System;
using System.Linq;

namespace Sample.AspNet5.Api.Middleware
{
    /// <summary>
    /// Own middleware with custom special exception handling, added to provided base middleware class.
    /// </summary>
    public class ApiJsonErrorMiddleware : ApiJsonExceptionMiddleware
    {
        public ApiJsonErrorMiddleware(RequestDelegate next, ILogger<ApiJsonExceptionMiddleware> logger, bool showStackTrace) : base(next, logger, showStackTrace)
        {
        }

        /// <summary>
        /// This method is called from base class handler to add more information to Json Error object.
        /// Here all special exception types should be handled, so API Json Error returns appropriate data.
        /// </summary>
        /// <param name="apiError">ApiError object, which gets returned from API in case of exception/error. Provided by </param>
        /// <param name="exception">Exception which got bubbled up from somewhere deep in API logic.</param>
        protected override ApiError HandleSpecialException(ApiError apiError, Exception exception)
        {
            // When using FluentValidation, could use also handler for its ValidationException in stead of this custom
            if (exception is SampleDataValidationException validationException)
            {
                apiError.Status = 400;
                apiError.ErrorType = ApiErrorType.DataValidationError;
                apiError.ValidationErrors
                    .AddRange(
                        validationException.ValidationErrors.Select(failure =>
                            new ApiDataValidationError
                            {
                                Message = failure.ValidationMessage,
                                PropertyName = failure.PropertyName,
                                AttemptedValue = failure.AppliedValue
                            }));
            }

            if (exception is AccessViolationException securityException)
            {
                apiError.Status = 401; // or 403
                apiError.ErrorType = ApiErrorType.AccessRestrictedError;
            }

            if (exception is SampleDatabaseException dbException)
            {
                apiError.Status = 500;
                if (dbException.ErrorType == DatabaseProblemType.WrongSyntax)
                apiError.ErrorType = ApiErrorType.StorageError;
            }

            if (exception is NotImplementedException noImplemented)
            {
                apiError.Status = 501;
                apiError.Title = "Functionality is not yet implemented.";
            }

            return apiError;
        }
    }
}
