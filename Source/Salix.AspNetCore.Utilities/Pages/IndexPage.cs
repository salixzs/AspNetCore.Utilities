using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Salix.AspNetCore.Utilities
{
    /// <summary>
    /// Class to compose landing/index page for API.
    /// </summary>
    public class IndexPage
    {
        internal IndexPageValues IndexPageOptions { get; private set; } = new IndexPageValues();

        /// <summary>
        /// Index page with default options/values.
        /// </summary>
        public IndexPage() { }

        /// <summary>
        /// Index page with default options/values and specified name.
        /// </summary>
        public IndexPage(string apiName) => this.IndexPageOptions.SetName(apiName);

        /// <summary>
        /// Retrieves Index/Landing page as string, containing ready-made HTML.
        /// </summary>
        /// <param name="buildData">Build and API specific data to include in page.</param>
        public string GetContents()
        {
            // {IncludeFile} = <div class="column"></div> or string empty
            string indexHtml = Pages.Html.index;
            indexHtml = indexHtml
                .Replace("{ApiName}", this.IndexPageOptions.ApiName)
                .Replace("{Description}", this.IndexPageOptions.Description)
                .Replace("{Version}", this.IndexPageOptions.Version)
                .Replace("{Environment}", this.IndexPageOptions.HostingEnvironment)
                .Replace("{Mode}", this.IndexPageOptions.BuildMode)
                .Replace("{HealthTestUrl}", this.IndexPageOptions.HealthPageAddress);

            indexHtml = this.IndexPageOptions.BuiltTime == DateTime.MinValue
                ? indexHtml.Replace("{Built}", "---")
                : indexHtml.Replace("{Built}", this.IndexPageOptions.BuiltTime.ToHumanDateString());

            if (this.IndexPageOptions.LinkButtons.Any())
            {
                var buttons = new StringBuilder("<hr/>");
                buttons.AppendLine("<p style=\"margin-top:2em;\">");
                foreach (var button in this.IndexPageOptions.LinkButtons)
                {
                    buttons.AppendLine($"<a class=\"button\" href=\"{button.Value}\">{button.Key}</a>");
                }

                indexHtml = indexHtml.Replace("{Buttons}", buttons.ToString());
            }
            else
            {
                indexHtml = indexHtml.Replace("{Buttons}", string.Empty);
            }

            indexHtml = string.IsNullOrEmpty(this.IndexPageOptions.IncludeFileName)
                ? indexHtml
                    .Replace("{OneColumnStyle}", "min-width:100%;")
                    .Replace("{IncludeFile}", string.Empty)
                : indexHtml
                    .Replace("{OneColumnStyle}", "padding-right: 2rem;")
                    .Replace("{IncludeFile}", LoadFileContents(this.IndexPageOptions.IncludeFileName));

            indexHtml = this.IndexPageOptions.Configurations != null && this.IndexPageOptions.Configurations.Any()
                ? indexHtml.Replace("{ConfigValues}", this.GenerateConfigurationsTable())
                : indexHtml.Replace("{ConfigValues}", "Configuration values are hidden for security purposes.");

            return indexHtml;
        }

        private static string LoadFileContents(string includeFileName)
        {
            if (!System.IO.File.Exists(includeFileName))
            {
                return $"<div class=\"column\"><h1>Included contents</h1><p>Contents file {includeFileName} not found!</p></div>";
            }

            if (new System.IO.FileInfo(includeFileName).Length > 51200)
            {
                return $"<div class=\"column\"><h1>Included contents</h1><p>Contents file {includeFileName} is too big!</p></div>";
            }

            string contents = System.IO.File.ReadAllText(includeFileName, Encoding.UTF8);
            if (System.IO.Path.GetExtension(includeFileName).StartsWith(".HTM", StringComparison.OrdinalIgnoreCase))
            {
                if (contents.Contains("<body>"))
                {
                    contents = Regex.Match(contents, @"(?s)(?<=<body>).+(?=<\/body>)", RegexOptions.IgnoreCase | RegexOptions.Multiline).Value;
                }

                return $"<div class=\"column\">{contents}</div>";
            }

            return $"<div class=\"column\"><h1>Included contents</h1><pre>{contents}</pre></div>";
        }

        private string GenerateConfigurationsTable()
        {
            var builder = new StringBuilder();
            builder.AppendLine("<table>");
            builder.AppendLine("<thead>");
            builder.AppendLine("<tr><th>Key</th><th>Value</th></tr>");
            builder.AppendLine("</thead>");
            builder.AppendLine("<tbody>");
            foreach (KeyValuePair<string, string> cfg in this.IndexPageOptions.Configurations)
            {
                builder.AppendLine($"<tr><td>{cfg.Key}</td><td>{cfg.Value}</td></tr>");
            }
            builder.AppendLine("</tbody>");
            builder.AppendLine("</table>");

            return builder.ToString();
        }
    }
}
