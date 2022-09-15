using FluentAssertions;
using Xunit.Categories;

namespace Azure.Messaging.ServiceBus.Compression.Test;

public class CompressionAwareServiceBusSenderTest
{
    [Fact]
    [UnitTest]
    public async Task BeforeSend_Should_Set_ApplicationProperties_If_Body_Is_Compressed()
    {
        //ARRANGE
        var minimumCompressionThresholdBytes = 256;
        var compressionConfiguration =
            new GzipCompressionConfiguration(minimumCompressionThresholdBytes);
        var sut = new CompressionAwareServiceBusSender("FOO",
            new ServiceBusClient(Constants.FakeServiceBusConnectionString), compressionConfiguration);
        var body = Helpers.RandomString(minimumCompressionThresholdBytes * 2);
        var message = new ServiceBusMessage(body);

        //ACT
        var processed = await sut.BeforeMessageSend(message).ConfigureAwait(false);

        //ASSERT

        processed.ApplicationProperties.Keys.Count().Should().Be(3,
            "the sender sets 3 attributes to define the compression has been imposed on the message");

        ((int) processed.ApplicationProperties[Headers.OriginalBodySize]).Should().Be(body.Length,
            "the original bodysize should be the same as the body of the input body");

        processed.ApplicationProperties[Headers.CompressionMethodName].ToString().Should().Be(
            compressionConfiguration.CompressionMethodName,
            "the name of the compression algorithm used should be set on the message");

        ((int) processed.ApplicationProperties[Headers.CompressedBodySize]).Should().BeLessThan(body.Length,
            "the compressed body should have less size then the original body");
    }


    [Fact]
    [UnitTest]
    public async Task BeforeSend_Should_Compress_Body_If_OriginalBodySize_Is_Greater_Than_Threshold()
    {
        //ARRANGE
        var minimumCompressionThresholdBytes = 256;
        var compressionConfiguration =
            new GzipCompressionConfiguration(minimumCompressionThresholdBytes);
        var sut = new CompressionAwareServiceBusSender("FOO",
            new ServiceBusClient(Constants.FakeServiceBusConnectionString), compressionConfiguration);
        var body = Helpers.RandomString(minimumCompressionThresholdBytes * 2);
        var message = new ServiceBusMessage(body);

        //ACT
        var processed = await sut.BeforeMessageSend(message).ConfigureAwait(false);

        //ASSERT
        // If compressed the compression header is set
        processed.ApplicationProperties[Headers.CompressionMethodName].ToString().Should().Be(
            compressionConfiguration.CompressionMethodName,
            "the name of the compression algorithm used should be set on the message");

        processed.Body.Should().NotBe(body,
            "the body of the processed message should be compressed thus not like the original body");
    }
    
    [Fact]
    [UnitTest]
    public async Task BeforeSend_Should_Not_Compress_Body_If_OriginalBodySize_Is_Less_Than_Threshold()
    {
        //ARRANGE
        var minimumCompressionThresholdBytes = 256;
        var compressionConfiguration =
            new GzipCompressionConfiguration(minimumCompressionThresholdBytes);
        var sut = new CompressionAwareServiceBusSender("FOO",
            new ServiceBusClient(Constants.FakeServiceBusConnectionString), compressionConfiguration);
        var body = Helpers.RandomString(minimumCompressionThresholdBytes >> 1); //Divide by 2
        var message = new ServiceBusMessage(body);

        //ACT
        var processed = await sut.BeforeMessageSend(message).ConfigureAwait(false);

        //ASSERT
        processed.ApplicationProperties.Keys.Count().Should().Be(0,
            "the sender should not set properties if not compressing");
    }

    [Fact]
    [UnitTest]
    public async Task BeforeSend_Should_Compress_Body_Can_Be_Decompressed_To_Original_Body()
    {
        //ARRANGE
        var minimumCompressionThresholdBytes = 256;
        var compressionConfiguration =
            new GzipCompressionConfiguration(minimumCompressionThresholdBytes);
        var fakeServicebusClient = new ServiceBusClient(Constants.FakeServiceBusConnectionString);
        var sut = new CompressionAwareServiceBusSender("FOO", fakeServicebusClient, compressionConfiguration);

        var receiver = new CompressionAwareServiceBusReceiver("FOO", fakeServicebusClient, compressionConfiguration,
            new ServiceBusReceiverOptions());

        var originalBody = Helpers.RandomString(minimumCompressionThresholdBytes * 2);
        var message = new ServiceBusMessage(originalBody);

        //ACT
        var processed = await sut.BeforeMessageSend(message).ConfigureAwait(false);

        var fakeReceivedMessage =
            ServiceBusModelFactory.ServiceBusReceivedMessage(processed.Body,
                properties: processed.ApplicationProperties);
        var decompressedMessage = receiver.AfterMessageReceived(fakeReceivedMessage);

        //ASSERT
        // If compressed the compression header is set
        processed.ApplicationProperties[Headers.CompressionMethodName].ToString().Should().Be(
            compressionConfiguration.CompressionMethodName,
            "the name of the compression algorithm used should be set on the message");

        processed.Body.ToString().Should().NotBe(originalBody,
            "the body of the processed message should be compressed thus not like the original body");

        decompressedMessage.Body.ToString().Should()
            .Be(originalBody, "the decompressed body must be equal to the original body");
    }
    
    [Fact]
    [UnitTest]
    public async Task BeforeSend_NonCompressed_Message_Should_Have_Same_Body_In_Received_Message()
    {
        //ARRANGE
        var minimumCompressionThresholdBytes = 256;
        var fakeServicebusClient = new ServiceBusClient(Constants.FakeServiceBusConnectionString);
        var compressionConfiguration =
            new GzipCompressionConfiguration(minimumCompressionThresholdBytes);
        var sut = new CompressionAwareServiceBusSender("FOO",
            fakeServicebusClient, compressionConfiguration);
        
        var receiver = new CompressionAwareServiceBusReceiver("FOO", fakeServicebusClient, compressionConfiguration,
            new ServiceBusReceiverOptions());
        
        
        var body = Helpers.RandomString(minimumCompressionThresholdBytes >> 1); //Divide by 2
        var message = new ServiceBusMessage(body);

        //ACT
        var processed = await sut.BeforeMessageSend(message).ConfigureAwait(false);
        
        
        var fakeReceivedMessage =
            ServiceBusModelFactory.ServiceBusReceivedMessage(processed.Body,
                properties: processed.ApplicationProperties);
        var receivedMessage = receiver.AfterMessageReceived(fakeReceivedMessage);

        //ASSERT
        processed.ApplicationProperties.Keys.Count().Should().Be(0,
            "the sender should not set properties if not compressing");
        receivedMessage.ApplicationProperties.Keys.Count().Should().Be(0,
            "the sender should not set properties if not compressing");
        receivedMessage.Body.ToString().Should().Be(body);
    }
}