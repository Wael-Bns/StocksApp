namespace StocksApp.Core.Exceptions
{
    public class OutboxDeserializationException : Exception
    {
        public OutboxDeserializationException(Type eventType, string payload)
            : base($"Failed to deserialize payload for event type '{eventType.FullName}'. Payload: {payload}")
        {
        }
    }
}
