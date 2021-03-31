using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Salix.AspNetCore.Utilities;
using Sample.AspNet5.Api.HealthChecks;
using Sample.AspNet5.Api.Middleware;
using Sample.AspNet5.Logic;

namespace Sample.AspNet5.Api
{
    /// <summary>
    /// ASP.Net Startup class to configure API before its launching.
    /// </summary>
    public class Startup
    {
        private readonly IWebHostEnvironment _environment;

        /// <summary>
        /// Whole loaded configuration from Program.cs ConfigureWebHostDefaults method of Host builder, passed via constructor.
        /// </summary>
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        /// <summary>
        /// Configures used services with Asp.Net IoC container.
        /// </summary>
        /// <param name="services">The services (IoC container).</param>
        public void ConfigureServices(IServiceCollection services)
        {
            // ---- Uncomment line below to validate configuration as part of application startup (can result in 500 server error) ----
            // services.AddConfigurationValidation();

            services.ConfigureValidatableSetting<SampleLogicConfig>(_configuration.GetSection("LogicConfiguration"));

            // For API - only Controllers needed - "Services" folder.
            // Setting Json serializer to satisfy JS frontends.
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.IgnoreNullValues = false; // All fields should present, even null valued
                    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase; // e.g. camelCase. To use PascalCase, set it to null
                    options.JsonSerializerOptions.WriteIndented = false; // compact json
                    options.JsonSerializerOptions.PropertyNameCaseInsensitive = false;
                    options.JsonSerializerOptions.AllowTrailingCommas = false;
                });

            services.RegisterLogicDependencies(); // Taking own dependencies registration outta startup.
            services.AddApiHealthChecks(_environment.IsDevelopment()); // See custom registration class in "HealthChecks" folder.
        }

        /// <summary>
        /// Configures API for launching.
        /// </summary>
        /// <param name="app">The Application (API) builder.</param>
        /// <param name="env">The hosting environment.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseConfigurationValidationErrorPage(); // Ends up in Yellow Screen of Death if configuration is invalid. Comment out to leave just HealthCheck.
            app.UseJsonErrorHandler(env.IsDevelopment()); // This registers your custom middleware of JSON Error, based on provided in package.
            app.UseRouting();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
            app.UseApiHealthChecks(env.IsDevelopment()); // See custom registration class in "HealthChecks" folder.
        }
    }
}
