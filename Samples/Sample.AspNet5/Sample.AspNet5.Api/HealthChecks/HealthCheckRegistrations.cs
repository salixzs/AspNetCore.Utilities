using System;
using System.Collections.Generic;
using ConfigurationValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Salix.AspNetCore.Utilities;

namespace Sample.AspNet5.Api.HealthChecks
{
    public static class HealthCheckRegistrations
    {
        private const string HealthHeckEndpoint = "/api/health";

        /// <summary>
        /// Registers HealthChecking with IoC (services).
        /// Used for easing up Startup.cs methods.
        /// </summary>
        /// <param name="services">ASP.Net IoC container (services).</param>
        /// <param name="isDevelopment">Flag, indicating whether API runs in developer mode - can add more infromation to health checks.</param>
        public static void AddApiHealthChecks(this IServiceCollection services, bool isDevelopment) =>
            services.AddHealthChecks()
                .Add(new HealthCheckRegistration("Configuration", sp => new ConfigurationHealthCheck(sp.GetServices<IValidatableConfiguration>(), isDevelopment), HealthStatus.Unhealthy, null, TimeSpan.FromSeconds(3)))
                .Add(new HealthCheckRegistration("Database", sp => new DummyDatabaseHealthCheck(isDevelopment), HealthStatus.Unhealthy, null, TimeSpan.FromSeconds(10)))
                .Add(new HealthCheckRegistration("ExtApi", sp => new DummyExternalApiHealthCheck(isDevelopment), HealthStatus.Unhealthy, null, TimeSpan.FromSeconds(5)));

        /// <summary>
        /// Provides formatted Json response for health check endpoint
        /// </summary>
        /// <param name="app">Application Builder object (from Startup).</param>
        /// <param name="isDevelopment">Flag, indicating whether API runs in developer mode - can add more infromation to health checks.</param>
        public static IApplicationBuilder UseApiHealthChecks(this IApplicationBuilder app, bool isDevelopment)
        {
            HealthCheckOptions healthCheckOptions = FormatHealthCheckResponse(isDevelopment);
            return app.UseHealthChecks(HealthHeckEndpoint, healthCheckOptions);
        }

        /// <summary>
        /// Provides Formatting options to the health check response.
        /// Creates JSON object with plenty of information for DEV and necessary for other environments.
        /// </summary>
        /// <param name="isDevelopment">Flag to indicate application is running in development environment.</param>
        private static HealthCheckOptions FormatHealthCheckResponse(bool isDevelopment)
        {
            var opts = new HealthCheckOptions
            {
                ResultStatusCodes = new Dictionary<HealthStatus, int>
                {
                    { HealthStatus.Healthy, StatusCodes.Status200OK },
                    { HealthStatus.Degraded, StatusCodes.Status503ServiceUnavailable },
                    { HealthStatus.Unhealthy, StatusCodes.Status503ServiceUnavailable },
                },
                ResponseWriter = async (context, report) =>
                        await HealthCheckFormatter.JsonResponseWriter(context, report, isDevelopment)
                    .ConfigureAwait(false),
            };
            return opts;
        }
    }
}
