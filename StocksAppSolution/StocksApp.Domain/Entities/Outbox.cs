using StocksApp.Domain.Events;

namespace StocksApp.Domain.Entities
{
    public class Outbox
    {
        public Guid OutboxId { get; set; }
        public string Event { get; set; } = null!;
        public string Payload { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }
    public static class OutboxExtensions
    {
        public static Outbox ToOutbox<TEvent>(this TEvent @event) where TEvent : IOutboxEvent
        {
            return new Outbox
            {
                OutboxId = Guid.NewGuid(),
                Event = TEvent.EventName,
                Payload = System.Text.Json.JsonSerializer.Serialize(@event),
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}
