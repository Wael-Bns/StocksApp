namespace StocksApp.Domain.Entities
{
    public class Candle1m
    {
        public string Symbol { get; set; } = null!;
        public DateTimeOffset BucketStart { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public long Volume { get; set; }
        public int TradeCount { get; set; }
    }
}
