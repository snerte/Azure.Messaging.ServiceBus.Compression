using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Amqp.Framing;

namespace Azure.Messaging.ServiceBus.Compression
{
    public class CompressionAwareServiceBusSender : ServiceBusSender
    {
        private readonly CompressionConfiguration _configuration;

        public CompressionAwareServiceBusSender(string queueOrTopicName, ServiceBusClient client, CompressionConfiguration configuration): base(client, queueOrTopicName)
        {
            Guard.AgainstNull(nameof(configuration), configuration);
            _configuration = configuration;
        }       
        
        public CompressionAwareServiceBusSender(string queueOrTopicName, ServiceBusClient client, CompressionConfiguration configuration,ServiceBusSenderOptions options): base(client, queueOrTopicName, options)
        {
            Guard.AgainstNull(nameof(configuration), configuration);
            _configuration = configuration;
        }
        
        public override async Task SendMessageAsync(ServiceBusMessage message, CancellationToken cancellationToken = default)
        {
            message = await BeforeMessageSend(message).ConfigureAwait(false);
            await base.SendMessageAsync(message, cancellationToken).ConfigureAwait(false);
        }



        internal async Task<ServiceBusMessage> BeforeMessageSend(ServiceBusMessage message)
        {
            //Check conditions for not compressing
            if (!ShouldCompress(message, out var bodyAsBytes)) return await Task.FromResult(message); 

            //We need to compress   
            return await CompressAndSetMessageBody(message, bodyAsBytes);
        }

        private bool ShouldCompress(ServiceBusMessage message, out byte[] bodyAsBytes)
        {
            bodyAsBytes = Array.Empty<byte>();
            var bytes = message.Body?.ToArray();
            
            if (bytes is null || !bytes.Any()) return false;
            if (bytes.Length < _configuration.MinimumSize) return false;
            bodyAsBytes = bytes;
            return true;

        }

        private async Task<ServiceBusMessage> CompressAndSetMessageBody(ServiceBusMessage message, byte[] bodyAsBytes)
        {
            var compressedBody = _configuration.Compressor(bodyAsBytes);
            message.Body = new BinaryData(compressedBody);
            message.ApplicationProperties[Headers.OriginalBodySize] = bodyAsBytes.Length;
            message.ApplicationProperties[Headers.CompressionMethodName] = _configuration.CompressionMethodName;
            message.ApplicationProperties[Headers.CompressedBodySize] = compressedBody.Length;
            return await Task.FromResult(message);
        }
    }
}
