using System.Net;
using System.Reflection;
using BusinessLogic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Salix.AspNetCore.Utilities;

namespace SampleApi.Controllers
{
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class HomeController : ControllerBase
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly ISampleLogic _logic;
        private readonly HealthCheckService _healthChecks;
        private readonly ILogger<HomeController> _logger;
        private readonly IConfigurationValuesLoader _configLoader;

        public HomeController(IWebHostEnvironment hostingEnvironment, ISampleLogic logic, HealthCheckService healthChecks, ILogger<HomeController> logger, IConfigurationValuesLoader configLoader)
        {
            _hostingEnvironment = hostingEnvironment;
            _logic = logic;
            _healthChecks = healthChecks;
            _logger = logger;
            _configLoader = configLoader;
        }

        /// <summary>
        /// Retrieves simple frontend/index page to display when API is open on its base URL.
        /// </summary>
        [HttpGet("/")]
        public ContentResult Index()
        {
            // Load filtered configuration items from entire configuration based on given whitelist filter
            var configurationItems =
                _configLoader.GetConfigurationValues(new HashSet<string>
                {
                    "AllowedHosts", "contentRoot", "Logging/LogLevel", "LogicConfiguration", "DatabaseConnection"
                });

            // #if !DEBUG <--- Do that only when running not in DEBUG mode
            var obfuscatedConfig = ObfuscateConfigurationValues(configurationItems);
            // #endif

            var apiAssembly = Assembly.GetAssembly(typeof(HomeController));
            var indexPage = new IndexPage("Sample API")
                .SetDescription("Demonstrating capabilities of Salix.AspNetCore.Utilities NuGet package.")
                .SetHostingEnvironment(_hostingEnvironment.EnvironmentName)
                .SetVersionFromAssembly(apiAssembly, 2) // Takes version from assembly - just first two numbers as specified
                .SetBuildTimeFromAssembly(apiAssembly)  // For this to work need non-deterministic AssemblyInfo.cs version set.
                .AddLinkButton("Health/Test", "/health/status")   // See operation URL set on action method below!
                .AddLinkButton("Swagger", "/swagger")
                .SetConfigurationValues(obfuscatedConfig)
                .IncludeContentFile("build_data.html");

            // "Hacking" to understand what mode API is getting compiled.
#if DEBUG
            indexPage.SetBuildMode("#DEBUG (Should not be in production!)");
#else
            indexPage.SetBuildMode("Release");
#endif
            return new ContentResult
            {
                ContentType = "text/html",
                StatusCode = (int)HttpStatusCode.OK,
                Content = indexPage.GetContents(),
            };
        }

        /// <summary>
        /// Obfuscates the configuration values for safe showing on index page.
        /// </summary>
        /// <param name="configurationItems">The loaded original configuration items.</param>
        /// <returns>Same list of configuration values, where selected are obfuscated.</returns>
        private static Dictionary<string, string> ObfuscateConfigurationValues(Dictionary<string, string> configurationItems)
        {
            var obfuscated = new Dictionary<string, string>();
            foreach (var original in configurationItems)
            {
                if (original.Key.StartsWith("LogicConfiguration/SomeIp", StringComparison.OrdinalIgnoreCase)
                    || original.Key.StartsWith("LogicConfiguration/SomeEmail", StringComparison.OrdinalIgnoreCase)
                    || original.Key.StartsWith("LogicConfiguration/SomeArray[1].Id", StringComparison.OrdinalIgnoreCase))
                {
                    obfuscated.Add(original.Key, original.Value.HideValuePartially());
                    continue;
                }

                if (original.Key.StartsWith("DatabaseConnection", StringComparison.OrdinalIgnoreCase))
                {
                    obfuscated.Add(original.Key, original.Value.ObfuscateSqlConnectionString(true));
                    continue;
                }

                obfuscated.Add(original.Key, original.Value);
            }
            return obfuscated;
        }

        /// <summary>
        /// Displays separate Health status page, extracted from standard Health Check report.
        /// Added few testing links to showcase possibility to add custom links for API testing.
        /// </summary>
        [HttpGet("/health/status")]
        public async Task<ContentResult> HealthTest()
        {
            var healthResult = await _healthChecks.CheckHealthAsync().ConfigureAwait(false);
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
            await _logic.DatabaseProblemLogic();
        }

        [HttpGet("/api/sample/notyet")]
        public void NotImplemented() => throw new NotImplementedException("Just a demonstration of not implemented stuff.");

        /// <summary>
        /// Could be any custom testing endpoint to validate how API works.
        /// </summary>
        [HttpGet("/api/sample/anytest")]
        public DateTime GetData() => DateTime.Now;
    }
}
