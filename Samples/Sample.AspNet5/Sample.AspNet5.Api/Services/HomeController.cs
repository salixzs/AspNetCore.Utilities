using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Salix.AspNetCore.Utilities;
using Sample.AspNet5.Logic;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace Sample.AspNet5.Api.Services
{
    /// <summary>
    /// Frontend page show functionality to avoid having 404 page does not exist.
    /// Also giving nicer output of some description.
    /// </summary>
    /// <remarks>
    /// Should put [ApiExplorerSettings(IgnoreApi = true)] attribute to filter this out from Swagger, is used.
    /// </remarks>
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly ISampleLogic _logic;
        private readonly HealthCheckService _healthChecks;
        private readonly ILogger<HomeController> _logger;
        private const string HealthTestEndpoint = "/api/healthtest";

        /// <summary>
        /// Home controller for two pages, available from Utilities.
        /// </summary>
        /// <param name="hostingEnvironment">Hosting environment.</param>
        /// <param name="logic">Demonstration business logic (throwing errors).</param>
        /// <param name="healthChecks">ASP.Net built in health checking services. DO NOT INJECT this, if you do not have Health checks configured in API.</param>
        /// <param name="logger">Logging object.</param>
        public HomeController(IWebHostEnvironment hostingEnvironment, ISampleLogic logic, HealthCheckService healthChecks, ILogger<HomeController> logger)
        {
            _hostingEnvironment = hostingEnvironment;
            _logic = logic;
            _healthChecks = healthChecks;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves simple frontend/index page to display when API is open on its base URL.
        /// </summary>
        [HttpGet("/")]
        public ContentResult Index()
        {
            var apiAssembly = Assembly.GetAssembly(typeof(Startup));
            IndexPageValues buildData = new()
            {
                ApiName = "Sample API",
                Description = "Demonstrating capabilities of Salix.AspNetCore.Utilities NuGet package.",
                HostingEnvironment = _hostingEnvironment.EnvironmentName,
                Version = IndexPage.ExtractVersionFromAssembly(apiAssembly, 2), // Takes version from asssembly - just first two numbers as specified
                BuiltTime = IndexPage.ExtractBuildTimeFromAssembly(apiAssembly), // For this to work need non-deterministic AssemblyInfo.cs version set.
                HealthPageAddress = HealthTestEndpoint, // See operation URL set on action method below!
            };

// "Hacking" to understand what mode API is getting compiled.
#if DEBUG
            buildData.BuildMode = "#DEBUG (Should not be in production!)";
#else
            buildData.BuildMode = "Release";
#endif

            return new ContentResult
            {
                ContentType = "text/html",
                StatusCode = (int)HttpStatusCode.OK,
                Content = IndexPage.GetContents(buildData),
            };
        }

        /// <summary>
        /// Displays separate Health status page, extracted from standard Health Check report.
        /// Added few testing links to showcase possibility to add custom links for API testing.
        /// </summary>
        [HttpGet(HealthTestEndpoint)]
        public async Task<ContentResult> HealthTest()
        {
            HealthReport healthResult = await _healthChecks.CheckHealthAsync().ConfigureAwait(false);
            return new ContentResult
            {
                ContentType = "text/html",
                StatusCode = (int)HttpStatusCode.OK,
                Content = HealthTestPage.GetContents(
                    healthResult, 
                    "/api/health",
                    new List<HealthTestPageLink>
                    {
                        new HealthTestPageLink { TestEndpoint = "/api/sample/exception", Name = "Exception", Description = "Throws dummy exception/error to check Json Error functionality." },
                        new HealthTestPageLink { TestEndpoint = "/api/sample/validation", Name = "Validation Error", Description = "Throws dummy data validation exception to check Json Error functionality for data validation." },
                        new HealthTestPageLink { TestEndpoint = "/api/sample/db", Name = "DB Exception", Description = "Throws dummy database exception to check Json Error custom exception handling." },
                        new HealthTestPageLink { TestEndpoint = "/api/sample/notyet", Name = "Not yet!", Description = "Showcase Json handler work on NotImplementedException." },
                        new HealthTestPageLink { TestEndpoint = "/api/sample/anytest", Name = "DateTime", Description = "Showcase some custom API testing endpont returning some data." },
                    }),
            };
        }

        // BELOW are custom endpoints for testing, not something necessary normally for using Utilities.

        [HttpGet("/api/sample/exception")]
        public async Task ThrowException()
        {
            _logger.LogInformation("Within API controller, about to call faulty business logic.");
            await _logic.FaultyLogic();
        }

        [HttpGet("/api/sample/validation")]
        public async Task ThrowValidationException()
        {
            _logger.LogInformation("Within API controller, about to call validations.");
            await _logic.ValidationError();
        }

        [HttpGet("/api/sample/db")]
        public async Task ThrowDatabaseException()
        {
            _logger.LogInformation("Within API controller, about to call \"storage/database\".");
            await _logic.DatabaseProlemLogic();
        }

        [HttpGet("/api/sample/notyet")]
        public void NotImplemented() => throw new System.NotImplementedException("Just a demonstration of not implemented stuff.");

        /// <summary>
        /// Could be any custom testing endpoint to validate how API works.
        /// </summary>
        [HttpGet("/api/sample/anytest")]
        public DateTime GetData() => DateTime.Now;
    }
}
