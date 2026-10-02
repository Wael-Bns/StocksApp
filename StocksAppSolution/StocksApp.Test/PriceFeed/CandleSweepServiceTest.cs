using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using StocksApp.Core.Options;
using StocksApp.Core.ServiceContracts;
using StocksApp.PriceFeed.BackgroundServices;
using StocksApp.Tests.Common;
using Xunit;

namespace StocksApp.Test.PriceFeed
{
    public class CandleSweepServiceTest : IAsyncLifetime
    {
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(1);
        private static readonly TimeSpan ShortSweepInterval = TimeSpan.FromMilliseconds(20);

        private readonly Mock<IOhlcBarAggregator> _aggregatorMock;
        private CandleSweepService _service = default!;

        public CandleSweepServiceTest()
        {
            _aggregatorMock = new Mock<IOhlcBarAggregator>();
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            await _service.StopAsync(CancellationToken.None);

            // StopAsync suppresses exceptions from ExecuteAsync, so surface a crashed loop here
            if (_service.ExecuteTask is not null)
                _service.ExecuteTask.IsFaulted.Should().BeFalse("the sweep loop must not crash");

            _service.Dispose();
        }

        #region Feature Flag Tests

        [Fact]
        public async Task StartAsync_CandlesDisabled_NeverCallsFlushIdleBars()
        {
            ArrangeService(enabled: false);

            await _service.StartAsync(CancellationToken.None);
            await Task.Delay(ShortSweepInterval * 5);

            FlushIdleBarsCount().Should().Be(0);
        }

        #endregion

        #region Sweep Interval Tests

        [Fact]
        public async Task StartAsync_CandlesEnabled_CallsFlushIdleBarsPeriodically()
        {
            ArrangeService(enabled: true);

            await _service.StartAsync(CancellationToken.None);
            await WaitUntilAsync(() => FlushIdleBarsCount() >= 2, DefaultTimeout);

            FlushIdleBarsCount().Should().BeGreaterThanOrEqualTo(2);
        }

        [Fact]
        public async Task StartAsync_FlushIdleBarsThrows_KeepsSweepingOnNextInterval()
        {
            ArrangeService(enabled: true);
            _aggregatorMock.SetupSequence(a => a.FlushIdleBarsAsync(It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("sweep failed"))
                .Returns(Task.CompletedTask);

            await _service.StartAsync(CancellationToken.None);
            await WaitUntilAsync(() => FlushIdleBarsCount() >= 2, DefaultTimeout);

            FlushIdleBarsCount().Should().BeGreaterThanOrEqualTo(2);
        }

        #endregion

        #region Helpers

        private void ArrangeService(bool enabled)
        {
            _service = new CandleSweepService(
                _aggregatorMock.Object,
                Options.Create(new CandleCacheOptions { Enabled = enabled, SweepInterval = ShortSweepInterval }),
                Mock.Of<ILogger<CandleSweepService>>());
        }

        private int FlushIdleBarsCount() =>
            AsyncTestHelpers.CountInvocations(_aggregatorMock, nameof(IOhlcBarAggregator.FlushIdleBarsAsync));

        private static int CountInvocations<T>(Mock<T> mock, string methodName) where T : class =>
            mock.Invocations.Count(i => i.Method.Name == methodName);

        private static async Task WaitUntilAsync(Func<bool> condition, TimeSpan? timeout = null)
        {
            var deadline = DateTime.UtcNow + (timeout ?? DefaultTimeout);
            while (!condition())
            {
                if (DateTime.UtcNow > deadline)
                    throw new TimeoutException("Condition was not met in time.");
                await Task.Delay(10);
            }
        }

        #endregion
    }
}