using System;
using System.ComponentModel;
using Azure.Core.Extensions;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Azure.Messaging.ServiceBus.Compression;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Azure
{
    public static class ServiceBusCompressionAwareClientBuilderExtensions
    {

        /// <summary>
        /// Registers a <see cref="ServiceBusClient "/> instance with the provided <paramref name="connectionString"/> and <paramref name="configuration"/>.
        /// The client will be able to compress and decompress the payload of a message based on configurations.
        /// </summary>
        /// <param name="builder">The builder</param>
        /// <param name="connectionString">Servicebus connectionstring</param>
        /// <param name="configuration">Controls the compression</param>
        /// <typeparam name="TBuilder">The Builder</typeparam>
        /// <returns></returns>
        public static IAzureClientBuilder<ServiceBusClient, ServiceBusClientOptions> AddCompressionAwareServiceBusClient<TBuilder>(this TBuilder builder, string connectionString, CompressionConfiguration configuration)
            where TBuilder : IAzureClientFactoryBuilder
        {
            return builder.RegisterClientFactory<ServiceBusClient, ServiceBusClientOptions>(options => new CompressionAwareServiceBusClient(connectionString, options, configuration));
        }       
        
        /// <summary>
        /// Registers a <see cref="ServiceBusClient "/> instance with the provided <paramref name="connectionString"/> and provied <paramref name="compressionThresholdLimitBytes"/>.
        /// Create a ServiceBus client that is able to Compress and decompress the messagebody of the message if required
        /// </summary>
        /// <param name="builder">The Builder</param>
        /// <param name="connectionString">Connection to the Servicebus</param>
        /// <param name="compressionThresholdLimitBytes">The minimum size of the messagepayload before compressing it. Basicaly if the payload is greater than the threshold it will be compressed. Must be greater than 0</param>
        /// <typeparam name="TBuilder">The Builder</typeparam>
        /// <returns>Registration of a ServiceBusclient that is aware of Compressing and Decompressing message body content</returns>
        public static IAzureClientBuilder<ServiceBusClient, ServiceBusClientOptions> AddCompressionAwareServiceBusClient<TBuilder>(this TBuilder builder, string connectionString, int compressionThresholdLimitBytes = GzipCompressionConfiguration.MinimumCompressionSize )
            where TBuilder : IAzureClientFactoryBuilder
        {
            Guard.AgainstNegativeOrZero(nameof(compressionThresholdLimitBytes), compressionThresholdLimitBytes);
            return builder.RegisterClientFactory<ServiceBusClient, ServiceBusClientOptions>(options => new CompressionAwareServiceBusClient(connectionString, options, new GzipCompressionConfiguration(compressionThresholdLimitBytes)));
        }
        
    }
}