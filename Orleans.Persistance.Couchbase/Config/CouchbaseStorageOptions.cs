using Couchbase.Configuration.Client;

namespace Orleans.Persistence.Couchbase.Config
{
    public class CouchbaseStorageOptions
	{
        public string BucketName { get; set; }
        public ClientConfiguration ClientConfiguration { get; set; }
    }
}
