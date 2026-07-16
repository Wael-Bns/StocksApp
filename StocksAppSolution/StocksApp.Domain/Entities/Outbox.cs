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
}
