using System;
using Microsoft.Extensions.Options;
using Orleans.Hosting;
using Orleans.Persistence.Couchbase.Serialization;

namespace Orleans.Persistence.Couchbase.Config
{
    public class CouchbaseStorageSiloHostBuilderOptionsBuilder
    {
        private readonly ISiloHostBuilder builder;
        private bool serializerAdded;
        private readonly string name;

        public CouchbaseStorageSiloHostBuilderOptionsBuilder(ISiloHostBuilder builder, string name)
        {
            this.builder = builder;
            this.name = name;
        }

        public CouchbaseStorageSiloHostBuilderOptionsBuilder AddCouchbaseSerializer<TSerializer>(params object[] settings) where TSerializer : ISerializer
        {
            builder.AddCouchbaseSerializer<TSerializer>(name, settings);
            serializerAdded = true;
            return this;
        }

        public CouchbaseStorageSiloHostBuilderOptionsBuilder AddCouchbaseDefaultSerializer()
        {
            builder.AddCouchbaseDefaultSerializer(name);
            serializerAdded = true;
            return this;
        }

        public ISiloHostBuilder Build(Action<OptionsBuilder<CouchbaseStorageOptions>> configureOptions)
        {
            if (!serializerAdded)
            {
                builder.AddCouchbaseDefaultSerializer(name);
            }

            return builder.ConfigureServices(services => services.AddCouchbaseGrainStorage(name, configureOptions));
        }
    }
}