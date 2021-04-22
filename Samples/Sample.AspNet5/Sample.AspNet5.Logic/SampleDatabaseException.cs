using System;
using System.Runtime.Serialization;

namespace Sample.AspNet5.Logic
{
    [Serializable]
    public class SampleDatabaseException : Exception
    {
        public DatabaseProblemType ErrorType { get; set; }

        public SampleDatabaseException()
        {
        }

        public SampleDatabaseException(string message)
            : base(message)
        {
        }

        public SampleDatabaseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected SampleDatabaseException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }
    }

    /// <summary>
    /// Dumb types of exception. Could be separate exceptions, depending on DB engine.
    /// </summary>
    public enum DatabaseProblemType
    {
        General,
        WrongSyntax,
        Concurrency,
        ObjectExists
    }
}
