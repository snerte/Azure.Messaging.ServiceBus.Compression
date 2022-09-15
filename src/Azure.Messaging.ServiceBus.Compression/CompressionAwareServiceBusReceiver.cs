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
            return messages.Select(AfterMessageReceived).ToList().AsReadOnly();
        }

        public override async Task<ServiceBusReceivedMessage> ReceiveMessageAsync(TimeSpan? maxWaitTime = null,
            CancellationToken cancellationToken = default)
        {
            var message = await base.ReceiveMessageAsync(maxWaitTime, cancellationToken).ConfigureAwait(false);

            return AfterMessageReceived(message);
        }
        

        internal ServiceBusReceivedMessage AfterMessageReceived(ServiceBusReceivedMessage message)
        {
            if (!ShouldDeCompress(message, out var compressionMethodName)) return message;

            return DecompressAndSetBody(message, compressionMethodName);

        }


        private bool ShouldDeCompress(ServiceBusReceivedMessage message, out string compressionMethodName)
        {
            compressionMethodName = string.Empty;

            // Is it compressed?
            if (!message.ApplicationProperties.TryGetValue(Headers.CompressionMethodName,
                    out var compressionName)) return false;
            
            //Is the compressionMethodName valid?
            compressionMethodName = compressionName as string ?? throw new Exception(
                $"{nameof(ServiceBusReceivedMessage)}. {Headers.CompressionMethodName} is set in message but value is not STRING as exptected. No valid decompressor can be selected.");

            return true;
        }

        private ServiceBusReceivedMessage DecompressAndSetBody(ServiceBusReceivedMessage message, string decompressorName)
        {
            var decompressed = _configuration.Decompressors(decompressorName, message.Body.ToArray());
            if (decompressed is null) return message;
            if (!decompressed.Length.Equals(message.ApplicationProperties[Headers.OriginalBodySize]))
                throw new Exception($"decompressed Size: {decompressed.Length} bytes does not equal the expected size of: {message.ApplicationProperties[Headers.OriginalBodySize]}");
            
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
