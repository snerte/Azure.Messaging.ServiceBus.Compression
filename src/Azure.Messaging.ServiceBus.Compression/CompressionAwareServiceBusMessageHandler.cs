using Azure.Messaging.ServiceBus.Compression.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Azure.Messaging.ServiceBus.Compression
{
    public class CompressionAwareServiceBusMessageHandler
    {
        private readonly CompressionConfiguration _configuration;

        public CompressionAwareServiceBusMessageHandler(CompressionConfiguration configuration)
        {
            _configuration = configuration;
        }

        public ServiceBusReceivedMessage HandleMessageReceived(ServiceBusReceivedMessage message)
        {
            return !message.IsCompressed(out var compressionMethodName) ? message : DecompressAndSetBody(message, compressionMethodName);
        }
        private ServiceBusReceivedMessage DecompressAndSetBody(ServiceBusReceivedMessage message, string decompressorName)
        {
            var decompressed = message.DeCompressToByteArray(decompressorName, _configuration);
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
