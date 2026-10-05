namespace StocksApp.Core.Options
{
    public sealed class OrderMatchingOptions
    {
        public const string SectionName = "OrderMatching";
        public bool Enabled { get; set; } = true;
        public TimeSpan MatchBucketSize { get; set; } = TimeSpan.FromSeconds(5);
        public TimeSpan MaxImmediateFillPriceAge { get; set; } = TimeSpan.FromSeconds(10);
        public TimeSpan MatcherInterval { get; set; } = TimeSpan.FromSeconds(5);
        public TimeSpan MatcherGracePeriod { get; set; } = TimeSpan.FromSeconds(2);
    }
}
