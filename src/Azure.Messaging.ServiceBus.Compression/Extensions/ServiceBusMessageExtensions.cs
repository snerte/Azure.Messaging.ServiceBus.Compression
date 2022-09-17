using System;
using System.Linq;

namespace Azure.Messaging.ServiceBus.Compression.Extensions
{
    
    public static class ServiceBusMessageExtensions
    {
        /// <summary>
        /// Check if the message should be compressed.
        /// Exposed to ensure it can be used in other scenarios where the <see cref="CompressionAwareServiceBusSender"/>
        /// should not be instanciated but the compression logic should be available
        /// </summary>
        /// <param name="message">The message to check </param>
        /// <param name="configuration">The compression configuration</param>
        /// <param name="bodyAsBytes">returns the message body as bytes</param>
        /// <returns>true of the body should be compressed</returns>
        public static bool ShouldBeCompressed(this ServiceBusMessage message, CompressionConfiguration configuration, out byte[] bodyAsBytes)
        {
            Guard.AgainstNull(nameof(configuration), configuration);
            bodyAsBytes = Array.Empty<byte>();
            var bytes = message.Body?.ToArray();
            
            if (bytes is null || !bytes.Any()) return false;
            if (bytes.Length < configuration.MinimumSize) return false;
            bodyAsBytes = bytes;
            return true;

        }

        /// <summary>
        /// Compress the body and set it on the message.
        /// Exposed to ensure it can be used in other scenarios where the <see cref="CompressionAwareServiceBusSender"/>
        /// should not be instanciated but the compression logic should be available
        /// </summary>
        /// <param name="message">The message to set the compressed body on</param>
        /// <param name="configuration">Compression Configuration</param>
        /// <param name="bodyAsBytes">The body to compress</param>
        /// <returns></returns>
        public static ServiceBusMessage CompressAndSetMessageBody(this ServiceBusMessage message, CompressionConfiguration configuration, byte[] bodyAsBytes)
        { 
            Guard.AgainstNull(nameof(configuration), configuration);
            
            var compressedBody = configuration.Compressor(bodyAsBytes);
            message.Body = new BinaryData(compressedBody);
            message.ApplicationProperties[Headers.OriginalBodySize] = bodyAsBytes.Length;
            message.ApplicationProperties[Headers.CompressionMethodName] = configuration.CompressionMethodName;
            message.ApplicationProperties[Headers.CompressedBodySize] = compressedBody.Length;
            return message;
        }
    }
}