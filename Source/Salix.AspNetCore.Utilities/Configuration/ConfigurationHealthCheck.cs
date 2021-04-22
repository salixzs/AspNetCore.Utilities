using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ConfigurationValidation;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Salix.AspNetCore.Utilities
{
    /// <summary>
    /// Uses <see cref="ConfigurationValidation"/> functionality to verify application configuration.
    /// </summary>
    public class ConfigurationHealthCheck : IHealthCheck
    {
        private readonly IEnumerable<IValidatableConfiguration> _configurations;
        private readonly bool _showConfigurationValues;

        /// <summary>
        /// Uses <see cref="ConfigurationValidation"/> functionality to verify application configuration.
        /// </summary>
        /// <param name="showConfigurationValues">When true - shows used configuration values as part of Health check data.</param>
        public ConfigurationHealthCheck(IEnumerable<IValidatableConfiguration> configurations, bool showConfigurationValues)
        {
            _configurations = configurations;
            _showConfigurationValues = showConfigurationValues;
        }

        /// <summary>
        /// Performs actual connection try and reports success in expected format.
        /// </summary>
        /// <param name="context">Health checking context (framework).</param>
        /// <param name="cancellationToken">Operation cancellation token.</param>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously - expected async by framework
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            var healthCheckData = new Dictionary<string, object>();
            var failures = new List<ConfigurationValidationItem>();
            foreach (IValidatableConfiguration validatableObject in _configurations)
            {
                failures.AddRange(validatableObject.Validate());
            }

            if (failures.Count == 0)
            {
                return HealthCheckResult.Healthy($"{_configurations.Count()} configuration(s) OK.", healthCheckData);
            }

            int index = 1;
            foreach (ConfigurationValidationItem failure in failures)
            {
                healthCheckData.Add(
                    $"{failure.ConfigurationSection} ({index})",
                    $"{failure.ConfigurationItem}: {failure.ValidationMessage}" +
                        (_showConfigurationValues
                            ? $" ({failure.ConfigurationValue})"
                            : string.Empty)
                );
                index++;
            }

            return new HealthCheckResult(context.Registration.FailureStatus, "Configuration validation failed.", data: healthCheckData);
        }
    }
}
