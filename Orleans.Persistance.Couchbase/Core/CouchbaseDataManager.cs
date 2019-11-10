using System;
using System.Linq;
using System.Threading.Tasks;
using Couchbase;
using Couchbase.Configuration.Client;
using Couchbase.Core;
using Couchbase.IO;
using Orleans.Persistence.Couchbase.Exceptions;
using Orleans.Persistence.Couchbase.Models;
using Orleans.Storage;
using Polly;
using Polly.Timeout;

namespace Orleans.Persistence.Couchbase.Core
{
    public class CouchbaseDataManager : IDisposable, ICouchbaseDataManager
    {
        private IBucket bucket;

        public string BucketName { get; }

        private ClientConfiguration ClientConfig { get; }

        public CouchbaseDataManager(string bucketName, ClientConfiguration clientConfig)
        {
            this.BucketName = bucketName;
            this.ClientConfig = clientConfig;
        }

        public async Task Initialise()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(this.BucketName))
                {
                    throw new ArgumentException("BucketName can not be null or empty");
                }

                if (this.ClientConfig == null)
                {
                    throw new ArgumentException("You must supply a configuration to connect to Couchbase");
                }

                if (this.ClientConfig.BucketConfigs.All(a => a.Key != this.BucketName))
                {
                    throw new BucketConfigMissingFromConfigurationException($"The requested bucket is named '{this.BucketName}' however the provided Couchbase configuration has no bucket configuration");
                }

                if (!ClusterHelper.Initialized)
                {
                    ClusterHelper.Initialize(this.ClientConfig);
                }
                else
                {
                    foreach (var conf in this.ClientConfig.BucketConfigs)
                    {
                        if (ClusterHelper.Get().Configuration.BucketConfigs.ContainsKey(conf.Key))
                        {
                            ClusterHelper.Get().Configuration.BucketConfigs.Remove(conf.Key);
                        }

                        ClusterHelper.Get().Configuration.BucketConfigs.Add(conf.Key, conf.Value);
                    }
                }

                var timeoutPolicy = Policy.TimeoutAsync(30, TimeoutStrategy.Pessimistic);
                this.bucket = await timeoutPolicy.ExecuteAsync(async () => this.bucket = await ClusterHelper.GetBucketAsync(this.BucketName));
            }
            catch (Exception e)
            {
                await Task.FromException(e);
            }
        }

        /// <inheritdoc />
        public async Task DeleteAsync(string grainTypeName, string key, string eTag)
        {
            var documentId = GetDocumentId(grainTypeName, key);
            var result = await bucket.RemoveAsync(documentId, ulong.Parse(eTag));
            if (!result.Success)
            {
                throw new InconsistentStateException(result.Message, eTag, result.Cas.ToString());
            }
        }

        /// <inheritdoc />
        public async Task<ReadResponse> ReadAsync(string grainTypeName, string key)
        {
            var documentId = GetDocumentId(grainTypeName, key);

            var result = await bucket.GetAsync<string>(documentId);
            if (result.Success)
            {
                return new ReadResponse { Document = result.Value, ETag = result.Cas.ToString() };
            }

            if (!result.Success && result.Status == ResponseStatus.KeyNotFound)
            {
                return new ReadResponse { Document = null, ETag = string.Empty };
            }

            throw result.Exception;
        }

        /// <inheritdoc />
        public async Task<string> WriteAsync(string grainTypeName, string key, string entityData, string eTag)
        {
            var documentId = GetDocumentId(grainTypeName, key);

            string result;

            if (ulong.TryParse(eTag, out var realETag))
            {
                var r = await bucket.UpsertAsync(documentId, entityData, realETag, TimeSpan.Zero);
                if (!r.Success)
                {
                    throw new InconsistentStateException(r.Status.ToString(), eTag, r.Cas.ToString());
                }

                result = r.Cas.ToString();
            }
            else
            {
                var r = await bucket.InsertAsync(documentId, entityData, TimeSpan.Zero);

                if (!r.Success && r.Status == ResponseStatus.KeyExists)
                {
                    throw new InconsistentStateException(r.Status.ToString(), eTag, r.Cas.ToString());
                }

                if (!r.Success)
                {
                    throw new Exception(r.Status.ToString());
                }

                result = r.Cas.ToString();
            }

            return result;
        }

        public void Dispose()
        {
            bucket.Dispose();
            bucket = null;
            ClusterHelper.Close();
            GC.SuppressFinalize(this);
        }
        
        private static string GetDocumentId(string grainTypeName, string key) => $"{grainTypeName}_{key}";
    }
}
