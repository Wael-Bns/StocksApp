using System.Diagnostics.Metrics;

namespace StocksApp.PriceFeed.Diagnostics
{
    public sealed class PriceFeedMetrics
    {
        public const string MeterName = "StocksApp.PriceFeed";

        private readonly Counter<long> _received;
        private readonly Counter<long> _published;
        private readonly Counter<long> _dropped;

        public PriceFeedMetrics(IMeterFactory meterFactory)
        {
            var meter = meterFactory.Create(MeterName);
            _received = meter.CreateCounter<long>("pricefeed_ticks_received_total");
            _published = meter.CreateCounter<long>("pricefeed_ticks_published_total");
            _dropped = meter.CreateCounter<long>("pricefeed_ticks_dropped_total");
        }

        public void TickReceived() => _received.Add(1);
        public void TickPublished() => _published.Add(1);
        public void TickDropped() => _dropped.Add(1);
    }
}