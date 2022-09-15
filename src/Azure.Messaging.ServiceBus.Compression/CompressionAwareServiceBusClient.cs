using System.Diagnostics.CodeAnalysis;
using Azure.Messaging.ServiceBus.Compression.Extensions;

namespace Azure.Messaging.ServiceBus.Compression
{
    public class CompressionAwareServiceBusClient : ServiceBusClient
    {
        private readonly CompressionConfiguration _configuration;

        public CompressionAwareServiceBusClient(string connectionString, int compressionThresholdBytes  = GzipCompressionConfiguration.MinimumCompressionSize) : this(connectionString, new GzipCompressionConfiguration(compressionThresholdBytes))
        {
            
        }
        public CompressionAwareServiceBusClient(string connectionString, CompressionConfiguration configuration) : base(connectionString)
        {
            _configuration = configuration;
            Guard.AgainstNull(nameof(configuration), configuration);
        }
        
        public CompressionAwareServiceBusClient(string connectionString, ServiceBusClientOptions options, CompressionConfiguration configuration) : base(connectionString, options)
        {
            _configuration = configuration;
            Guard.AgainstNull(nameof(configuration), configuration);
        }
        
        
        public override ServiceBusSender CreateSender(string queueOrTopicName)
        {
            return this.CreateCompressionAwareSender(queueOrTopicName, _configuration);
        }

        public override ServiceBusSender CreateSender(string queueOrTopicName, ServiceBusSenderOptions options)
        {
            return this.CreateCompressionAwareSender(queueOrTopicName,options, _configuration);
        }

        public override ServiceBusReceiver CreateReceiver(string queueName)
        {
            return this.CreateCompressionAwareReceiver(queueName, _configuration);
        }

        public override ServiceBusReceiver CreateReceiver(string queueName, ServiceBusReceiverOptions? options)
        {
            return this.CreateCompressionAwareReceiver(queueName,_configuration, options);
        }

        public override ServiceBusReceiver CreateReceiver(string topicName, string subscriptionName)
        {
            return this.CreateCompressionAwareReceiver(topicName, subscriptionName, _configuration);
        }

        public override ServiceBusReceiver CreateReceiver(string topicName, string subscriptionName, ServiceBusReceiverOptions? options)
        {
            return this.CreateCompressionAwareReceiver(topicName, subscriptionName, _configuration, options);
        }
    }
}