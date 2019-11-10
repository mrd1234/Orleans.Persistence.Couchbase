using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Persistence.Couchbase.Core;
using Orleans.Persistence.Couchbase.Serialization;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Serialization;
using Orleans.Storage;

namespace Orleans.Persistence.Couchbase.Config
{
    public static class CouchbaseSiloHostBuilderExtensions
	{
		public static CouchbaseStorageSiloHostBuilderOptionsBuilder AddCouchbaseGrainStorage(this ISiloHostBuilder builder, string name) 
            => new CouchbaseStorageSiloHostBuilderOptionsBuilder(builder, name);

		public static CouchbaseStorageSiloHostBuilderOptionsBuilder AddCouchbaseGrainStorageAsDefault(this ISiloHostBuilder builder) => builder.AddCouchbaseGrainStorage("Default");

		internal static IServiceCollection AddCouchbaseGrainStorage(this IServiceCollection services, string name, Action<OptionsBuilder<CouchbaseStorageOptions>> configureOptions = null)
		{
			configureOptions?.Invoke(services.AddOptions<CouchbaseStorageOptions>(name));
			services.AddSingletonNamedService(name, CreateDataManager);
			services.ConfigureNamedOptionForLogging<CouchbaseStorageOptions>(name);
			services.TryAddSingleton(sp => sp.GetServiceByName<IGrainStorage>(ProviderConstants.DEFAULT_STORAGE_PROVIDER_NAME));

			return services
				.AddSingletonNamedService(name, CreateCouchbaseStorage)
				.AddSingletonNamedService(name, (provider, n)
					=> (ILifecycleParticipant<ISiloLifecycle>)provider.GetRequiredServiceByName<IGrainStorage>(n));
		}

		internal static ISiloHostBuilder AddCouchbaseDefaultSerializer(this ISiloHostBuilder builder, string name)
			=> builder.AddCouchbaseSerializer<JsonSerializer>(
				name,
				provider => new object[] 
                {
					OrleansJsonSerializer.GetDefaultSerializerSettings(provider.GetRequiredService<ITypeResolver>(), provider.GetRequiredService<IGrainFactory>())
                }
			);

		internal static ISiloHostBuilder AddCouchbaseSerializer<TSerializer>(this ISiloHostBuilder builder, string name, params object[] settings)
			where TSerializer : ISerializer
			=> builder.ConfigureServices(services =>
				services.AddSingletonNamedService<ISerializer>(name, (provider, n)
					=> ActivatorUtilities.CreateInstance<TSerializer>(provider, settings))
			);

		internal static ISiloHostBuilder AddCouchbaseSerializer<TSerializer>(this ISiloHostBuilder builder, string name, Func<IServiceProvider, object[]> cfg) where TSerializer : ISerializer
			=> builder.ConfigureServices(services =>
				services.AddSingletonNamedService<ISerializer>(name, (provider, n)
					=> ActivatorUtilities.CreateInstance<TSerializer>(provider, cfg?.Invoke(provider)))
			);

		private static IGrainStorage CreateCouchbaseStorage(IServiceProvider services, string name)
		{
            var dataManager = services.GetRequiredServiceByName<ICouchbaseDataManager>(name);
            var logger = services.GetRequiredService<ILogger<CouchbaseGrainStorage>>();
            var serializer = services.GetServiceByName<ISerializer>(name);
            return ActivatorUtilities.CreateInstance<CouchbaseGrainStorage>(services, name, dataManager, logger, serializer);
		}

		private static ICouchbaseDataManager CreateDataManager(IServiceProvider provider, string name)
		{
			var options = provider.GetRequiredService<IOptionsSnapshot<CouchbaseStorageOptions>>();
			return ActivatorUtilities.CreateInstance<CouchbaseDataManager>(provider, options.Get(name).BucketName, options.Get(name).ClientConfiguration);
        }
	}
}