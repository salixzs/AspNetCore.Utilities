using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Sample.AspNet5.Api.HealthChecks
{
    /// <summary>
    /// Provides means of checking whether External API connection is OK.
    /// </summary>
    public class DummyExternalApiHealthCheck : IHealthCheck
    {
        private readonly bool _showAuthenticationData;

        /// <summary>
        /// Provides means of checking whether External API connection is OK.
        /// </summary>
        /// <param name="showConnectionString">When true - shows authentication data as Health ckeck data.</param>
        /// <remarks>Normally inject helth check routine/class and use it to actually check connection.</remarks>
        public DummyExternalApiHealthCheck(bool showAuthenticationData) => _showAuthenticationData = showAuthenticationData;

        /// <summary>
        /// Performs actual connection try and reports success in expected format.
        /// </summary>
        /// <param name="context">Health checking context (framework).</param>
        /// <param name="cancellationToken">Operation cancellation token.</param>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously - expected by framework
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            var healthCheckData = new Dictionary<string, object>
            {
                { "ExtApi URL", "https://extapi.com/api" },
            };

            if (_showAuthenticationData)
            {
                healthCheckData.Add("User", "username from config");
                healthCheckData.Add("Password", "password from config");
                healthCheckData.Add("Token", "Secret token from config");
            };

            // TODO: Here you would want to call some things to actually check external api connection.

            return HealthCheckResult.Healthy("ExtAPI is OK.", healthCheckData);
        }
    }
}
