namespace Azure.Messaging.ServiceBus.Compression
{
    static class Headers
    {
        public const string CompressionMethodName = "compression-method";
        public const string OriginalBodySize = "compression-original-size";
        public const string CompressedBodySize = "compression-compressed-size";
        public const string CompressionOrigin = "compression-origin";
    }
}