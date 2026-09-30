namespace StocksApp.Domain.Entities
{
    public class TrackedSymbol
    {
        public string Symbol { get; set; } = null!; // always uppercase, e.g. "AAPL"
        public string DisplayName { get; set; } = null!;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}