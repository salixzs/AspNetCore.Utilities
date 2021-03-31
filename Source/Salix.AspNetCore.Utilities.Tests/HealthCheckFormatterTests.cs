using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;
using Xunit;

namespace Salix.AspNetCore.Utilities.Tests
{
    [ExcludeFromCodeCoverage]
    public class HealthCheckFormatterTests
    {
        [Fact]
        public void JsonResponseWriter_StandardReport_AsExpected()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream();

            HealthCheckFormatter.JsonResponseWriter(httpContext, CreateHealthReport(false), false);
            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(httpContext.Response.Body);
            string streamText = reader.ReadToEnd();
            streamText.Should().NotBeNullOrEmpty();
            httpContext.Response.ContentType.Should().Be("application/json");

            ApiHealth errObj = JsonConvert.DeserializeObject<ApiHealth>(streamText);
            errObj.Should().NotBeNull();
            errObj.Status.Should().Be("Degraded");
            errObj.Checks.Should().NotBeEmpty();
            errObj.Checks.Should().HaveCount(1);
            errObj.Checks[0].Key.Should().Be("db");
            errObj.Checks[0].Status.Should().Be("Degraded");
            errObj.Checks[0].Description.Should().Be("Slow");
            errObj.Checks[0].Exception.Should().BeNull();
            errObj.Checks[0].Data.Should().NotBeEmpty();
            errObj.Checks[0].Data.Should().HaveCount(1);
            errObj.Checks[0].Data[0].Key.Should().Be("Oracle");
            errObj.Checks[0].Data[0].Value.Should().Be("Sux");
        }

        [Fact]
        public void JsonResponseWriter_ExceptionDev_ShowsStackTrace()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream();

            HealthCheckFormatter.JsonResponseWriter(httpContext, CreateHealthReport(true), true);
            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(httpContext.Response.Body);
            string streamText = reader.ReadToEnd();
            streamText.Should().NotBeNullOrEmpty();
            ApiHealth errObj = JsonConvert.DeserializeObject<ApiHealth>(streamText);
            errObj.Should().NotBeNull();
            errObj.Checks.Should().NotBeEmpty();
            errObj.Checks.Should().HaveCount(1);
            errObj.Checks[0].Exception.Should().NotBeNull();
            errObj.Checks[0].Exception.Message.Should().Be("Testable problem");
            errObj.Checks[0].Exception.Type.Should().Be("ApplicationException");
            errObj.Checks[0].Exception.InnerException.Should().Be("Goin' deeper");
            errObj.Checks[0].Exception.StackTrace.Should().NotBeNullOrEmpty();
            errObj.Checks[0].Exception.StackTrace.Should().Contain("CreateHealthReport(Boolean addException)");
        }

        [Fact]
        public void JsonResponseWriter_ExceptionProd_HidesStackTrace()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Response.Body = new MemoryStream();

            HealthCheckFormatter.JsonResponseWriter(httpContext, CreateHealthReport(true), false);
            httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(httpContext.Response.Body);
            string streamText = reader.ReadToEnd();
            streamText.Should().NotBeNullOrEmpty();
            ApiHealth errObj = JsonConvert.DeserializeObject<ApiHealth>(streamText);
            errObj.Should().NotBeNull();
            errObj.Checks.Should().NotBeEmpty();
            errObj.Checks.Should().HaveCount(1);
            errObj.Checks[0].Exception.Should().NotBeNull();
            errObj.Checks[0].Exception.Message.Should().Be("Testable problem");
            errObj.Checks[0].Exception.Type.Should().Be("ApplicationException");
            errObj.Checks[0].Exception.InnerException.Should().Be("Goin' deeper");
            errObj.Checks[0].Exception.StackTrace.Should().BeNullOrEmpty();
        }

        private static HealthReport CreateHealthReport(bool addException)
        {
            ApplicationException exc = null;
            if (addException)
            {
                try
                {
                    // To get stack trace
                    throw new ApplicationException("Testable problem", new Exception("Goin' deeper"));
                }
                catch (ApplicationException e)
                {
                    exc = e;
                }
            }

            return new HealthReport(
                new Dictionary<string, HealthReportEntry>
                {
                    {
                        "db",
                        new HealthReportEntry(
                            HealthStatus.Degraded,
                            "Slow",
                            TimeSpan.FromMilliseconds(2889),
                            exc,
                            new Dictionary<string, object>
                            {
                                { "Oracle", "Sux" }
                            })
                    }
                },
                TimeSpan.FromMilliseconds(3757));
        }
    }

    // Helper classes to deserialize Health check result
    public class ApiHealth
    {
        public string Status { get; set; }
        public List<ApiHealthCheck> Checks { get; set; } = new List<ApiHealthCheck>();
    }

    public class ApiHealthCheck
    {
        public string Key { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public ApiExceptionData Exception { get; set; }
        public List<KeyValuePair<string, object>> Data { get; set; } = new List<KeyValuePair<string, object>>();
    }

    public class ApiExceptionData
    {
        public string Message { get; set; }
        public string Type { get; set; }
        public string InnerException { get; set; }
        public string StackTrace { get; set; }
    }
}
