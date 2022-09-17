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
            return messages.Select(HandleMessageReceived).ToList().AsReadOnly();
        }

        public override async Task<ServiceBusReceivedMessage> ReceiveMessageAsync(TimeSpan? maxWaitTime = null,
            CancellationToken cancellationToken = default)
        {
            var message = await base.ReceiveMessageAsync(maxWaitTime, cancellationToken).ConfigureAwait(false);

            return HandleMessageReceived(message);
        }

        public override async Task<ServiceBusReceivedMessage> PeekMessageAsync(long? fromSequenceNumber = null, CancellationToken cancellationToken = new CancellationToken())
        {
            var message = await  base.PeekMessageAsync(fromSequenceNumber, cancellationToken).ConfigureAwait(false);
            return HandleMessageReceived(message);
        }

        public override async Task<IReadOnlyList<ServiceBusReceivedMessage>> PeekMessagesAsync(int maxMessages, long? fromSequenceNumber = null,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var messages =  await base.PeekMessagesAsync(maxMessages, fromSequenceNumber, cancellationToken).ConfigureAwait(false);
            return messages.Select(HandleMessageReceived).ToList().AsReadOnly();
        }


        internal ServiceBusReceivedMessage HandleMessageReceived(ServiceBusReceivedMessage message)
        {
            
            return !message.IsCompressed( out var compressionMethodName) ? message : DecompressAndSetBody(message, compressionMethodName);
        }
        private ServiceBusReceivedMessage DecompressAndSetBody(ServiceBusReceivedMessage message, string decompressorName)
        {
            var decompressed = message.DeCompress(decompressorName, _configuration);
            return Map(message, decompressed);
        }

        private ServiceBusReceivedMessage Map(ServiceBusReceivedMessage message, byte[] body)
        {
            if (body.Length == 0) return message;

            //It is not really easy to create the body proper - however the ServiceBusMessage is able to. Thus the Hack is.
            // Let ServiceBusMessage create the new body and pass it to the ServiceBusReceivedMessage
            var msg = new ServiceBusMessage(body);
            var raw = message.GetRawAmqpMessage();
            raw.Body = msg.GetRawAmqpMessage().Body; //Is reference and thus actually changes the body on the ServiceBusReceivedMessage
            return message;

        }
    }
}
