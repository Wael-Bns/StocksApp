namespace StocksApp.PriceFeed.Options
{
    public sealed class PriceFeedIngestionOptions
    {
        public const string SectionName = "PriceFeedIngestion";

        public TimeSpan InitialBackoff { get; set; } = TimeSpan.FromSeconds(1);
        public TimeSpan MaxBackoff { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>How long a connection must stay up before a future disconnect resets backoff to InitialBackoff.</summary>
        public TimeSpan StableConnectionThreshold { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>Periodic safety-net reconcile, in case a NOTIFY was missed.</summary>
        public TimeSpan ReconcileInterval { get; set; } = TimeSpan.FromSeconds(60);

        /// <summary>Debounce window after a signal, to absorb bursts of NOTIFYs.</summary>
        public TimeSpan ReconcileDebounce { get; set; } = TimeSpan.FromMilliseconds(250);
    }
}