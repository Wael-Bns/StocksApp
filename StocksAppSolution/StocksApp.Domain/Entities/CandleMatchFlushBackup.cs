namespace StocksApp.Domain.Entities
{
    public class CandleMatchFlushBackup
    {
        public Guid Id { get; set; }
        public string Symbol { get; set; } = null!;
        public DateTimeOffset BucketStart { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public long Volume { get; set; }
        public int TradeCount { get; set; }
        public DateTime FailedAt { get; set; }
    }
}