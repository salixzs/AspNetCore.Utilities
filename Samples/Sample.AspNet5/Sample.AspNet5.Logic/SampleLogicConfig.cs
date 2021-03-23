using System.Net.Mail;
using Salix.AspNetCore.Utilities;

namespace Sample.AspNet5.Logic
{
    /// <summary>
    /// Configuration for some BUsiness Logic component.
    /// Logic references only Salix.AspNetCore.Utilities.Abstractions "package".
    /// </summary>
    public class SampleLogicConfig : IValidatableConfiguration
    {
        public int SomeValue { get; set; }
        public string SomeName { get; set; }
        public string SomeEndpoint { get; set; }
        public string SomeEmail { get; set; }

        /// <summary>
        /// This method is getting called by ASP.NET Startup Filter (provided in package) automatically when API starts.
        /// </summary>
        public void Validate()
        {
            // Simple cases
            if (this.SomeValue == 0)
            {
                throw new ConfigurationValidationException(nameof(SampleLogicConfig), nameof(this.SomeValue), "SomeValue should not be default value (0).");
            }

            if (string.IsNullOrEmpty(this.SomeName))
            {
                throw new ConfigurationValidationException(nameof(SampleLogicConfig), nameof(this.SomeName), "Value cannot be empty.");
            }

            // You can use regex or any other value checking functionality.

            // Fancy validators
            if (!System.Uri.IsWellFormedUriString(this.SomeEndpoint, System.UriKind.Absolute))
            {
                throw new ConfigurationValidationException(nameof(SampleLogicConfig), nameof(this.SomeEndpoint), "URL for configuration value seems to be missing or wrong.");
            }

            try
            {
                // Will throw exception if string does not contain valid e-mail address.
                var email = new MailAddress(this.SomeEmail);
            }
            catch
            {
                throw new ConfigurationValidationException(nameof(SampleLogicConfig), nameof(this.SomeEmail), "E-mail address seems to be missing or incorrect.");
            }
        }
    }
}
