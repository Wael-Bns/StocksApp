namespace StocksApp.Infrastructure.Options
{
    public sealed class CandleCacheOptions
    {
        public const string SectionName = "PriceFeed:Candles";

        public bool Enabled { get; set; } = false;
        public string RedisConnectionString { get; set; } = string.Empty;
        public TimeSpan BucketSize { get; set; } = TimeSpan.FromMinutes(1);
        public TimeSpan SweepInterval { get; set; } = TimeSpan.FromSeconds(20);
    }
}