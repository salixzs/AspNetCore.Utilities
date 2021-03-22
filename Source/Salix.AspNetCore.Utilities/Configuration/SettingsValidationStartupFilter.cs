namespace Salix.AspNetCore.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;

    /// <summary>
    /// An <see cref="IStartupFilter"/> that validates <see cref="IValidatableConfiguration"/> objects are valid on app startup.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class SettingsValidationStartupFilter : IStartupFilter
    {
        private readonly IEnumerable<IValidatableConfiguration> _validatableConfigs;

        /// <summary>
        /// An <see cref="IStartupFilter"/> that validates <see cref="IValidatableConfiguration"/> objects are valid on app startup.
        /// </summary>
        /// <param name="validatableConfigurations">Configuration objects to get validated.</param>
        public SettingsValidationStartupFilter(IEnumerable<IValidatableConfiguration> validatableConfigurations)
            => _validatableConfigs = validatableConfigurations;

        /// <summary>
        /// Actually does the intended Job.
        /// </summary>
        /// <param name="next">The next handler in chain.</param>
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            foreach (IValidatableConfiguration validatableObject in _validatableConfigs)
            {
                validatableObject.Validate();
            }

            // Don't alter the configuration !!!
            return next;
        }

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => $"{_validatableConfigs.ToList().Count.ToString("D")} config sections";
    }
}
