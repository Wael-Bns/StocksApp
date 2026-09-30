using StocksApp.Core.DTO.StockDTO;

namespace StocksApp.Tests.Common.Builders
{
    public class PriceUpdateMessageBuilder
    {
        private string _stockSymbol = "AAPL";
        private double _price = 150.25;
        private long _timestamp = 1_700_000_000_000; // fixed unix ms, so tests stay deterministic
        private double _volume = 100;

        public PriceUpdateMessageBuilder WithSymbol(string stockSymbol)
        {
            _stockSymbol = stockSymbol;
            return this;
        }

        public PriceUpdateMessageBuilder WithPrice(double price)
        {
            _price = price;
            return this;
        }

        public PriceUpdateMessageBuilder WithTimestamp(long timestamp)
        {
            _timestamp = timestamp;
            return this;
        }

        public PriceUpdateMessageBuilder WithVolume(double volume)
        {
            _volume = volume;
            return this;
        }

        public PriceUpdateMessage Build() => new()
        {
            StockSymbol = _stockSymbol,
            Price = _price,
            Timestamp = _timestamp,
            Volume = _volume
        };
    }
}