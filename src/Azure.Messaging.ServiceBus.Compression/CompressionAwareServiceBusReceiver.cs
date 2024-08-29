using Azure.Messaging.ServiceBus.Compression.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Azure.Messaging.ServiceBus.Compression
{
    public class CompressionAwareServiceBusReceiver : ServiceBusReceiver
    {
        private readonly CompressionConfiguration _configuration;
        private CompressionAwareServiceBusMessageHandler _messageHandler;

        public CompressionAwareServiceBusReceiver()
        {
            _messageHandler = new CompressionAwareServiceBusMessageHandler(_configuration);
        }

        public CompressionAwareServiceBusReceiver(string queueName, ServiceBusClient client,
            CompressionConfiguration configuration, ServiceBusReceiverOptions options) : base(client, queueName, options)
        {
            Guard.AgainstNull(nameof(configuration), configuration);
            _configuration = configuration;
        }
        
        public CompressionAwareServiceBusReceiver(string topicName,string subscriptionName, ServiceBusClient client,
            CompressionConfiguration configuration, ServiceBusReceiverOptions options) : base(client, topicName,subscriptionName, options)
        {
            Guard.AgainstNull(nameof(configuration), configuration);
            _configuration = configuration;
        }

        public override async Task<IReadOnlyList<ServiceBusReceivedMessage>> ReceiveMessagesAsync(int maxMessages, TimeSpan? maxWaitTime = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var messages =  await base.ReceiveMessagesAsync(maxMessages, maxWaitTime, cancellationToken).ConfigureAwait(false);
            return messages.Select(_messageHandler.HandleMessageReceived).ToList().AsReadOnly();
        }

        public override async Task<ServiceBusReceivedMessage> ReceiveMessageAsync(TimeSpan? maxWaitTime = null,
            CancellationToken cancellationToken = default)
        {
            var message = await base.ReceiveMessageAsync(maxWaitTime, cancellationToken).ConfigureAwait(false);

            return _messageHandler.HandleMessageReceived(message);
        }

        public override async Task<ServiceBusReceivedMessage> PeekMessageAsync(long? fromSequenceNumber = null, CancellationToken cancellationToken = new CancellationToken())
        {
            var message = await  base.PeekMessageAsync(fromSequenceNumber, cancellationToken).ConfigureAwait(false);
            return _messageHandler.HandleMessageReceived(message);
        }

        public override async Task<IReadOnlyList<ServiceBusReceivedMessage>> PeekMessagesAsync(int maxMessages, long? fromSequenceNumber = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var messages =  await base.PeekMessagesAsync(maxMessages, fromSequenceNumber, cancellationToken).ConfigureAwait(false);
            return messages.Select(_messageHandler.HandleMessageReceived).ToList().AsReadOnly();
        }
    }
}
