using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Sample.AspNet5.Api
{
    /// <summary>
    /// Entry point of API during boot-up.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Defines the entry point for API.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        public static int Main(string[] args)
        {
            // Normally should use Serilog for extensibility.
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddFilter("Sample.AspNet5.Program", LogLevel.Debug)
                    .AddDebug()
                    .AddConsole();
            });
            var logger = loggerFactory.CreateLogger<Program>();

            logger.LogInformation("Starting up API.");
            var host = CreateHostBuilder(args).Build();
            logger.LogInformation("Startup finalized. Launching...");
            host.Run();
            logger.LogInformation("API stopped cleanly.");
            return 0;
        }

        /// <summary>
        /// Creates the host builder object.
        /// </summary>
        /// <param name="args">The arguments from command line.</param>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => webBuilder
                    .UseStartup<Startup>()
                    .CaptureStartupErrors(true));
    }
}
