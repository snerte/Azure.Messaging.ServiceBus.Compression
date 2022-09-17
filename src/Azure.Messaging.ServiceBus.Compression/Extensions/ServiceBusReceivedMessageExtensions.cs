using System;

namespace Azure.Messaging.ServiceBus.Compression.Extensions
{
    public static class ServiceBusReceivedMessageExtensions
    {
        
        
        /// <summary>
        /// Check if the <paramref name="message"/> is compressed.
        /// Exposed to ensure it can be used in other scenarios where the <see cref="CompressionAwareServiceBusReceiver"/>
        /// should not be instanciated but the compression logic should be available
        /// </summary>
        /// <param name="message"></param>
        /// <param name="compressionMethodName">if the message is compressed the name of compressor will be set</param>
        /// <returns>true of the message is compressed</returns>
        /// <exception cref="Exception">if the compressionMethodName is set on the message but is not convertible to a string</exception>
        public static bool IsCompressed(this ServiceBusReceivedMessage message, out string compressionMethodName)
        {
            compressionMethodName = string.Empty;

            // Is it compressed?
            if (!message.ApplicationProperties.TryGetValue(Headers.CompressionMethodName,
                    out var compressionName)) return false;
            
            //Is the compressionMethodName valid?
            compressionMethodName = compressionName as string ?? throw new Exception(
                $"{nameof(ServiceBusReceivedMessage)}. {Headers.CompressionMethodName} is set in message but value is not STRING as expected. No valid decompressor can be selected.");

            return true;
        }

        /// <summary>
        /// Will try and decompress using the provided <paramref name="compressionMethodName"/> and <paramref name="configuration"/>
        /// Mark it WILL NOT check if the message is compressed. That is the responsibility of the caller to verify.
        /// Exposed to ensure it can be used in other scenarios where the <see cref="CompressionAwareServiceBusReceiver"/>
        /// should not be instanciated but the compression logic should be available
        /// </summary>
        /// <param name="message">The message to decompress the body</param>
        /// <param name="compressionMethodName">The decompressor name</param>
        /// <param name="configuration">The Compression configuration</param>
        /// <returns>the decompressed body as bytearray.</returns>
        /// <exception cref="Exception">If the decompressed data does not have the same length as the original data</exception>
        public static byte[] DeCompressToByteArray(this ServiceBusReceivedMessage message, string compressionMethodName,
            CompressionConfiguration configuration)
        {
            var decompressed = configuration.Decompressors(compressionMethodName, message.Body.ToArray());
            if (decompressed is null) return message.Body.ToArray();
            if (!decompressed.Length.Equals(message.ApplicationProperties[Headers.OriginalBodySize]))
                throw new Exception($"decompressed Size: {decompressed.Length} bytes does not equal the expected size of: {message.ApplicationProperties[Headers.OriginalBodySize]}");

            return decompressed;
        }
        
        /// <summary>
        /// Will try and decompress using the provided <paramref name="compressionMethodName"/> and <paramref name="configuration"/>
        /// Mark it WILL NOT check if the message is compressed. That is the responsibility of the caller to verify.
        /// Exposed to ensure it can be used in other scenarios where the <see cref="CompressionAwareServiceBusReceiver"/>
        /// should not be instanciated but the compression logic should be available
        /// </summary>
        /// <param name="message">The message to decompress the body</param>
        /// <param name="compressionMethodName">The decompressor name</param>
        /// <param name="configuration">The Compression configuration</param>
        /// <returns>the decompressed body as bytearray.</returns>
        /// <exception cref="Exception">If the decompressed data does not have the same length as the original data</exception>
        public static BinaryData DeCompressToBinaryData(this ServiceBusReceivedMessage message, string compressionMethodName,
            CompressionConfiguration configuration)
        {
            var decompressed = configuration.Decompressors(compressionMethodName, message.Body.ToArray());
            if (decompressed is null) return message.Body;
            if (!decompressed.Length.Equals(message.ApplicationProperties[Headers.OriginalBodySize]))
                throw new Exception($"decompressed Size: {decompressed.Length} bytes does not equal the expected size of: {message.ApplicationProperties[Headers.OriginalBodySize]}");

            var msg = new ServiceBusMessage(decompressed);
            return msg.Body;
        }
    }
}