using System;
using System.Runtime.Serialization;

namespace Orleans.Persistence.Couchbase.Exceptions
{
    [Serializable]
    public class InvalidCouchbaseConfigurationException : Exception
    {
        public InvalidCouchbaseConfigurationException()
        {
        }

        public InvalidCouchbaseConfigurationException(string message) : base(message)
        {
        }

        public InvalidCouchbaseConfigurationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidCouchbaseConfigurationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
