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
        private readonly Counter<long> _reconnects;

        public PriceFeedMetrics(IMeterFactory meterFactory)
        {
            var meter = meterFactory.Create(MeterName);
            _received = meter.CreateCounter<long>("pricefeed_ticks_received_total");
            _published = meter.CreateCounter<long>("pricefeed_ticks_published_total");
            _dropped = meter.CreateCounter<long>("pricefeed_ticks_dropped_total");
            _isLeader = meter.CreateUpDownCounter<int>("pricefeed_is_leader");
            _socketConnected = meter.CreateUpDownCounter<int>("pricefeed_socket_connected");
            _reconnects = meter.CreateCounter<long>("pricefeed_reconnects_total");

            meter.CreateObservableGauge("pricefeed_symbols_desired", () => Interlocked.Read(ref _desiredSymbols));
            meter.CreateObservableGauge("pricefeed_symbols_actual", () => Interlocked.Read(ref _actualSymbols));
            meter.CreateObservableGauge("pricefeed_last_tick_age_seconds", GetLastTickAgeSeconds);
        }

        private long _desiredSymbols;
        private long _actualSymbols;
        private long _lastTickUnixMs;

        public void ReconnectAttempted() => _reconnects.Add(1);

        public void SymbolCounts(int desired, int actual)
        {
            Interlocked.Exchange(ref _desiredSymbols, desired);
            Interlocked.Exchange(ref _actualSymbols, actual);
        }

        public void TickReceived()
        {
            _received.Add(1);
            Interlocked.Exchange(ref _lastTickUnixMs, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        }

        private double GetLastTickAgeSeconds()
        {
            var last = Interlocked.Read(ref _lastTickUnixMs);
            if (last == 0) return double.NaN;   // no tick received yet this process lifetime
            return (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - last) / 1000.0;
        }
        public void TickPublished() => _published.Add(1);
        public void TickDropped() => _dropped.Add(1);
        public void LeaderAcquired() => _isLeader.Add(1);
        public void LeaderLost() => _isLeader.Add(-1);
        public void SocketConnected() => _socketConnected.Add(1);
        public void SocketDisconnected() => _socketConnected.Add(-1);
    }
}