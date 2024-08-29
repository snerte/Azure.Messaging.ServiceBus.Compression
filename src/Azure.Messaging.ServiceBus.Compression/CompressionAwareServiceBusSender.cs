using Azure.Messaging.ServiceBus.Compression.Extensions;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Amqp.Framing;
using System.Collections.Generic;

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
        
        public override async Task SendMessagesAsync(IEnumerable<ServiceBusMessage> messages, CancellationToken cancellationToken = new CancellationToken())
        {
            var processedMessages = messages.Select(BeforeMessageSend);
            await base.SendMessagesAsync(processedMessages, cancellationToken);
        }
        
        public override Task<IReadOnlyList<long>> ScheduleMessagesAsync(IEnumerable<ServiceBusMessage> messages, DateTimeOffset scheduledEnqueueTime,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var processedMessages = messages.Select(BeforeMessageSend);
            return base.ScheduleMessagesAsync(processedMessages, scheduledEnqueueTime, cancellationToken);
        }
        
        internal ServiceBusMessage BeforeMessageSend(ServiceBusMessage message)
        {
            return !message.ShouldBeCompressed(_configuration, out var bodyAsBytes) ? message : message.CompressAndSetMessageBody(_configuration, bodyAsBytes);
        }
    }
}
