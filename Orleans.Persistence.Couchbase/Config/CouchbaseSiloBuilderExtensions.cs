using System;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
using Orleans.Persistence.Couchbase.Serialization;
using Orleans.Runtime;
using Orleans.Serialization;

namespace Orleans.Persistence.Couchbase.Config
{
    public static class CouchbaseSiloBuilderExtensions
    {
        public static CouchbaseStorageOptionsBuilder AddCouchbaseGrainStorage(this ISiloBuilder builder, string name) => new CouchbaseStorageOptionsBuilder(builder, name);

        public static CouchbaseStorageOptionsBuilder AddCouchbaseGrainStorageAsDefault(this ISiloBuilder builder) => builder.AddCouchbaseGrainStorage("Default");

        public static ISiloBuilder AddCouchbaseDefaultSerializer(this ISiloBuilder builder, string name)
            => builder.AddCouchbaseSerializer<JsonSerializer>(
                name,
                provider => new object[] 
                { 
                    OrleansJsonSerializer.GetDefaultSerializerSettings(provider.GetRequiredService<ITypeResolver>(), provider.GetRequiredService<IGrainFactory>())
                }
            );

        public static ISiloBuilder AddCouchbaseSerializer<TSerializer>(this ISiloBuilder builder, string name, params object[] settings) where TSerializer : ISerializer
            => builder.ConfigureServices(services =>
                services.AddSingletonNamedService<ISerializer>(name, (provider, n)
                    => ActivatorUtilities.CreateInstance<TSerializer>(provider, settings))
            );

        private static ISiloBuilder AddCouchbaseSerializer<TSerializer>(this ISiloBuilder builder, string name, Func<IServiceProvider, object[]> cfg) 
            where TSerializer : ISerializer
            => builder.ConfigureServices(services =>
                services.AddSingletonNamedService<ISerializer>(name, (provider, n)
                    => ActivatorUtilities.CreateInstance<TSerializer>(provider, cfg?.Invoke(provider)))
            );
    }
}