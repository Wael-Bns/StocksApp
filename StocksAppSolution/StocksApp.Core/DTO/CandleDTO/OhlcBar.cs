using StocksApp.Core.DTO.StockDTO;

namespace StocksApp.Core.Candles
{
    public sealed class OhlcBar
    {
        public string Symbol { get; }
        public DateTimeOffset BucketStart { get; }
        public decimal Open { get; }
        public decimal High { get; }
        public decimal Low { get; }
        public decimal Close { get; }
        public long Volume { get; }
        public int TradeCount { get; }

        private OhlcBar(string symbol, DateTimeOffset bucketStart, decimal open, decimal high,
            decimal low, decimal close, long volume, int tradeCount)
        {
            Symbol = symbol;
            BucketStart = bucketStart;
            Open = open; High = high; Low = low; Close = close;
            Volume = volume; TradeCount = tradeCount;
        }

        public static OhlcBar StartNew(PriceUpdateMessage tick, DateTimeOffset bucketStart) =>
            new(tick.StockSymbol, bucketStart, (decimal)tick.Price, (decimal)tick.Price, (decimal)tick.Price, (decimal)tick.Price, (long)tick.Volume, 1);

        public OhlcBar Apply(PriceUpdateMessage tick) => new(
            Symbol, BucketStart, Open,
            Math.Max(High, (decimal)tick.Price), Math.Min(Low, (decimal)tick.Price), (decimal)tick.Price,
            Volume + (long)tick.Volume, TradeCount + 1);

        /// <summary>Reconstructs a bar from a persisted Redis snapshot — not derived from a tick.</summary>
        public static OhlcBar FromSnapshot(string symbol, DateTimeOffset bucketStart, decimal open,
            decimal high, decimal low, decimal close, long volume, int tradeCount) =>
            new(symbol, bucketStart, open, high, low, close, volume, tradeCount);
    }
}