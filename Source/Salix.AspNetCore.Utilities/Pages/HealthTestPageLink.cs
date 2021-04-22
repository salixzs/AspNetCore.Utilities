using System.Diagnostics;

namespace Salix.AspNetCore.Utilities
{
    /// <summary>
    /// Data contract to populate Health checking page with custom links.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class HealthTestPageLink
    {
        /// <summary>
        /// Endpoint to open when page link is clicked. Can be relative (to API) or even absolute from external resource.
        /// </summary>
        public string TestEndpoint { get; set; }

        /// <summary>
        /// Short name to use as link.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Not very long description of a link (as helping explanation for link).
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Displays object main properties in Debug screen. (Only for development purposes).
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => $"{this.Name} ({this.TestEndpoint})";
    }
}
