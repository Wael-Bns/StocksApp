using StocksApp.Domain.Enums;
using StocksApp.Domain.Events;

namespace StocksApp.Domain.Entities
{
    public class Outbox
    {
        public Guid OutboxId { get; set; }
        public string EventName { get; set; } = null!;
        public string Payload { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }

        public int RetryCount { get; set; }
        public DateTime? NextRetryAt { get; set; }
        public string? LastError { get; set; }
        public OutboxStatus Status { get; set; } = OutboxStatus.Pending;
    }

    public static class OutboxExtensions
    {
        public static Outbox ToOutbox<TEvent>(this TEvent @event) where TEvent : IOutboxEvent
        {
            return new Outbox
            {
                OutboxId = Guid.NewGuid(),
                EventName = TEvent.EventName,
                Payload = System.Text.Json.JsonSerializer.Serialize(@event),
                CreatedAt = DateTime.UtcNow,
                RetryCount = 0,
                Status = OutboxStatus.Pending
            };
        }
    }
}
