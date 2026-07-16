namespace StocksApp.Domain.Entities
{
    public class Outbox
    {
        public Guid OutboxId { get; set; }
        public string EventType { get; set; } = null!;
        public string AggregateType { get; set; } = null!;
        public string AggregateId { get; set; } = null!;
        public string Payload { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }
}
