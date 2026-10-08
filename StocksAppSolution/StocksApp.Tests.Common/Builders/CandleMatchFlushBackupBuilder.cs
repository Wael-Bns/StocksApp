using System;
using StocksApp.Domain.Entities;

namespace StocksApp.Tests.Common.Builders
{
    public class CandleMatchFlushBackupBuilder
    {
        private Guid _id = Guid.NewGuid();
        private string _symbol = "AAPL";
        private DateTimeOffset _bucketStart = new(2026, 10, 4, 14, 0, 0, TimeSpan.Zero);
        private decimal _open = 100m;
        private decimal _high = 100m;
        private decimal _low = 100m;
        private decimal _close = 100m;
        private long _volume = 100;
        private int _tradeCount = 1;
        private DateTime _failedAt = DateTime.UtcNow;

        public CandleMatchFlushBackupBuilder WithId(Guid id) { _id = id; return this; }
        public CandleMatchFlushBackupBuilder WithSymbol(string symbol) { _symbol = symbol; return this; }
        public CandleMatchFlushBackupBuilder WithBucketStart(DateTimeOffset bucketStart) { _bucketStart = bucketStart; return this; }
        public CandleMatchFlushBackupBuilder WithHigh(decimal high) { _high = high; return this; }
        public CandleMatchFlushBackupBuilder WithOhlc(decimal open, decimal high, decimal low, decimal close)
        {
            _open = open; _high = high; _low = low; _close = close;
            return this;
        }
        public CandleMatchFlushBackupBuilder WithVolume(long volume) { _volume = volume; return this; }
        public CandleMatchFlushBackupBuilder WithTradeCount(int tradeCount) { _tradeCount = tradeCount; return this; }
        public CandleMatchFlushBackupBuilder WithFailedAt(DateTime failedAt) { _failedAt = failedAt; return this; }

        public CandleMatchFlushBackup Build() => new()
        {
            Id = _id,
            Symbol = _symbol,
            BucketStart = _bucketStart,
            Open = _open,
            High = _high,
            Low = _low,
            Close = _close,
            Volume = _volume,
            TradeCount = _tradeCount,
            FailedAt = _failedAt
        };
    }
}