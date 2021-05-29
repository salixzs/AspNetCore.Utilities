using System.Collections.Generic;

namespace Salix.AspNetCore.Utilities
{
    /// <summary>
    /// External logic to load customized set of configuration values to be shown on Index page for API.
    /// </summary>
    public interface IConfigurationValuesLoader
    {
        /// <summary>
        /// Gets the selected configuration values to be shown on Index page for API.
        /// </summary>
        /// <param name="whitelistFilter">The filter of whitelisted configuration keys (starts-with*).</param>
        public Dictionary<string, string> GetConfigurationValues(HashSet<string> whitelistFilter = null);
    }
}
