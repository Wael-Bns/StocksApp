using System.Diagnostics.Metrics;

namespace StocksApp.PriceFeed.Diagnostics
{
    public sealed class PriceFeedMetrics : IPriceFeedMetrics
    {
        public const string MeterName = "StocksApp.PriceFeed";

        private readonly Counter<long> _received;
        private readonly Counter<long> _published;
        private readonly Counter<long> _dropped;
        private readonly UpDownCounter<int> _isLeader;
        private readonly UpDownCounter<int> _socketConnected;

        public PriceFeedMetrics(IMeterFactory meterFactory)
        {
            var meter = meterFactory.Create(MeterName);
            _received = meter.CreateCounter<long>("pricefeed_ticks_received_total");
            _published = meter.CreateCounter<long>("pricefeed_ticks_published_total");
            _dropped = meter.CreateCounter<long>("pricefeed_ticks_dropped_total");
            _isLeader = meter.CreateUpDownCounter<int>("pricefeed_is_leader");
            _socketConnected = meter.CreateUpDownCounter<int>("pricefeed_socket_connected");
        }

        public void TickReceived() => _received.Add(1);
        public void TickPublished() => _published.Add(1);
        public void TickDropped() => _dropped.Add(1);
        public void LeaderAcquired() => _isLeader.Add(1);
        public void LeaderLost() => _isLeader.Add(-1);
        public void SocketConnected() => _socketConnected.Add(1);
        public void SocketDisconnected() => _socketConnected.Add(-1);
    }
}