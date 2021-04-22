using System;
using System.Globalization;
using System.Reflection;

namespace Salix.AspNetCore.Utilities
{
    /// <summary>
    /// Class to compose landing/index page for API.
    /// </summary>
    public static class IndexPage
    {
        /// <summary>
        /// Retrieves Index/Landing page as string, containing ready-made HTML.
        /// </summary>
        /// <param name="buildData">Build and API specific data to include in page.</param>
        public static string GetContents(IndexPageValues buildData)
        {
            string indexHtml = Pages.Html.index;
            indexHtml = indexHtml
                .Replace("{ApiName}", buildData.ApiName)
                .Replace("{Description}", buildData.Description)
                .Replace("{Version}", buildData.Version)
                .Replace("{Environment}", buildData.HostingEnvironment)
                .Replace("{Mode}", buildData.BuildMode)
                .Replace("{HealthTestUrl}", buildData.HealthPageAddress);

            indexHtml = buildData.BuiltTime == DateTime.MinValue
                ? indexHtml.Replace("{Built}", "---")
                : indexHtml.Replace("{Built}", buildData.BuiltTime.ToHumanDateString());

            indexHtml = indexHtml.Replace("{Swagger}", !string.IsNullOrEmpty(buildData.SwaggerPageAddress)
                ? $"<a href=\"{buildData.SwaggerPageAddress}\">Swagger</a>"
                : string.Empty);

            return indexHtml;
        }

        /// <summary>
        /// Retrieves Version number from assembly (usually controlled by AssemblyInfo.cs file).
        /// </summary>
        /// <param name="assembly">Assembly, for which to get version number.</param>
        /// <param name="partsToReturn">How many parts/numbers to return as version (AssemblyInfo can contain 4 by default or custom).</param>
        public static string ExtractVersionFromAssembly(Assembly assembly, int partsToReturn = 2)
        {
            string[] version = assembly.GetName().Version.ToString().Split('.');
            return version.Length switch
            {
                0 => "Not determined",
                > 3 when partsToReturn == 4 => $"{version[0]}.{version[1]}.{version[2]}.{version[3]}",
                > 2 when partsToReturn >= 3 => $"{version[0]}.{version[1]}.{version[2]}",
                > 1 when partsToReturn >= 2 => $"{version[0]}.{version[1]}",
                _ => version[0]
            };
        }

        /// <summary>
        /// When non-deterministic version number in AssemblyInfo.cs is used
        /// in format "X.x.*" (X = major version number, x = minor version number, * to fill current datetime as numbers by compiler)
        /// this method can extract this datetime back as typed.
        /// </summary>
        /// <param name="assembly">Assembly with non-deterministic version number (with *) set in AssemblyInfo.cs</param>
        /// <returns>Extracted Date and Time or current datetime - 1 minute.</returns>
        public static DateTime ExtractBuildTimeFromAssembly(Assembly assembly)
        {
            string[] version = assembly.GetName().Version.ToString().Split('.');

            if (version.Length != 4)
            {
                return DateTime.Now.AddMinutes(-1);
            }

            if (!int.TryParse(version[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int build))
            {
                return DateTime.Now.AddMinutes(-1);
            }

            if (!int.TryParse(version[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out int revision))
            {
                return DateTime.Now.AddMinutes(-1);
            }

            return new DateTime(2000, 1, 1)
                    + new TimeSpan(build, 0, 0, 0)
                    + TimeSpan.FromSeconds(revision * 2);
        }
    }
}
