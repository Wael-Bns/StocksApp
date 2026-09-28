using System.Text.Json.Serialization;

namespace StocksApp.Infrastructure.MessageBroker
{
    public class MasstransitEnvelope<T>
    {
        [JsonPropertyName("message")]
        public T Message { get; init; } = default!;
    }
}
