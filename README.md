# Azure.Messaging.ServiceBus.Compression
Compress and Decompress Azure Servicebus messages with ease.

### This is a library based on [Azure.Messaging.ServiceBus client](https://www.nuget.org/packages/Azure.Messaging.ServiceBus/) 

This library supportors optional compression of Servicebus messages if the size of the body is larger than a defined Threshold.

This can be helpfull when using the standard servicebus where the message cannot be greater then 256Kb.

The library borrows a lot from [ServiceBus.CompressionPlugin](https://github.com/SeanFeldman/ServiceBus.CompressionPlugin) 


### NuGet package

[![NuGet Status](https://buildstats.info/nuget/AzureServicebus.Compression?includePreReleases=true)](https://www.nuget.org/packages/AzureServiceBus.Compression/)

Available here https://www.nuget.org/packages/AzureServiceBus.Compression/

## Codecoverage 
Coverage is calculated using [Coverlet](https://github.com/coverlet-coverage/coverlet) and analysed by Codecov.io   
[![codecov](https://codecov.io/gh/tlogik/Azure.Messaging.ServiceBus.Compression/branch/main/graph/badge.svg?token=wfvbH4xb3F)](https://codecov.io/gh/tlogik/Azure.Messaging.ServiceBus.Compression)

## Examples

### Use extensions to register the CompressionAware ServiceBus Client
To use the ServiceCollection extension please install the nuget package [Microsoft.Extensions.Azure](https://www.nuget.org/packages/Microsoft.Extensions.Azure)

Examples.

Use the default GzipCompressionConfiguration and just define the threshold. If the message body is larger than the threshold the body will be compressed.
This is the simples and recommended approach to instanciate the client.
The default compressionThresholdLimitBytes is 1500 bytes.

```c#
builder.Services.AddAzureClients(builder =>
{
    builder.AddCompressionAwareServiceBusClient(connectionString:"YOUR_SERVICEBUS_CONNECTIONSTRING",compressionThresholdLimitBytes:1500);
});

```


### Instanciate the ServiceBus Client manually
Use default GzipCompressionConfiguration

```c#
var client = new CompressionAwareServiceBusClient(connectionString: "YOUR_SERVICEBUS_CONNECTIONSTRING") { };

```

Use default GzipCompressionConfiguration but define the threshold

```c#
var client = new CompressionAwareServiceBusClient(connectionString: "YOUR_SERVICEBUS_CONNECTIONSTRING", compressionThresholdBytes: 1500) { };

```

Use your own compression configuration

```c#
var compressionConfig = new YouCustomCompressionConfiguration();
var client = new CompressionAwareServiceBusClient(connectionString: "YOUR_SERVICEBUS_CONNECTIONSTRING", configuration: compressionConfig ) { };

```


### Sending and Receiving messages

#### Sending Messages
```c#
//support Cancellation
var cancellationTokenSource = new CancellationTokenSource();
var cancellationToken = cancellationTokenSource.Token;

var client = new CompressionAwareServiceBusClient(connectionString: "YOUR_SERVICEBUS_CONNECTIONSTRING", compressionThresholdBytes:1500) { };

var sender = client.CreateSender(queueOrTopicName: "FOO_QUEUE_OR_TOPIC");

//Create a message
var fooBody = new string('*', 2000);
var message = new ServiceBusMessage(fooBody);

//Send it. If body is greater than threshold it will compressed
await sender.SendMessageAsync(message, cancellationToken: cancellationToken).ConfigureAwait(false);

```

Please check the class [CompressionAwareServiceBusSender](https://github.com/tlogik/Azure.Messaging.ServiceBus.Compression/blob/main/src/Azure.Messaging.ServiceBus.Compression/CompressionAwareServiceBusSender.cs)
to see which methods has been overridden.

#### Receiving Messages
```c#
//support Cancellation
var cancellationTokenSource = new CancellationTokenSource();
var cancellationToken = cancellationTokenSource.Token;

var receiverConfig = new ServiceBusReceiverOptions()
{
    PrefetchCount = 10,
    ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete
};
var receiver = client.CreateReceiver(topicName: "MyTopic", subscriptionName: "MyTopic_Subscription", receiverConfig);


//The messages will be decompressed if required.
var messages = await receiver.ReceiveMessagesAsync(10, cancellationToken: cancellationToken).ConfigureAwait(false);

//Process the messages

```

Please check the class [CompressionAwareServiceBusReceiver](https://github.com/tlogik/Azure.Messaging.ServiceBus.Compression/blob/main/src/Azure.Messaging.ServiceBus.Compression/CompressionAwareServiceBusReceiver.cs)
to see which methods has been overridden.

### Receiving messages in Azure Functions

To receive compressed messages in Azure Functions that use the `[ServiceBusTrigger]`, register in `Startup.cs`:

``` cs
services.AddCompressionAwareServiceBusMessageHandler();
```

Then use the `CompressionAwareServiceBusMessageHandler` to decompress the message:
``` cs
public class MyFunction {
    private CompressionAwareServiceBusMessageHandler _handler;

    public MyFunction(CompressionAwareServiceBusMessageHandler handler)
    {
        _handler = handler;
    }

    [Function("MyFunction")]
    public async Task Run([ServiceBusTrigger("MyTopic", "MysSubscriptionName", Connection = "ServiceBus")] ServiceBusReceivedMessage message)
    {
        message = _handler.HandleMessageReceived(message);
    }    
}    
``` 

### Custom compressions

Configuration and registration

```c#
var compressionConfig = new CompressionConfiguration(compressionMethodName: "noop", compressor: bytes => Task.FromResult, decompressor: bytes => Task.FromResult, minimumSize: 1);

var client = new CompressionAwareServiceBusClient(connectionString: "YOUR_SERVICEBUS_CONNECTIONSTRING", configuration: compressionConfig ) { };
```    

### Transitioning to a different compression or receiving a different compression

To transition to a different compression or process messages compressed used a different method, additional decompressors can be registered to ensure messages in flight compressed using older/other methods are handled properly.

```c#
compressionConfig = new CompressionConfiguration(/* new version of compression */);
compressionConfig.AddDecompressor(compressionMethodName: "old compression method name", decompressor: bytes => Task.FromResult);
compressionConfig.AddDecompressor(compressionMethodName: "other compression method name", decompressor: bytes => Task.FromResult);

var client = new CompressionAwareServiceBusClient(connectionString: "YOUR_SERVICEBUS_CONNECTIONSTRING", configuration: compressionConfig ) { };
```



## Disclaimer and Info

### Overrides
Only a subset of overrides has been implemented. The overrides should be sufficient to support the purpose of compressing and decompressing message Payloads but if you find something is missing please feel free to fix and create PR.
