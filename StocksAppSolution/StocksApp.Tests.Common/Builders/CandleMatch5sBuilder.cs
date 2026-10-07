using StocksApp.Domain.Entities;

namespace StocksApp.Tests.Common.Builders
{
    public class CandleMatch5sBuilder
    {
        private string _symbol = "AAPL";
        private DateTimeOffset _bucketStart = new(2026, 10, 4, 14, 0, 0, TimeSpan.Zero);
        private decimal _open = 100;
        private decimal _high = 100;
        private decimal _low = 100;
        private decimal _close = 100;
        private long _volume = 100;
        private int _tradeCount = 1;

        public CandleMatch5sBuilder WithSymbol(string symbol) { _symbol = symbol; return this; }
        public CandleMatch5sBuilder WithBucketStart(DateTimeOffset bucketStart) { _bucketStart = bucketStart; return this; }
        public CandleMatch5sBuilder WithHigh(decimal high) { _high = high; return this; }
        public CandleMatch5sBuilder WithOhlc(decimal open, decimal high, decimal low, decimal close)
        {
            _open = open; _high = high; _low = low; _close = close;
            return this;
        }

        public CandleMatch5s Build() => new()
        {
            Symbol = _symbol,
            BucketStart = _bucketStart,
            Open = _open,
            High = _high,
            Low = _low,
            Close = _close,
            Volume = _volume,
            TradeCount = _tradeCount
        };
    }
}