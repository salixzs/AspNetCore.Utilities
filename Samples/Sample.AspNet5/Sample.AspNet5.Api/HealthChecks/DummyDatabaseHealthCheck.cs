using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Sample.AspNet5.Api.HealthChecks
{
    /// <summary>
    /// Provides means of checking whether database connection is OK and is it a correct database.
    /// </summary>
    public class DummyDatabaseHealthCheck : IHealthCheck
    {
        private readonly bool _showConnectionString;

        /// <summary>
        /// Provides means of checking whether database connection is OK and is it a correct database.
        /// </summary>
        /// <param name="showConnectionString">When true - shows connection string as Health ckeck data.</param>
        /// <remarks>Normally inject helth check routine/class and use it to actually check connection.</remarks>
        public DummyDatabaseHealthCheck(bool showConnectionString) => _showConnectionString = showConnectionString;

        /// <summary>
        /// Performs actual connection try and reports success in expected format.
        /// </summary>
        /// <param name="context">Health checking context (framework).</param>
        /// <param name="cancellationToken">Operation cancellation token.</param>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously - expected by framework
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            var healthCheckData = new Dictionary<string, object>();
            if (_showConnectionString)
            {
                healthCheckData.Add("ConnString", "database connection string");
            };

            // TODO: Here you would want to call some things to actually check database connection.

            return HealthCheckResult.Healthy("Database is OK.", healthCheckData);
        }
    }
}
