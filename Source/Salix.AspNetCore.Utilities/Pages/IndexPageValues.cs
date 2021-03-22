using System;
using System.Diagnostics;

namespace Salix.AspNetCore.Utilities
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class IndexPageValues
    {
        /// <summary>
        /// Populate with Short name of API.
        /// </summary>
        public string ApiName { get; set; } = "API";

        /// <summary>
        /// Populate with description of API.
        /// </summary>
        public string Description { get; set; } = "API is successfully launched.";

        /// <summary>
        /// Populate with version number for API.
        /// </summary>
        public string Version { get; set; } = "1.0";

        /// <summary>
        /// Specify Date and Time when API was built.
        /// </summary>
        public DateTime BuiltTime { get; set; } = DateTime.Now;

        /// <summary>
        /// Environment name, where API is hosted.
        /// Use IWebHostEnvironment.Environment name to populate this automatically.
        /// </summary>
        public string HostingEnvironment { get; set; } = "Not specified";

        /// <summary>
        /// Populate with text, describing which mode API was built (DEBUG, PRODUCTION).
        /// </summary>
        public string BuildMode { get; set; } = "Not specified";

        /// <summary>
        /// 
        /// </summary>
        public string HealthPageAddress { get; set; } = "api/healthcheck";

        /// <summary>
        /// When using and configuring Swagger, populate with address, where it publishes its page.
        /// When not specified - will not display link to it.
        /// </summary>
        public string SwaggerPageAddress { get; set; }

        /// <summary>
        /// Displays object main properties in Debug screen. (Only for development purposes).
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => $"{this.ApiName} v{this.Version})";
    }
}
