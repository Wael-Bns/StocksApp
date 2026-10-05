namespace StocksApp.Core.Options
{
    public sealed class MatchCandleStoreOptions
    {
        public const string SectionName = "OrderMatching:FlushRetry";
        public int InlineRetryCount { get; set; } = 3;
        public TimeSpan InitialBackoff { get; set; } = TimeSpan.FromMilliseconds(200);
    }
}
