# Global error handler
When API throws unhandled exception, it usually is bubbled up to some global error handler. Microsoft templates adds these lines for development-time error handling (as page):

```csharp
if (env.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
```

but in production it  leaves with checking Http status code (400-500) without any additional information on what happened on server side in API.

Here comes utility functionality in package to fix this problem by returning Json structure along with this 400/500 status code, which can be parsed at consumer side to get more information on problem right away.

## Usage
When package is added to API project, `public abstract class ApiJsonExceptionMiddleware` becomes available to override with your own simplified middleware class, using this as base for its implementation. In essence, if you do not need special exception type handling it can be completely empty and should work, except it will return all exceptions as 500 (Server error), but including all information as much as possible:

```csharp
/// <summary>
/// Own middleware with provided base middleware class.
/// </summary>
public class ApiJsonErrorMiddleware : ApiJsonExceptionMiddleware
{
    // use either this simplified constructor
    public ApiJsonErrorMiddleware(RequestDelegate next, ILogger<ApiJsonExceptionMiddleware> logger, bool showStackTrace)
        : base(next, logger, showStackTrace)
    {
    }
    
    // or use this constructor to supply extended options
    public ApiJsonErrorMiddleware(RequestDelegate next, ILogger<ApiJsonExceptionMiddleware> logger, ApiJsonExceptionOptions options)
        : base(next, logger, options)
    {
    }
}
```

After it is created, you can register it in API Startup.cs `Configure` method like this (somewhere in the very beginning of this method):

```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    app.UseMiddleware<ApiJsonErrorMiddleware>(env.IsDevelopment()); // simple constructor
    // ... everything else
}
```

The only parameter in simple constructor controls whether StackTrace is shown to consumer. In example above we control it by environment variable and then it is shown during API development, but hidden in any other environment. If you put constant true/false in stead - it is either shown always or hidden always.

For options - you can set the same `showStackTrace` boolean and also specify list of stack trace frames to be filtered out from being shown. It is `OmitSources` property, containing list (HashSet) of strings, which should not be a part of file path in stack trace frame.
For example, if you set it to `new HashSet<string> { "middleware" }`, it will filter out all middleware components (given they have string "middleware" in their file name or in path).

### Custom exception handling
If you want to handle (return data on) some specific exceptions, then you should override `HandleSpecialException` method from base class. There you can check whether exception is of this special type and modify returned Json data structure accordingly:

```csharp
/// <summary>
/// This method is called from base class handler to add more information to Json Error object.
/// Here all special exception types should be handled, so API Json Error returns appropriate data.
/// </summary>
/// <param name="apiError">ApiError object, which gets returned from API in case of exception/error. Provided by </param>
/// <param name="exception">Exception which got bubbled up from somewhere deep in API logic.</param>
protected override ApiError HandleSpecialException(ApiError apiError, Exception exception)
{
    // When using FluentValidation, could use also handler for its ValidationException in stead of this custom one
    if (exception is SampleDataValidationException validationException)
    {
        apiError.Status = 400; // or 422
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
        {
            apiError.ErrorType = ApiErrorType.StorageError;
        }
    }

    if (exception is NotImplementedException noImplemented)
    {
        apiError.Status = 501;
        apiError.Title = "Functionality is not yet implemented.";
    }
    
    if (exception is OperationCanceledException operationCanceledException)
    {
        // This returns empty (200) response and does not log error.
        apiError.ErrorBehavior = ApiErrorBehavior.Ignore;
    }

    return apiError;
}
```


In case of data validation exceptions, when they are handled fully (as shown in example above), Json property `validationErrors` is provided:

```json
{
    "type": "DataValidationError",
    "title": "There are validation errors.",
    "status": 400,
    "requestedUrl": "/api/sample/validation",
    "errorType": 3,
    "exceptionType": "SampleDataValidationException",
    "innerException": null,
    "innerInnerException": null,
    "stackTrace": [
        "at ValidationError() in Sample.AspNet5.Logic\\SampleLogic.cs: line 50",
        "at ThrowValidationException() in Sample.AspNet5.Api\\Services\\HomeController.cs: line 117",
        "at Invoke(HttpContext httpContext) in Source\\Salix.ExceptionHandling\\ApiJsonExceptionMiddleware.cs: line 56"
    ],
    "validationErrors": [
        {
            "propertyName": "Name",
            "attemptedValue": "",
            "message": "Missing/Empty"
        },
        {
            "propertyName": "Id",
            "attemptedValue": null,
            "message": "Cannot be null"
        },
        {
            "propertyName": "Description",
            "attemptedValue": "Lorem Ipsum very long...",
            "message": "Text is too long"
        },
        {
            "propertyName": "Birthday",
            "attemptedValue": "2054-06-22T23:55:26.1708087+03:00",
            "message": "Cannot be in future"
        }
    ]
}
```

## Behavior control
By default Json error handler will write exception to configured `ILogger` instance (you control where and how it writes - AppInsights, File, Debug, Console etc.)\
and also creates Json error response and returns it to caller with specified HttpStatus code (400+, 500+).

If you use custom exception handler method, you can intercept specific exceptions and make error handler do not write an error statement to `ILogger` and/or return Json error object at all (returns 200 status code with empty response).

To control it, in specific exception handling method, intercept your special exception and set `ApiError` object property `ErrorBehavior` to desired behavior.

```csharp
if (exception is OperationCanceledException operationCanceledException)
{
    // This returns empty (200) response and does not log error.
    apiError.ErrorBehavior = ApiErrorBehavior.Ignore;
}

if (exception is TaskCanceledException taskCanceledException)
{
    // This does not log error, but still returns Json error.
    apiError.ErrorBehavior = ApiErrorBehavior.RespondWithError;
    apiError.Status = (int)HttpStatusCode.UnprocessableEntity; // or other by your design
    apiError.ErrorType = ApiErrorType.CancelledOperation;
}
```

It could come handy to ignore user canceled operations when using async code with CancellationToken.

#### That's basically it. Happy error handling!