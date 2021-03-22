using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Salix.AspNetCore.Utilities
{
    /// <summary>
    /// General API Exception for handling exceptional situations, to distinguish custom thrown API exceptions and system default exceptions.
    /// </summary>
    [DebuggerDisplay("{Message}")]
    [Serializable]
    public class ApiException : Exception
    {
        /// <summary>
        /// General API Exception for handling exceptional situations, to distinguish custom thrown API exceptions and system default exceptions.
        /// </summary>
        public ApiException()
        {
        }

        /// <summary>
        /// General API Exception for handling exceptional situations, to distinguish custom thrown API exceptions and system default exceptions.
        /// </summary>
        /// <param name="message">A message which describes the error.</param>
        public ApiException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// General API Exception for handling exceptional situations, to distinguish custom thrown API exceptions and system default exceptions.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception.
        /// If the innerException parameter is not a null reference,
        /// the current exception is raised in a catch block that handles the inner exception.
        /// </param>
        public ApiException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// General API Exception for handling exceptional situations, to distinguish custom thrown API exceptions and system default exceptions.
        /// Constructor for serialization and deserialization needs.
        /// </summary>
        /// <param name="serializationInfo">Serialization information.</param>
        /// <param name="streamingContext">Serialization Stream context.</param>
        protected ApiException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }
    }
}
