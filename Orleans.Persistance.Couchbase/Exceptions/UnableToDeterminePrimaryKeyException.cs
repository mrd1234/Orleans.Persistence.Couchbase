using System;
using System.Runtime.Serialization;

namespace Orleans.Persistence.Couchbase.Exceptions
{
    [Serializable]
    public class UnableToDeterminePrimaryKeyException : Exception
    {
        public UnableToDeterminePrimaryKeyException()
        {
        }

        public UnableToDeterminePrimaryKeyException(string message) : base(message)
        {
        }

        public UnableToDeterminePrimaryKeyException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected UnableToDeterminePrimaryKeyException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}