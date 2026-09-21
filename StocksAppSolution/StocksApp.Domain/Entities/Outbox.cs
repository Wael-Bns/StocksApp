namespace StocksApp.Domain.Entities
{
    public class Outbox
    {
        public Guid OutboxId { get; set; }
        public Type EventType { get; set; } = null!;
        public string Payload { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }
    public static class OutboxExtensions
    {
        public static Outbox ToOutbox(this object @event)
        {
            return new Outbox
            {
                OutboxId = Guid.NewGuid(),
                EventType = @event.GetType(),
                Payload = System.Text.Json.JsonSerializer.Serialize(@event),
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}
