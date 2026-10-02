using StocksApp.Core.DTO.CandleDTO;

namespace StocksApp.Tests.Common.Builders
{
    public class ActiveBarSnapshotBuilder
    {
        private string _symbol = "AAPL";
        private DateTimeOffset _bucketStart = new(2026, 9, 30, 14, 0, 0, TimeSpan.Zero);
        private decimal _open = 100m;
        private decimal _high = 105m;
        private decimal _low = 99m;
        private decimal _close = 102m;
        private long _volume = 1000;
        private int _tradeCount = 10;

        public ActiveBarSnapshotBuilder WithSymbol(string symbol)
        {
            _symbol = symbol;
            return this;
        }

        public ActiveBarSnapshotBuilder WithBucketStart(DateTimeOffset bucketStart)
        {
            _bucketStart = bucketStart;
            return this;
        }

        public ActiveBarSnapshotBuilder WithOhlc(decimal open, decimal high, decimal low, decimal close)
        {
            _open = open;
            _high = high;
            _low = low;
            _close = close;
            return this;
        }

        public ActiveBarSnapshotBuilder WithVolume(long volume)
        {
            _volume = volume;
            return this;
        }

        public ActiveBarSnapshotBuilder WithTradeCount(int tradeCount)
        {
            _tradeCount = tradeCount;
            return this;
        }

        public ActiveBarSnapshot Build() => new(
            _symbol, _bucketStart, _open, _high, _low, _close, _volume, _tradeCount);
    }
}