using ConfigurationValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace Salix.AspNetCore.Utilities;

/// <summary>
/// An <see cref="IStartupFilter"/> that validates <see cref="IValidatableConfiguration"/> objects are valid on app startup.
/// </summary>
[System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
public class ConfigurationValidationStartupFilter : IStartupFilter
{
    private readonly IEnumerable<IValidatableConfiguration> _validatableConfigs;

    /// <summary>
    /// An <see cref="IStartupFilter"/> that validates <see cref="IValidatableConfiguration"/> objects are valid on app startup.
    /// </summary>
    /// <param name="validatableConfigurations">Configuration objects to get validated.</param>
    public ConfigurationValidationStartupFilter(IEnumerable<IValidatableConfiguration> validatableConfigurations)
        => _validatableConfigs = validatableConfigurations;

    /// <summary>
    /// Actually does the intended Job.
    /// </summary>
    /// <param name="next">The next handler in chain.</param>
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        var failures = new List<ConfigurationValidationItem>();
        foreach (var validatableObject in _validatableConfigs)
        {
            failures.AddRange(validatableObject.Validate());
        }

        if (failures.Count > 0)
        {
            throw new ConfigurationValidationException("There are issues with configuration.", failures);
        }

        return next;
    }

    [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => $"{_validatableConfigs.ToList().Count:D} config sections";
}
