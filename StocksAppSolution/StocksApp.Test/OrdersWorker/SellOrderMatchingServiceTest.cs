using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using StocksApp.Core.Options;
using StocksApp.Core.ServiceContracts;
using StocksApp.OrdersWorker.BackgroundServices;
using Xunit;
using static StocksApp.Tests.Common.AsyncTestHelpers;

namespace StocksApp.Test.OrdersWorker
{
    public class SellOrderMatchingServiceTest : IAsyncLifetime
    {
        private static readonly TimeSpan ShortMatcherInterval = TimeSpan.FromMilliseconds(20);

        private readonly Mock<IOrderMatcher> _matcherMock;
        private SellOrderMatchingService _service = default!;

        public SellOrderMatchingServiceTest()
        {
            _matcherMock = new Mock<IOrderMatcher>();
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            await _service.StopAsync(CancellationToken.None);
            if (_service.ExecuteTask is not null)
                _service.ExecuteTask.IsFaulted.Should().BeFalse("the matching loop must not crash");
            _service.Dispose();
        }

        #region Periodic Execution

        [Fact]
        public async Task StartAsync_CallsRunOnceAsyncPeriodically()
        {
            ArrangeService();

            await _service.StartAsync(CancellationToken.None);
            await WaitUntilAsync(() => RunOnceCount() >= 2);

            RunOnceCount().Should().BeGreaterThanOrEqualTo(2);
        }

        [Fact]
        public async Task StartAsync_EachPassUsesAFreshScope()
        {
            var scopeFactoryMock = new Mock<IServiceScopeFactory>();
            var createdScopes = 0;
            var services = new ServiceCollection();
            services.AddSingleton(_matcherMock.Object);
            var provider = services.BuildServiceProvider();

            scopeFactoryMock.Setup(f => f.CreateScope()).Returns(() =>
            {
                createdScopes++;
                return provider.CreateScope();
            });

            _service = new SellOrderMatchingService(
                scopeFactoryMock.Object,
                Options.Create(new OrderMatchingOptions { MatcherInterval = ShortMatcherInterval }),
                Mock.Of<ILogger<SellOrderMatchingService>>());

            await _service.StartAsync(CancellationToken.None);
            await WaitUntilAsync(() => RunOnceCount() >= 2);

            createdScopes.Should().BeGreaterThanOrEqualTo(2);
        }

        #endregion

        #region Resilience

        [Fact]
        public async Task StartAsync_RunOnceAsyncThrows_KeepsMatchingOnNextInterval()
        {
            ArrangeService();
            _matcherMock.SetupSequence(m => m.RunOnceAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("db unavailable"))
                .Returns(Task.CompletedTask);

            await _service.StartAsync(CancellationToken.None);
            await WaitUntilAsync(() => RunOnceCount() >= 2);

            RunOnceCount().Should().BeGreaterThanOrEqualTo(2);
        }

        #endregion

        #region Helpers

        private void ArrangeService()
        {
            var services = new ServiceCollection();
            services.AddSingleton(_matcherMock.Object);
            var provider = services.BuildServiceProvider();

            _service = new SellOrderMatchingService(
                provider.GetRequiredService<IServiceScopeFactory>(),
                Options.Create(new OrderMatchingOptions { MatcherInterval = ShortMatcherInterval }),
                Mock.Of<ILogger<SellOrderMatchingService>>());
        }

        private int RunOnceCount() => CountInvocations(_matcherMock, nameof(IOrderMatcher.RunOnceAsync));

        #endregion
    }
}