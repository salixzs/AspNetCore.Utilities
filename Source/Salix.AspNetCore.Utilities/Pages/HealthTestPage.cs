using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Salix.AspNetCore.Utilities
{
    /// <summary>
    /// Provides contents of Health test checking page as string.
    /// </summary>
    public static class HealthTestPage
    {
        /// <summary>
        /// Retrieves contents of Health checking page with filled HealthReport data in human readable format.
        /// </summary>
        /// <param name="healthReport">AspNet built-in Health report / results.</param>
        /// <param name="originalHealthTestEndpoint">Health test endpoint exposing JSON result (standard, provided by Asp.Net).</param>
        /// <param name="testingLinks">Collection of custom links to show additinally on this page.</param>
        public static string GetContents(HealthReport healthReport, string originalHealthTestEndpoint, List<HealthTestPageLink> testingLinks = null)
        {
            string healthPage = Pages.Html.health;
            healthPage = healthPage
                .Replace("{HealthEndpoint}", originalHealthTestEndpoint)
                .Replace("<div id=\"hc\"></div>", PrepareHealthReport(healthReport));

            if (testingLinks != null && testingLinks.Count > 0)
            {
                var links = new StringBuilder();
                foreach (HealthTestPageLink link in testingLinks)
                {
                    links.Append($"<li><a href='{link.TestEndpoint}'>{link.Name}</a> - {link.Description}</li>");
                }

                healthPage = healthPage.Replace("<div id=\"links\"></div>", $"<h2>Testing links</h2><ul>{links}</ul>");
            }
            return healthPage;
        }

        /// <summary>
        /// Old-fashioned server-side page creation from dynamic data.
        /// </summary>
        /// <param name="healthResult">The health result.</param>
        private static string PrepareHealthReport(HealthReport healthResult)
        {
            var poorManRazor = new StringBuilder("<p>Overall health status: ");
            switch (healthResult.Status)
            {
                case HealthStatus.Unhealthy:
                    poorManRazor.Append("<span style=\"color:#FA7575\">");
                    break;
                case HealthStatus.Degraded:
                    poorManRazor.Append("<span style=\"color:#F6E979\">");
                    break;
                case HealthStatus.Healthy:
                    poorManRazor.Append("<span style=\"color:#5DEC50\">");
                    break;
                default:
                    break;
            }

            poorManRazor.AppendLine($"{healthResult.Status}</span></p><table>");
            foreach (KeyValuePair<string, HealthReportEntry> healthCheckItem in healthResult.Entries)
            {
                poorManRazor.AppendLine("<tr>");

                poorManRazor.AppendLine("<td>");
                poorManRazor.AppendLine(healthCheckItem.Key.ToUpper(CultureInfo.InvariantCulture));
                poorManRazor.AppendLine("</td>");

                poorManRazor.AppendLine("<td>");
                switch (healthCheckItem.Value.Status)
                {
                    case HealthStatus.Unhealthy:
                        poorManRazor.AppendLine("<img src=\"data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAADEklEQVRYR+1XT2jTUBj/Xpq0aZtsVed1HaLO7eLAiwqeHcIgDTiY4BgDL9KBB/Gm9OBZBOcfEHQOcTgliRRlnjyI4kWYBzfnQZyCCDo3lz9N2zTPvEql7drkbW4UwQehkPf6fb/v974/vyBo8UIt9g//FoABSZI4YI6HGdSTd919DkJRFuNchGHeFVw8X0R4Oquqj9bDKhUDA5I8EgJ8Ncmx7sFwREiyIUiyLMQYBizXhUXH8Z4SvCrkDe+XKQI+/VhV79AA8QXQL0ldcUAzyTCXHI7HeeI0aBEwk6ZpLxaKiybg/hlN++j3n6YAjklSXxjQi1FRjB3hI0F+1+w/t/NwyzByBewefqJps80MNARAIucBvU23ibEDkfU7rzh7nc/DuG7kbOz2NmOiIQA5lVoYEcS9JHJyx9+8h4b+imNyDTu9/CA5QpiYMPT3iqp2N2JhDQCScLu50I0LiUQ5dGLs8qoOZ9pEKhCNzmdWVowPxdJYVlMm6kGsASClZCuTaI9WR0wLotk58v7iys+lh6rS4QuA1PkujrubSSTi9QeDQATt/2aheDKraVq17RoGUlLq3mA8PtQfizZMWr8Ig65pxsrBtGlOqZp6oimAIVmeHRPF/T3hcNOyqwcRFHnF0HyhAFd0/c2UovQ1ZyAl58a3b+NJ9vqtilPZY0rxIqNJUFJN6R/LtqoqNfTWXMFASsaTHTuomg4pr5uGAacEAWgb1fD3JciqSo3P2hxYNwMxjwFr8xhofQ60ugq2sg+cW162vzrOkG8fINm3FZ2QlOClVT24ExIAZBbs4djr5xPt/CbOAtObBWmqWUCcbuY0JB3wgWXST0MCoKwHEDOXFoVoS/QAAVFWRIh5OSoIUdpGU93BSOT3LWtjiqhiiDARQ+hpF8t1Dgv0mvC2bhifS84XC+OjG9aE1dGUVTGCa8lQqHSIJ6qYhc5Q6I8q/lQqlYXLs5ydW3JdK4/hbKOEo1JEfoOA9AkW0CDPoF4bQ7cDwHs62eYRLNgunnPA+y6om/dBg4XquyDIyN/s/wfQcgZ+Aatm/TA7zsIHAAAAAElFTkSuQmCC\" align=\"top\" width=\"24\" />");
                        break;
                    case HealthStatus.Degraded:
                        poorManRazor.AppendLine("<img src=\"data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAACRUlEQVRYR2NkGGDAOMD2MwxdB4RFBHV8/8lQzsnO0LlqxboKckOSrBDwCQhw4Odm3dKbI8ddPOXR149ff/ts2bDhADmOIMsB0dHBFxK9xPWdjfkY9p79xDBv66sLy5atMaSLA3wDghJUpNkm9OUq8MMsLJzy8MPdJz8LN29Yt4BUR5AUAgEBAQJs7Ey3a+JlRHSVuOB2Xb73jaF50dO3v3/8VdmwYcMHUhxBkgNACc9YjSe7LEqKB92SrmXPvpy99WUqqQmSaAd4BAQoCHCyXJiQJ88vLsiK4cmX738zFEx6+PHD9z8GOzZseEBsKBDtgMjI4DWeloLesW4iHLgMX7zrzY/tx99vXb58bQhVHQDLdjNLFLi5OZnhZvtV3mLY1K4G53/9/pchvecBSdmSqBAAxv2dNF8JZVC2QwboDgDJkZotCToAW7aDOQKbA0BypGRLvA4AZTtGZsanjcmyXMjZjpADSMmWeB2AL9uBHIErBEByxGZLnA4AZTsuFqYb04oV2bFlO0IOIDZb4nQA0Pe7fKyEbPFlO3whAHIgMdkSqwNwZTv0vE3IAcRkS6wOwJXtSHUAMdkSwwGgbKcmxz6lJ0uem9jSjJA6fNkSxQGEsh0hi3DJ48uWKA4AZTszDd7C4ghJNmIsI5QGkM3AlS3hDiAm2xHjKFxqcGVLuANAtZ2PtVBwlIswJfbg1YstW8Id4BsY9H92mSIDrkKHGq4ChUJq132GzevXwe2FM2DNbGpYhM8M9GY8wdqQ1g4adcBoCAAAaFxHMEl6bQIAAAAASUVORK5CYII=\" align=\"top\" width=\"24\" />");
                        break;
                    case HealthStatus.Healthy:
                        poorManRazor.AppendLine("<img src=\"data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAABqUlEQVRYR2NkGGDAOMD2M4w6gG4h4BUQYMDIwOSwdcO6CcjRjuEA34CAAEYO1tn/f/4VoTR9qM/ybgCZ8fPZp1cP6g+VMTAwNm7esG4BXgf4RYa+kS+xEOaQ5afUfrj+R70nGP79/rt7dd98N3RDMUMgMOi/xkwvqln+cuU1hp9PPjHIFpk39oingkMEfxRQ0QEfjz1heL3lNoNijQ0DEycLfR3w4/FHhifTzzHIZBoxgKLzRvo2hs3r12GEOE2i4O+33wwPWo4wiPioMvBbyYBDnK4OACU6dhk+BvFwLXh0080BsEQnV2yBktjo4gDkRMfMxUp9B7zdc5+BW10InKjQAXqiQ5enSgjg8iG2REcTB4AMBcXxD2DBIo8Ux9gSHc0cADIY2UJciY6mDgAF+X1gPudWE2b4eustuKRDT3Q0dQDIcEKJjuYOAFnw6803BjYRLqIqLqrkAqJswqFo1AGjIUB0CPjHhj+SKzCTpWabEJRlH/aceLtp+WqMhi72VjEn65z/P/4KU5LqkfUy8bA+/vv5Z9yWDRsOoJtJt34BLs+MOmDAQwAAFdYXMCaGdjIAAAAASUVORK5CYII=\" align=\"top\" width=\"24\" />");
                        break;
                    default:
                        break;
                }

                poorManRazor.AppendLine($"<strong>{healthCheckItem.Value.Status}</strong>");
                poorManRazor.AppendLine("<br/>");
                poorManRazor.AppendLine(healthCheckItem.Value.Description);
                if (healthCheckItem.Value.Exception != null)
                {
                    poorManRazor.AppendLine("<br/>");
                    poorManRazor.AppendLine(healthCheckItem.Value.Exception.Message);
                }

                poorManRazor.AppendLine("</td>");

                poorManRazor.AppendLine("<td>");
                if (healthCheckItem.Value.Data?.Any() == true)
                {
                    foreach (KeyValuePair<string, object> dataItem in healthCheckItem.Value.Data)
                    {
                        poorManRazor.AppendLine($"* {dataItem.Key}: {dataItem.Value}<br/>");
                    }
                }

                poorManRazor.AppendLine("</td>");
                poorManRazor.AppendLine("</tr>");
            }

            poorManRazor.AppendLine("</table>");
            return poorManRazor.ToString();
        }
    }
}
