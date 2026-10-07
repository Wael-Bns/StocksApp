using System.Threading.Channels;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using StocksApp.Core.Diagnostics;
using StocksApp.Core.DTO.StockDTO;
using StocksApp.Core.Options;
using StocksApp.Core.ServiceContracts;
using StocksApp.PriceFeed.BackgroundServices;
using StocksApp.Tests.Common;
using StocksApp.Tests.Common.Builders;
using Xunit;

namespace StocksApp.Test.PriceFeed
{
    public class TickPublisherServiceTest : IAsyncLifetime
    {
        private readonly Channel<PriceUpdateMessage> _channel;
        private readonly Mock<IOhlcBarAggregator> _chartAggregatorMock;
        private readonly Mock<IOhlcBarAggregator> _matchAggregatorMock;
        private readonly Mock<ILatestPriceCacheWriter> _latestPriceCacheWriterMock;
        private readonly Mock<IPriceFeedMetrics> _metricsMock;
        private TickPublisherService _service = default!;

        public TickPublisherServiceTest()
        {
            _channel = Channel.CreateUnbounded<PriceUpdateMessage>();
            _chartAggregatorMock = new Mock<IOhlcBarAggregator>();
            _matchAggregatorMock = new Mock<IOhlcBarAggregator>();
            _latestPriceCacheWriterMock = new Mock<ILatestPriceCacheWriter>();
            _metricsMock = new Mock<IPriceFeedMetrics>();
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            await _service.StopAsync(CancellationToken.None);
            if (_service.ExecuteTask is not null)
                _service.ExecuteTask.IsFaulted.Should().BeFalse("the publisher loop must not crash");
            _service.Dispose();
        }

        #region Feature Flag Tests

        [Fact]
        public async Task CandlesDisabled_NeverCallsAggregatorOrLatestPriceTracker()
        {
            ArrangeService(candlesEnabled: false);
            await _service.StartAsync(CancellationToken.None);

            await _channel.Writer.WriteAsync(new PriceUpdateMessageBuilder().Build());
            await AsyncTestHelpers.WaitUntilAsync(() => AsyncTestHelpers.CountInvocations(_metricsMock, nameof(IPriceFeedMetrics.TickPublished)) >= 1);

            _chartAggregatorMock.Invocations.Should().BeEmpty();
            _latestPriceCacheWriterMock.Invocations.Should().BeEmpty();
        }
        [Fact]
        public async Task CandlesAndMatchingBothEnabled_EveryTick_FeedsBothAggregatorsIndependently()
        {
            ArrangeService(candlesEnabled: true, matchingEnabled: true);
            var tick = new PriceUpdateMessageBuilder().WithSymbol("AAPL").Build();

            await _service.StartAsync(CancellationToken.None);
            await _channel.Writer.WriteAsync(tick);

            await AsyncTestHelpers.WaitUntilAsync(() =>
                AsyncTestHelpers.CountInvocations(_chartAggregatorMock, nameof(IOhlcBarAggregator.ApplyTickAsync)) >= 1 &&
                AsyncTestHelpers.CountInvocations(_matchAggregatorMock, nameof(IOhlcBarAggregator.ApplyTickAsync)) >= 1);

            _chartAggregatorMock.Verify(a => a.ApplyTickAsync(
                It.Is<PriceUpdateMessage>(m => m.StockSymbol == "AAPL"), It.IsAny<CancellationToken>()), Times.Once);
            _matchAggregatorMock.Verify(a => a.ApplyTickAsync(
                It.Is<PriceUpdateMessage>(m => m.StockSymbol == "AAPL"), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task MatchingDisabled_ChartEnabled_MatchAggregatorNeverCalled()
        {
            ArrangeService(candlesEnabled: true, matchingEnabled: false);

            await _service.StartAsync(CancellationToken.None);
            await _channel.Writer.WriteAsync(new PriceUpdateMessageBuilder().Build());

            await AsyncTestHelpers.WaitUntilAsync(() =>
                AsyncTestHelpers.CountInvocations(_chartAggregatorMock, nameof(IOhlcBarAggregator.ApplyTickAsync)) >= 1);

            _matchAggregatorMock.Invocations.Should().BeEmpty();
        }
        #endregion

        #region Independence Tests

        [Fact]
        public async Task CandlesEnabled_EveryTick_TracksLatestPriceRegardlessOfAggregatorOutcome()
        {
            ArrangeService(candlesEnabled: true);
            _chartAggregatorMock.Setup(a => a.ApplyTickAsync(It.IsAny<PriceUpdateMessage>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("aggregation failed"));
            var tick = new PriceUpdateMessageBuilder().WithSymbol("AAPL").Build();

            await _service.StartAsync(CancellationToken.None);
            await _channel.Writer.WriteAsync(tick);

            await AsyncTestHelpers.WaitUntilAsync(() =>
                AsyncTestHelpers.CountInvocations(_latestPriceCacheWriterMock, nameof(ILatestPriceCacheWriter.WriteAsync)) >= 1);

            // a failing aggregator must never block latest-price tracking — independent blocks
            _latestPriceCacheWriterMock.Verify(t => t.WriteAsync(
                It.Is<PriceUpdateMessage>(m => m.StockSymbol == "AAPL"), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CandlesEnabled_AggregatorThrows_PublishingAndLatestPriceStillProceed()
        {
            ArrangeService(candlesEnabled: true);
            _chartAggregatorMock.Setup(a => a.ApplyTickAsync(It.IsAny<PriceUpdateMessage>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("aggregation failed"));

            await _service.StartAsync(CancellationToken.None);
            await _channel.Writer.WriteAsync(new PriceUpdateMessageBuilder().Build());

            await AsyncTestHelpers.WaitUntilAsync(() => AsyncTestHelpers.CountInvocations(_metricsMock, nameof(IPriceFeedMetrics.TickPublished)) >= 1);
            await AsyncTestHelpers.WaitUntilAsync(() =>
                AsyncTestHelpers.CountInvocations(_latestPriceCacheWriterMock, nameof(ILatestPriceCacheWriter.WriteAsync)) >= 1);
        }

        [Fact]
        public async Task CandlesEnabled_LatestPriceTrackerThrows_AggregationStillProceeds()
        {
            ArrangeService(candlesEnabled: true);
            _latestPriceCacheWriterMock.Setup(t => t.WriteAsync(It.IsAny<PriceUpdateMessage>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("tracking failed"));

            await _service.StartAsync(CancellationToken.None);
            await _channel.Writer.WriteAsync(new PriceUpdateMessageBuilder().Build());

            await AsyncTestHelpers.WaitUntilAsync(() =>
               AsyncTestHelpers.CountInvocations(_chartAggregatorMock, nameof(IOhlcBarAggregator.ApplyTickAsync)) >= 1);
        }

        #endregion

        #region Helpers

        private void ArrangeService(bool candlesEnabled = false, bool matchingEnabled = false)
        {
            _service = new TickPublisherService(
                _channel.Reader,
                Mock.Of<MassTransit.IBus>(),
                _chartAggregatorMock.Object,
                _matchAggregatorMock.Object,
                _latestPriceCacheWriterMock.Object,
                _metricsMock.Object,
                Options.Create(new CandleCacheOptions { Enabled = candlesEnabled }),
                Options.Create(new OrderMatchingOptions { Enabled = matchingEnabled }),
                Mock.Of<ILogger<TickPublisherService>>());
        }

        #endregion
    }
}