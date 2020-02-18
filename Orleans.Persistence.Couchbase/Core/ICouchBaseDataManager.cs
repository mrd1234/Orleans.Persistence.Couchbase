using System.Threading.Tasks;
using Orleans.Persistence.Couchbase.Models;

namespace Orleans.Persistence.Couchbase.Core
{
    public interface ICouchbaseDataManager
    {
        string BucketName { get; }

        /// <summary>
        /// Validates and applies storage provider configuration
        /// </summary>
        Task Initialise();

        /// <summary>
        /// Deletes a document representing a grain state object.
        /// </summary>
        /// <param name="grainTypeName">The type of the grain state object.</param>
        /// <param name="key">The grain id string.</param>
        Task DeleteAsync(string grainTypeName, string key, string eTag);

        /// <summary>
        /// Reads a document representing a grain state object.
        /// </summary>
        /// <param name="grainTypeName">The type of the grain state object.</param>
        /// <param name="key">The grain id string.</param>
        Task<ReadResponse> ReadAsync(string grainTypeName, string key);

        /// <summary>
        /// Writes a document representing a grain state object.
        /// </summary>
        /// <param name="grainTypeName">The type of the grain state object.</param>
        /// <param name="key">The grain id string.</param>
        /// <param name="entityData">The grain state data to be stored.</param>
        /// <param name="eTag"></param>
        Task<string> WriteAsync(string grainTypeName, string key, string entityData, string eTag);

        void Dispose();
    }
}