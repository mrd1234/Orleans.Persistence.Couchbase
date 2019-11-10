using System;
using Couchbase.Configuration.Client;
using Microsoft.Extensions.Configuration;
using Orleans.Persistence.Couchbase.Exceptions;

namespace Orleans.Persistence.Couchbase.Config
{
    public static class CouchbaseConfigurationExtensions
    {
        public static ClientConfiguration ReadJsonConfiguration(string filePathAndName, string sectionName = "couchbase:clientConfiguration")
        {
            try
            {
                var configBuilder = new ConfigurationBuilder();
                configBuilder.AddJsonFile(filePathAndName, optional: false, reloadOnChange: false);

                var jsonConfiguration = configBuilder.Build();
                var definition = new CouchbaseClientDefinition();

                var section = jsonConfiguration.GetSection(sectionName);
                section.Bind(definition);

                var clientConfig = new ClientConfiguration(definition);
                return clientConfig;
            }
            catch (Exception ex)
            {
                throw new InvalidCouchbaseConfigurationException($"The section '{sectionName}' of config file {filePathAndName} is not in the expected format", ex);
            }
        }

        public static ClientConfiguration ReadConfiguration(IConfigurationRoot configuration, string sectionName = "couchbase:clientConfiguration")
        {
            try
            {
                var definition = new CouchbaseClientDefinition();

                var section = configuration.GetSection(sectionName);
                section.Bind(definition);

                var clientConfig = new ClientConfiguration(definition);
                return clientConfig;
            }
            catch (Exception ex)
            {
                throw new InvalidCouchbaseConfigurationException($"The section '{sectionName}' of configuration is not in the expected format", ex);
            }
        }
    }
}
