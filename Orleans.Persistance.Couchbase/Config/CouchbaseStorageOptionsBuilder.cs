using System;
using Microsoft.Extensions.Options;
using Orleans.Hosting;
using Orleans.Persistence.Couchbase.Serialization;

namespace Orleans.Persistence.Couchbase.Config
{
    public class CouchbaseStorageOptionsBuilder
    {
        private readonly ISiloBuilder builder;
        private bool serialiserAdded;
        private readonly string name;

        public CouchbaseStorageOptionsBuilder(ISiloBuilder builder, string name)
        {
            this.builder = builder;
            this.name = name;
        }

        public CouchbaseStorageOptionsBuilder AddCouchbaseSerializer<TSerializer>(params object[] settings)
            where TSerializer : ISerializer
        {
            builder.AddCouchbaseSerializer<TSerializer>(name, settings);
            serialiserAdded = true;
            return this;
        }

        public CouchbaseStorageOptionsBuilder AddCouchbaseDefaultSerializer()
        {
            builder.AddCouchbaseDefaultSerializer(name);
            serialiserAdded = true;
            return this;
        }

        public ISiloBuilder Build(Action<OptionsBuilder<CouchbaseStorageOptions>> configureOptions)
        {
            if (!serialiserAdded)
            {
                builder.AddCouchbaseDefaultSerializer(name);
            }

            return builder.ConfigureServices(services => services.AddCouchbaseGrainStorage(name, configureOptions));
        }
    }
}