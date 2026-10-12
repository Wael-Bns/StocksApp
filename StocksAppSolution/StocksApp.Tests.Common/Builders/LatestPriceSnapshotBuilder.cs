// StocksApp.Tests.Common/Builders/LatestPriceSnapshotBuilder.cs
using StocksApp.Core.DTO.CandleDTO;

namespace StocksApp.Tests.Common.Builders
{
    public class LatestPriceSnapshotBuilder
    {
        private string _symbol = "AAPL";
        private decimal _price = 150m;
        private long _volume = 25;
        private long _timestamp = 123456789;

        public LatestPriceSnapshotBuilder WithSymbol(string symbol)
        {
            _symbol = symbol;
            return this;
        }

        public LatestPriceSnapshotBuilder WithPrice(decimal price)
        {
            _price = price;
            return this;
        }

        public LatestPriceSnapshotBuilder WithVolume(long volume)
        {
            _volume = volume;
            return this;
        }

        public LatestPriceSnapshotBuilder WithTimestamp(long timestamp)
        {
            _timestamp = timestamp;
            return this;
        }
        public LatestPriceSnapshotBuilder WithTimestamp(DateTimeOffset timestamp)
        {
            _timestamp = timestamp.ToUnixTimeMilliseconds();
            return this;
        }

        public LatestPriceSnapshot Build() => new(_symbol, _price, _volume, _timestamp);
    }
}