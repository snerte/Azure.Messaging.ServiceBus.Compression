namespace Azure.Messaging.ServiceBus.Compression.Extensions
{
    internal static class ServiceBusCompressionAwareFactory
    {
        internal static CompressionAwareServiceBusSender CreateCompressionAwareSender(
            this ServiceBusClient client,
            string queueOrTopicName,
            CompressionConfiguration compressionConfiguration)
        {
            return new CompressionAwareServiceBusSender(queueOrTopicName, client, compressionConfiguration);
        }
        
        internal static CompressionAwareServiceBusSender CreateCompressionAwareSender(
            this ServiceBusClient client,
            string queueOrTopicName,
            ServiceBusSenderOptions options,
            CompressionConfiguration compressionConfiguration)
        {
            return new CompressionAwareServiceBusSender(queueOrTopicName, client, compressionConfiguration, options);
        }

        internal static CompressionAwareServiceBusReceiver CreateCompressionAwareReceiver(
            this ServiceBusClient client,
            string queueName,
            CompressionConfiguration compressionConfiguration,
            ServiceBusReceiverOptions? options = default)
        {
            return new CompressionAwareServiceBusReceiver(queueName, client, compressionConfiguration,
                options ?? new ServiceBusReceiverOptions());
        }

        internal static CompressionAwareServiceBusReceiver CreateCompressionAwareReceiver(
            this ServiceBusClient client,
            string topicName,
            string subscriptionName,
            CompressionConfiguration compressionConfiguration,
            ServiceBusReceiverOptions? options = default)
        {
            return new CompressionAwareServiceBusReceiver(topicName, subscriptionName, client, compressionConfiguration,
                options ?? new ServiceBusReceiverOptions());
        }
        
        
        
    }

}