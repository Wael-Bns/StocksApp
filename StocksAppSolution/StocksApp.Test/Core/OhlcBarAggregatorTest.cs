using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using StocksApp.Core.Candles;
using StocksApp.Core.Diagnostics;
using StocksApp.Core.DTO.CandleDTO;
using StocksApp.Core.Options;
using StocksApp.Core.ServiceContracts;
using StocksApp.Core.Services;
using StocksApp.Tests.Common.Builders;
using Xunit;

namespace StocksApp.Test.Core
{
    public class OhlcBarAggregatorTest
    {
        private readonly Mock<ICandleCache> _cacheMock = new();
        private readonly Mock<ICandleStore> _storeMock = new();
        private readonly Mock<ICandleMetrics> _metricsMock = new();
        private readonly OhlcBarAggregator _aggregator;

        public OhlcBarAggregatorTest()
        {
            _aggregator = new OhlcBarAggregator(
                _cacheMock.Object, _storeMock.Object, _metricsMock.Object,
                Options.Create(new CandleCacheOptions { BucketSize = TimeSpan.FromMinutes(1) }),
                Mock.Of<ILogger<OhlcBarAggregator>>());
        }

        [Fact]
        public async Task ApplyTickAsync_FirstTick_OpensANewBar()
        {
            // Arrange
            var minuteStart = new DateTimeOffset(2026, 9, 30, 14, 5, 0, TimeSpan.Zero);
            var tick = new PriceUpdateMessageBuilder()
                .WithPrice(100).WithVolume(50)
                .WithTimestamp(minuteStart.AddSeconds(10))
                .Build();

            // Act
            await _aggregator.ApplyTickAsync(tick, CancellationToken.None);

            // Assert
            _cacheMock.Verify(c => c.SetActiveBarAsync(
                It.Is<ActiveBarSnapshot>(b =>
                    b.BucketStart == minuteStart &&
                    b.Open == 100m && b.High == 100m && b.Low == 100m && b.Close == 100m &&
                    b.Volume == 50 && b.TradeCount == 1),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ApplyTickAsync_SameBucketTick_UpdatesHighLowCloseVolumeTradeCount()
        {
            // Arrange
            var minuteStart = new DateTimeOffset(2026, 9, 30, 14, 5, 0, TimeSpan.Zero);
            await _aggregator.ApplyTickAsync(new PriceUpdateMessageBuilder()
                .WithPrice(100).WithVolume(50).WithTimestamp(minuteStart.AddSeconds(5)).Build(),
                CancellationToken.None);

            // Act — higher high, lower low, different close, same bucket
            await _aggregator.ApplyTickAsync(new PriceUpdateMessageBuilder()
                .WithPrice(95).WithVolume(30).WithTimestamp(minuteStart.AddSeconds(20)).Build(),
                CancellationToken.None);
            await _aggregator.ApplyTickAsync(new PriceUpdateMessageBuilder()
                .WithPrice(110).WithVolume(20).WithTimestamp(minuteStart.AddSeconds(40)).Build(),
                CancellationToken.None);

            // Assert
            _cacheMock.Verify(c => c.SetActiveBarAsync(
                It.Is<ActiveBarSnapshot>(b =>
                    b.Open == 100m && b.High == 110m && b.Low == 95m && b.Close == 110m &&
                    b.Volume == 100 && b.TradeCount == 3),
                It.IsAny<CancellationToken>()), Times.Once);
            _storeMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ApplyTickAsync_NewBucketTick_FlushesOldBarAndOpensNew()
        {
            // Arrange
            var firstMinute = new DateTimeOffset(2026, 9, 30, 14, 5, 0, TimeSpan.Zero);
            await _aggregator.ApplyTickAsync(new PriceUpdateMessageBuilder()
                .WithPrice(100).WithVolume(50).WithTimestamp(firstMinute.AddSeconds(10)).Build(),
                CancellationToken.None);

            // Act — tick lands in the next minute
            var nextMinute = firstMinute.AddMinutes(1);
            await _aggregator.ApplyTickAsync(new PriceUpdateMessageBuilder()
                .WithPrice(120).WithVolume(10).WithTimestamp(nextMinute.AddSeconds(5)).Build(),
                CancellationToken.None);

            // Assert
            _storeMock.Verify(s => s.UpsertClosedCandleAsync(
                It.Is<OhlcBar>(b => b.BucketStart == firstMinute && b.Close == 100m),
                It.IsAny<CancellationToken>()), Times.Once);
            _cacheMock.Verify(c => c.DeleteActiveBarAsync("AAPL", It.IsAny<CancellationToken>()), Times.Once);
            _cacheMock.Verify(c => c.SetActiveBarAsync(
                It.Is<ActiveBarSnapshot>(b => b.BucketStart == nextMinute && b.Open == 120m),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ApplyTickAsync_OlderBucketTick_IsDropped()
        {
            // Arrange
            var currentMinute = new DateTimeOffset(2026, 9, 30, 14, 5, 0, TimeSpan.Zero);
            await _aggregator.ApplyTickAsync(new PriceUpdateMessageBuilder()
                .WithPrice(100).WithTimestamp(currentMinute.AddSeconds(10)).Build(),
                CancellationToken.None);
            _cacheMock.Invocations.Clear();

            // Act — a tick from a minute before the current bar
            await _aggregator.ApplyTickAsync(new PriceUpdateMessageBuilder()
                .WithPrice(999).WithTimestamp(currentMinute.AddMinutes(-1).AddSeconds(30)).Build(),
                CancellationToken.None);

            // Assert
            _metricsMock.Verify(m => m.TickLateForClosedBucket(), Times.Once);
            _cacheMock.Verify(c => c.SetActiveBarAsync(It.IsAny<ActiveBarSnapshot>(), It.IsAny<CancellationToken>()),
                Times.Never);
            _storeMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ApplyTickAsync_TickForAlreadyFlushedBucket_IsDroppedNotRestarted()
        {
            // Arrange — close the bucket via the idle sweep, so state.Bar is null and
            // LastFlushedBucket is set, matching the guard this test targets
            var minute = new DateTimeOffset(2026, 9, 30, 14, 5, 0, TimeSpan.Zero);
            await _aggregator.ApplyTickAsync(new PriceUpdateMessageBuilder()
                .WithPrice(100).WithTimestamp(minute.AddSeconds(10)).Build(), CancellationToken.None);
            await _aggregator.FlushIdleBarsAsync(minute.AddMinutes(1).AddSeconds(1), CancellationToken.None);
            _cacheMock.Invocations.Clear();
            _storeMock.Invocations.Clear();

            // Act — a late tick still stamped for the now-closed bucket
            await _aggregator.ApplyTickAsync(new PriceUpdateMessageBuilder()
                .WithPrice(999).WithTimestamp(minute.AddSeconds(50)).Build(), CancellationToken.None);

            // Assert — must be dropped, not treated as "no bar yet" and restarted
            _metricsMock.Verify(m => m.TickLateForClosedBucket(), Times.Once);
            _cacheMock.Verify(c => c.SetActiveBarAsync(It.IsAny<ActiveBarSnapshot>(), It.IsAny<CancellationToken>()),
                Times.Never);
            _storeMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task FlushIdleBarsAsync_FlushesOnlyOnceBucketFullyElapsed()
        {
            // Arrange
            var minute = new DateTimeOffset(2026, 9, 30, 14, 5, 0, TimeSpan.Zero);
            await _aggregator.ApplyTickAsync(new PriceUpdateMessageBuilder()
                .WithPrice(100).WithTimestamp(minute.AddSeconds(10)).Build(), CancellationToken.None);

            // Act — bucket hasn't fully elapsed yet (still within the same minute)
            await _aggregator.FlushIdleBarsAsync(minute.AddSeconds(30), CancellationToken.None);

            // Assert
            _storeMock.Verify(s => s.UpsertClosedCandleAsync(It.IsAny<OhlcBar>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task FlushIdleBarsAsync_OnceBucketElapsed_FlushesAndClears()
        {
            // Arrange
            var minute = new DateTimeOffset(2026, 9, 30, 14, 5, 0, TimeSpan.Zero);
            await _aggregator.ApplyTickAsync(new PriceUpdateMessageBuilder()
                .WithPrice(100).WithTimestamp(minute.AddSeconds(10)).Build(), CancellationToken.None);

            // Act
            await _aggregator.FlushIdleBarsAsync(minute.AddMinutes(1).AddSeconds(1), CancellationToken.None);

            // Assert
            _storeMock.Verify(s => s.UpsertClosedCandleAsync(
                It.Is<OhlcBar>(b => b.BucketStart == minute), It.IsAny<CancellationToken>()), Times.Once);
            _cacheMock.Verify(c => c.DeleteActiveBarAsync("AAPL", It.IsAny<CancellationToken>()), Times.Once);

            // cleared afterward: a later sweep pass finds nothing to flush again
            _storeMock.Invocations.Clear();
            await _aggregator.FlushIdleBarsAsync(minute.AddMinutes(5), CancellationToken.None);
            _storeMock.Verify(s => s.UpsertClosedCandleAsync(It.IsAny<OhlcBar>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task HydrateFromCacheAsync_RepopulatesStates_SoNextTickContinuesSameBucket()
        {
            // Arrange
            var bucket = new DateTimeOffset(2026, 9, 30, 14, 5, 0, TimeSpan.Zero);
            var snapshot = new ActiveBarSnapshotBuilder()
                .WithBucketStart(bucket).WithOhlc(100m, 105m, 99m, 102m).WithVolume(500).WithTradeCount(5)
                .Build();
            _cacheMock.Setup(c => c.GetAllActiveBarsAsync(It.IsAny<CancellationToken>()))
                .Returns(ToAsyncEnumerable(snapshot));

            // Act
            await _aggregator.HydrateFromCacheAsync(CancellationToken.None);
            await _aggregator.ApplyTickAsync(new PriceUpdateMessageBuilder()
                .WithSymbol(snapshot.Symbol).WithPrice(108).WithTimestamp(bucket.AddSeconds(45)).Build(),
                CancellationToken.None);

            // Assert — continues the hydrated bucket rather than starting a new one
            _cacheMock.Verify(c => c.SetActiveBarAsync(
                It.Is<ActiveBarSnapshot>(b => b.BucketStart == bucket && b.High == 108m && b.TradeCount == 6),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task FlushAndRemoveSymbolAsync_FlushesOpenBarAndDeletesBothKeys()
        {
            // Arrange
            await _aggregator.ApplyTickAsync(new PriceUpdateMessageBuilder()
                .WithPrice(100).Build(), CancellationToken.None);

            // Act
            await _aggregator.FlushAndRemoveSymbolAsync("AAPL", CancellationToken.None);

            // Assert
            _storeMock.Verify(s => s.UpsertClosedCandleAsync(It.IsAny<OhlcBar>(), It.IsAny<CancellationToken>()),
                Times.Once);
            _cacheMock.Verify(c => c.DeleteActiveBarAsync("AAPL", It.IsAny<CancellationToken>()), Times.Once);
            _cacheMock.Verify(c => c.DeleteLatestPriceAsync("AAPL", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task FlushAndRemoveSymbolAsync_NoOpenBar_StillCleansUpLatestPrice()
        {
            // Act — symbol was never ticked (e.g. untracked before its first tick arrived)
            await _aggregator.FlushAndRemoveSymbolAsync("GHOST", CancellationToken.None);

            // Assert
            _storeMock.Verify(s => s.UpsertClosedCandleAsync(It.IsAny<OhlcBar>(), It.IsAny<CancellationToken>()),
                Times.Never);
            _cacheMock.Verify(c => c.DeleteLatestPriceAsync("GHOST", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Reset_ClearsInMemoryBars_NextTickStartsFreshBucketRegardlessOfPriorOne()
        {
            // Arrange
            var minute = new DateTimeOffset(2026, 9, 30, 14, 5, 0, TimeSpan.Zero);
            await _aggregator.ApplyTickAsync(new PriceUpdateMessageBuilder()
                .WithPrice(100).WithTimestamp(minute.AddSeconds(10)).Build(), CancellationToken.None);

            // Act
            _aggregator.Reset();
            _storeMock.Invocations.Clear();
            await _aggregator.ApplyTickAsync(new PriceUpdateMessageBuilder()
                .WithPrice(200).WithTimestamp(minute.AddSeconds(20)).Build(), CancellationToken.None);

            // Assert — a fresh bar, not a continuation; and no flush happened on Reset itself
            _storeMock.Verify(s => s.UpsertClosedCandleAsync(It.IsAny<OhlcBar>(), It.IsAny<CancellationToken>()),
                Times.Never);
            _cacheMock.Verify(c => c.SetActiveBarAsync(
                It.Is<ActiveBarSnapshot>(b => b.Open == 200m && b.TradeCount == 1),
                It.IsAny<CancellationToken>()), Times.Once);
        }
        [Fact]
        public async Task Reset_ThenFlushIdleBarsImmediately_FindsNothingToFlush()
        {
            // Arrange — a demoted leader's frozen bar must not get swept as if it were genuinely idle
            var minute = new DateTimeOffset(2026, 9, 30, 14, 5, 0, TimeSpan.Zero);
            await _aggregator.ApplyTickAsync(new PriceUpdateMessageBuilder()
                .WithPrice(100).WithTimestamp(minute.AddSeconds(10)).Build(), CancellationToken.None);

            // Act
            _aggregator.Reset();
            await _aggregator.FlushIdleBarsAsync(minute.AddHours(1), CancellationToken.None);   // long past elapsed

            // Assert — nothing to flush, because Reset already discarded the bar
            _storeMock.Verify(s => s.UpsertClosedCandleAsync(It.IsAny<OhlcBar>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task HydrateFromCacheAsync_SymbolAbsentFromCache_RemovesAnyPriorStaleEntry()
        {
            // Arrange — simulate a stale in-memory bar for a symbol the OTHER leader already
            // closed and deleted from Redis while this process was demoted
            await _aggregator.ApplyTickAsync(new PriceUpdateMessageBuilder()
                .WithSymbol("STALE").WithPrice(50).Build(), CancellationToken.None);

            _cacheMock.Setup(c => c.GetAllActiveBarsAsync(It.IsAny<CancellationToken>()))
                .Returns(ToAsyncEnumerable());   // STALE is absent — Redis has nothing for it anymore
            _storeMock.Invocations.Clear();

            // Act
            await _aggregator.HydrateFromCacheAsync(CancellationToken.None);

            // Assert — a fully elapsed sweep must find nothing for STALE: proves it's gone, not
            // just unrefreshed, since a leftover bar would otherwise get swept and re-flushed
            await _aggregator.FlushIdleBarsAsync(DateTimeOffset.UtcNow.AddHours(1), CancellationToken.None);
            _storeMock.Verify(s => s.UpsertClosedCandleAsync(
                It.Is<OhlcBar>(b => b.Symbol == "STALE"), It.IsAny<CancellationToken>()), Times.Never);
        }
        private static async IAsyncEnumerable<ActiveBarSnapshot> ToAsyncEnumerable(params ActiveBarSnapshot[] items)
        {
            foreach (var item in items)
            {
                yield return item;
                await Task.Yield();
            }
        }
    }
}