using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Orleans.Hosting;
using Orleans.Persistence.Couchbase.Config;
using Orleans.TestingHost;
using JsonSerializer = Orleans.Persistence.Couchbase.Serialization.JsonSerializer;

namespace Orleans.Persistence.Couchbase.IntegrationTests
{
    using JsonSerializer = JsonSerializer;

    public class TestBase<TSilo, TClient> where TSilo : ISiloBuilderConfigurator, new() where TClient : IClientBuilderConfigurator, new()
	{
		private short NumberOfSilos { get; set; }

		protected TestCluster Cluster { get; private set; }

		protected async Task Initialize(short numberOfSilos)
        {
            this.NumberOfSilos = numberOfSilos;
            await this.InitializeAsync();
        }

        protected void ShutDown() => Cluster?.StopAllSilos();

		public Task InitializeAsync()
		{
			var builder = new TestClusterBuilder(NumberOfSilos);

			builder.AddSiloBuilderConfigurator<TSilo>();
			builder.AddClientBuilderConfigurator<TClient>();

			Cluster = builder.Build();
			Cluster.Deploy();

			return Task.CompletedTask;
		}

		public Task DisposeAsync()
		{
			ShutDown();
			return Task.CompletedTask;
		}
	}

	public class ClientBuilderConfigurator : IClientBuilderConfigurator
	{
		public virtual void Configure(IConfiguration configuration, IClientBuilder clientBuilder)
			=> clientBuilder.ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(ITestGrain).Assembly).WithReferences());
	}

	public class SiloBuilderConfigurator : ISiloBuilderConfigurator
	{
		public void Configure(ISiloHostBuilder hostBuilder)
			=> hostBuilder
				.ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(ITestGrain).Assembly).WithReferences())
                .AddCouchbaseGrainStorage("TestStorageProvider")
                    .AddCouchbaseSerializer<JsonSerializer>(new JsonSerializerSettings())
                    .Build(optionsBuilder => optionsBuilder.Configure(
                        opts =>
                            {
                                var configuration = new ConfigurationBuilder()
                                    .AddJsonFile("appSettings.json")
                                    .Build();

                                var clientConfig = CouchbaseConfigurationExtensions.ReadConfiguration(configuration);
                                opts.BucketName = clientConfig.BucketConfigs.First().Value.BucketName;
                                opts.ClientConfiguration = clientConfig;
                            })
                    )
                .AddCouchbaseGrainStorage("TestStorageProvider2")
                .AddCouchbaseSerializer<JsonSerializer>(new JsonSerializerSettings())
                .Build(optionsBuilder => optionsBuilder.Configure(
                    opts =>
                        {
                            var configuration = new ConfigurationBuilder()
                                .AddJsonFile("appSettings.json")
                                .Build();

                            var clientConfig = CouchbaseConfigurationExtensions.ReadConfiguration(configuration);
                            opts.BucketName = clientConfig.BucketConfigs.First().Value.BucketName;
                            opts.ClientConfiguration = clientConfig;
                        })
                )
		;
	}
}
