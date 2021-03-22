using System;
using System.Diagnostics;

namespace Salix.AspNetCore.Utilities
{
    /// <summary>
    /// A utility <see cref="Exception"/> that indicates a strong typed configuration model was not configured correctly.
    /// Used by ASP.NET App builder custom filter.
    /// </summary>
    [DebuggerDisplay("{Message}")]
    public class ConfigurationValidationException : Exception
    {
        /// <summary>
        /// A utility <see cref="Exception"/> that indicates a strong typed configuration model was not configured correctly.
        /// Used by ASP.NET App builder custom filter.
        /// </summary>
        public ConfigurationValidationException()
            : this("There are invalid settings in configuration objects found by application builder.")
        {
        }

        /// <summary>
        /// A utility <see cref="Exception"/> that indicates a strong typed configuration model was not configured correctly.
        /// Used by ASP.NET App builder custom filter.
        /// </summary>
        /// <param name="className">The name of the Configuration Section Strongly typed class being validated.</param>
        /// <param name="propertyName">The property of the instance that was invalid.</param>
        /// <param name="error">A description of the configuration error.</param>
        public ConfigurationValidationException(string className, string propertyName, string error)
            : this(GetMessage(className, propertyName, error))
        {
        }

        /// <summary>
        /// A utility <see cref="Exception"/> that indicates a strong typed configuration model was not configured correctly.
        /// Used by ASP.NET App builder custom filter.
        /// </summary>
        /// <param name="message">A message that describes the validation problem.</param>
        public ConfigurationValidationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// A utility <see cref="Exception"/> that indicates a strong typed configuration model was not configured correctly.
        /// Used by ASP.NET App builder custom filter.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the validation to fail.</param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception.
        /// If the innerException parameter is not a null reference,
        /// the current exception is raised in a catch block that handles the inner exception.
        /// </param>
        public ConfigurationValidationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        private static string GetMessage(string className, string propertyName, string message)
        {
            return $@"Settings for {className}.{propertyName} are invalid: ""{message}"". 
Check that your configuration has been loaded correctly and all necessary values are set in the configuration files.";
        }
    }
}
