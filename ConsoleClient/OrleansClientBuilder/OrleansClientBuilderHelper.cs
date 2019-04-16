using System;
using System.IO;
using ConsoleClient.TypedOptions;
using Microsoft.Extensions.Configuration;
using Orleans;
using Orleans.Configuration;
using Serilog;

namespace ConsoleClient.OrleansClientBuilder
{
    public static class OrleansClientBuilderHelper
    {
        public static IClientBuilder CreateClientBuilder(string[] args)
        {
            var (clusterInfo, providerInfo) = GetConfigSettings(args);

            var clientBuilder = new ClientBuilder()
                .Configure<ClientMessagingOptions>(options =>
                {
                    options.ResponseTimeout = TimeSpan.FromSeconds(20);
                    options.ResponseTimeoutWithDebugger = TimeSpan.FromMinutes(60);
                })
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = clusterInfo.ClusterId;
                    options.ServiceId = clusterInfo.ServiceId;
                })
                .ConfigureLogging(logging => logging.AddSerilog(dispose: true));

            if (providerInfo.DefaultProvider == "MongoDB")
            {
                clientBuilder.UseMongoDBClustering(options =>
                {
                    var mongoDbSettings = providerInfo.MongoDB.Cluster;

                    options.ConnectionString = mongoDbSettings.DbConn;
                    options.DatabaseName = mongoDbSettings.DbName;

                    if (!string.IsNullOrEmpty(mongoDbSettings.CollectionPrefix))
                    {
                        options.CollectionPrefix = mongoDbSettings.CollectionPrefix;
                    }
                });
            }
            else
            {
                clientBuilder.UseLocalhostClustering();
            }

            return clientBuilder;
        }

        private static (ClusterInfoOption, OrleansProviderOption) GetConfigSettings(string[] args)
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables(prefix: "ORLEANS_CLIENT_APP_")
                .AddCommandLine(args);

            var config = builder.Build().GetSection("Orleans");

            var clusterInfo = new ClusterInfoOption();
            config.GetSection("Cluster").Bind(clusterInfo);

            var providerInfo = new OrleansProviderOption();
            config.GetSection("Provider").Bind(providerInfo);

            return (clusterInfo, providerInfo);
        }
    }
}