// StocksApp.Test.PriceFeed/TickPublisherServiceTest.cs
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
        private readonly Mock<IOhlcBarAggregator> _aggregatorMock;
        private readonly Mock<ILatestPriceCacheWriter> _latestPriceTrackerMock;
        private readonly Mock<IPriceFeedMetrics> _metricsMock;
        private TickPublisherService _service = default!;

        public TickPublisherServiceTest()
        {
            _channel = Channel.CreateUnbounded<PriceUpdateMessage>();
            _aggregatorMock = new Mock<IOhlcBarAggregator>();
            _latestPriceTrackerMock = new Mock<ILatestPriceCacheWriter>();
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

            _aggregatorMock.Invocations.Should().BeEmpty();
            _latestPriceTrackerMock.Invocations.Should().BeEmpty();
        }

        #endregion

        #region Independence Tests

        [Fact]
        public async Task CandlesEnabled_EveryTick_TracksLatestPriceRegardlessOfAggregatorOutcome()
        {
            ArrangeService(candlesEnabled: true);
            _aggregatorMock.Setup(a => a.ApplyTickAsync(It.IsAny<PriceUpdateMessage>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("aggregation failed"));
            var tick = new PriceUpdateMessageBuilder().WithSymbol("AAPL").Build();

            await _service.StartAsync(CancellationToken.None);
            await _channel.Writer.WriteAsync(tick);

            await AsyncTestHelpers.WaitUntilAsync(() =>
                AsyncTestHelpers.CountInvocations(_latestPriceTrackerMock, nameof(ILatestPriceCacheWriter.WriteAsync)) >= 1);

            // a failing aggregator must never block latest-price tracking — independent blocks
            _latestPriceTrackerMock.Verify(t => t.WriteAsync(
                It.Is<PriceUpdateMessage>(m => m.StockSymbol == "AAPL"), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CandlesEnabled_AggregatorThrows_PublishingAndLatestPriceStillProceed()
        {
            ArrangeService(candlesEnabled: true);
            _aggregatorMock.Setup(a => a.ApplyTickAsync(It.IsAny<PriceUpdateMessage>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("aggregation failed"));

            await _service.StartAsync(CancellationToken.None);
            await _channel.Writer.WriteAsync(new PriceUpdateMessageBuilder().Build());

            await AsyncTestHelpers.WaitUntilAsync(() => AsyncTestHelpers.CountInvocations(_metricsMock, nameof(IPriceFeedMetrics.TickPublished)) >= 1);
            await AsyncTestHelpers.WaitUntilAsync(() =>
                AsyncTestHelpers.CountInvocations(_latestPriceTrackerMock, nameof(ILatestPriceCacheWriter.WriteAsync)) >= 1);
        }

        [Fact]
        public async Task CandlesEnabled_LatestPriceTrackerThrows_AggregationStillProceeds()
        {
            ArrangeService(candlesEnabled: true);
            _latestPriceTrackerMock.Setup(t => t.WriteAsync(It.IsAny<PriceUpdateMessage>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("tracking failed"));

            await _service.StartAsync(CancellationToken.None);
            await _channel.Writer.WriteAsync(new PriceUpdateMessageBuilder().Build());

            await AsyncTestHelpers.WaitUntilAsync(() =>
               AsyncTestHelpers.CountInvocations(_aggregatorMock, nameof(IOhlcBarAggregator.ApplyTickAsync)) >= 1);
        }

        #endregion

        #region Helpers

        private void ArrangeService(bool candlesEnabled)
        {
            _service = new TickPublisherService(
                _channel.Reader,
                Mock.Of<MassTransit.IBus>(),
                _aggregatorMock.Object,
                _latestPriceTrackerMock.Object,
                _metricsMock.Object,
                Options.Create(new CandleCacheOptions { Enabled = candlesEnabled }),
                Mock.Of<ILogger<TickPublisherService>>());
        }

        #endregion
    }
}