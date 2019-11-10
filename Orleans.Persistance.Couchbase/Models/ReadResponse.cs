namespace Orleans.Persistence.Couchbase.Models
{
    public class ReadResponse
    {
        public string Document { get; set; }
        public string ETag { get; set; }
    }
}
