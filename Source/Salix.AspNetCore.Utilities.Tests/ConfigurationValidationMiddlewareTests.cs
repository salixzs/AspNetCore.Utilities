using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using ConfigurationValidation;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Salix.AspNetCore.Utilities.Tests
{
    [ExcludeFromCodeCoverage]
    public class ConfigurationValidationMiddlewareTests
    {
        [Fact]
        public async Task NoConfigurations_NoResult()
        {
            var logger = new Mock<ILogger<ConfigurationValidationMiddleware>>();
            var configs = new List<IValidatableConfiguration>();
            var middleware = new ConfigurationValidationMiddleware(next: (innerHttpContext) => Task.CompletedTask, configs, logger.Object);

            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            // Act
            await middleware.Invoke(context);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(context.Response.Body);
            string responseString = reader.ReadToEnd();

            responseString.Should().BeEmpty();
            logger.Verify(x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Exactly(2));
        }

        [Fact]
        public async Task Configurations_NoProblems_NoResult()
        {
            var logger = new Mock<ILogger<ConfigurationValidationMiddleware>>();
            var configs = new List<IValidatableConfiguration> { new TestValidConfig() };
            var middleware = new ConfigurationValidationMiddleware(next: (innerHttpContext) => Task.CompletedTask, configs, logger.Object);

            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            // Act
            await middleware.Invoke(context);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(context.Response.Body);
            string responseString = reader.ReadToEnd();

            responseString.Should().BeEmpty();
            logger.Verify(x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Exactly(2));
        }

        [Fact]
        public async Task Configurations_WithProblems_ReturnsPage()
        {
            var logger = new Mock<ILogger<ConfigurationValidationMiddleware>>();
            var configs = new List<IValidatableConfiguration> { new TestInvalidConfig() };
            var middleware = new ConfigurationValidationMiddleware(next: (innerHttpContext) => Task.CompletedTask, configs, logger.Object);

            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            // Act
            await middleware.Invoke(context);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(context.Response.Body);
            string responseString = reader.ReadToEnd();

            responseString.Should().NotBeEmpty();
            responseString.Should().StartWith("<!DOCTYPE html>");
            responseString.Should().Contain("<td>Database connection is invalid.</td>");
            logger.Verify(x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Exactly(1));
            logger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Exactly(2));
        }
    }

    public class TestValidConfig : IValidatableConfiguration
    {
        public IEnumerable<ConfigurationValidationItem> Validate() => new List<ConfigurationValidationItem>();
    }
    public class TestInvalidConfig : IValidatableConfiguration
    {
        public IEnumerable<ConfigurationValidationItem> Validate() =>
            new List<ConfigurationValidationItem>
            {
                new ConfigurationValidationItem("DB", "Conn", "Incorrect", "Database connection is invalid.")
            };
    }
}
