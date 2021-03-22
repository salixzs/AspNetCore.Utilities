namespace Salix.AspNetCore.Utilities.Tests
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Newtonsoft.Json;
    using Xunit;

    [ExcludeFromCodeCoverage]
    public class ApiJsonExceptionMiddlewareTests
    {
        [Fact]
        public async Task UnhandledException_WrappedFully()
        {
            // Arrange
            ApiException exc = null;
            try
            {
                // To get stack trace
                var prepared = new ApiException("Testable problem");
                throw prepared;
            }
            catch (ApiException e)
            {
                exc = e;
            }

            var logger = new Mock<ILogger<ApiJsonExceptionMiddleware>>();
            var middleware = new ApiJsonExceptionMiddleware(next: (innerHttpContext) => throw exc, logger.Object);

            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            // Act
            await middleware.Invoke(context);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(context.Response.Body);
            var streamText = reader.ReadToEnd();
            ApiError responseError = JsonConvert.DeserializeObject<ApiError>(streamText);

            // Assert
            context.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
            responseError.Should().NotBeNull();
            responseError.ErrorType.Should().Be(ApiErrorType.ServerError);
            responseError.ExceptionType.Should().Be("ApiException");
            responseError.InnerException.Should().BeNullOrEmpty();
            responseError.InnerInnerException.Should().BeNullOrEmpty();
            responseError.Status.Should().Be(500);
            responseError.Title.Should().Be("Testable problem");
            responseError.StackTrace.Should().NotBeEmpty();
            responseError.ValidationErrors.Should().BeEmpty();

            logger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public async Task UnhandledInnerException_WrappedFully()
        {
            // Arrange
            ApiException exc = null;
            try
            {
                // To get stack trace
                var prepared = new ApiException("Testable problem", new Exception("Goin' deeper"));
                throw prepared;
            }
            catch (ApiException e)
            {
                exc = e;
            }

            var logger = new Mock<ILogger<ApiJsonExceptionMiddleware>>();
            var middleware = new ApiJsonExceptionMiddleware(next: (innerHttpContext) => throw exc, logger.Object);

            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            // Act
            await middleware.Invoke(context);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(context.Response.Body);
            var streamText = reader.ReadToEnd();
            ApiError responseError = JsonConvert.DeserializeObject<ApiError>(streamText);

            // Assert
            responseError.InnerException.Should().Be("Goin' deeper");
            responseError.InnerInnerException.Should().BeNullOrEmpty();
        }

        [Fact]
        public async Task UnhandledInnerInnerException_IsShown()
        {
            // Arrange
            ApiException exc = null;
            try
            {
                // To get stack trace
                var prepared = new ApiException("Testable problem", new Exception("Goin' deeper", new Exception("Very deep")));
                throw prepared;
            }
            catch (ApiException e)
            {
                exc = e;
            }

            var logger = new Mock<ILogger<ApiJsonExceptionMiddleware>>();
            var middleware = new ApiJsonExceptionMiddleware(next: (innerHttpContext) => throw exc, logger.Object);

            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            // Act
            await middleware.Invoke(context);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(context.Response.Body);
            var streamText = reader.ReadToEnd();
            ApiError responseError = JsonConvert.DeserializeObject<ApiError>(streamText);

            // Assert
            context.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
            responseError.Should().NotBeNull();
            responseError.InnerException.Should().Be("Goin' deeper");
            responseError.InnerInnerException.Should().Be("Very deep");
        }

        [Fact]
        public async Task UnhandledInnerInnerException_VeryDeep()
        {
            // Arrange
            ApiException exc = null;
            try
            {
                // To get stack trace
                var prepared = new ApiException("Testable problem", new Exception("Inner", new Exception("1", new Exception("2", new Exception("3", new Exception("4"))))));
                throw prepared;
            }
            catch (ApiException e)
            {
                exc = e;
            }

            var logger = new Mock<ILogger<ApiJsonExceptionMiddleware>>();
            var middleware = new ApiJsonExceptionMiddleware(next: (innerHttpContext) => throw exc, logger.Object);

            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            // Act
            await middleware.Invoke(context);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(context.Response.Body);
            var streamText = reader.ReadToEnd();
            ApiError responseError = JsonConvert.DeserializeObject<ApiError>(streamText);

            // Assert
            context.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
            responseError.Should().NotBeNull();
            responseError.InnerException.Should().Be("Inner");
            responseError.InnerInnerException.Should().Be("1; 2; 3; 4");
        }
    }
}
