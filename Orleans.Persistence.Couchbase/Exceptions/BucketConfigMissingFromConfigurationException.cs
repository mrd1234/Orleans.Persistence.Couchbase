using System;
using System.Runtime.Serialization;

namespace Orleans.Persistence.Couchbase.Exceptions
{
    [Serializable]
    public class BucketConfigMissingFromConfigurationException : Exception
    {
        public BucketConfigMissingFromConfigurationException()
        {
        }

        public BucketConfigMissingFromConfigurationException(string message) : base(message)
        {
        }

        public BucketConfigMissingFromConfigurationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected BucketConfigMissingFromConfigurationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
