using System.Text.Json.Serialization;

namespace StocksApp.Domain.Notifications
{
    public class OutboxNotification
    {
        [JsonPropertyName("outbox_id")]
        public string OutboxId { get; set; } = null!;
        [JsonPropertyName("event_type")]
        public string EventType { get; set; } = null!;
        [JsonPropertyName("payload")]
        public string Payload { get; set; } = null!;
        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
