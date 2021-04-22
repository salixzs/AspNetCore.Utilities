using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Sample.AspNet5.Logic
{
    /// <summary>
    /// Sample custom data validation exception, which can be used to throw in case incoming data is not valid.
    /// </summary>
    [Serializable]
    public class SampleDataValidationException : Exception
    {
        public List<ValidatedProperty> ValidationErrors { get; private set; } = new List<ValidatedProperty>();

        public SampleDataValidationException()
        {
        }

        public SampleDataValidationException(string message)
            : base(message)
        {
        }

        public SampleDataValidationException(string message, List<ValidatedProperty> validationErrors)
            : base(message) => this.ValidationErrors = validationErrors;

        public SampleDataValidationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected SampleDataValidationException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }
    }

    /// <summary>
    /// Some custom object to describe validation failure.
    /// </summary>
    public class ValidatedProperty
    {
        public string PropertyName { get; set; }
        public string ValidationMessage { get; set; }
        public object AppliedValue { get; set; }
    }
}
