using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using StocksApp.Core.Diagnostics;
using StocksApp.Core.Options;
using StocksApp.Domain.Entities;
using StocksApp.Domain.RepositoryContracts;
using StocksApp.PriceFeed.BackgroundServices;
using StocksApp.Tests.Common.Builders;
using Xunit;
using static StocksApp.Tests.Common.AsyncTestHelpers;

namespace StocksApp.Test.PriceFeed
{
    public class MatchBarBackupRetryServiceTest : IAsyncLifetime
    {
        private static readonly TimeSpan ShortInterval = TimeSpan.FromMilliseconds(20);

        private readonly Mock<ICandleMatchFlushBackupRepository> _backupRepoMock;
        private readonly Mock<ICandleMatchRepository> _matchRepoMock;
        private readonly Mock<ICandleMetrics> _metricsMock;
        private MatchBarBackupRetryService _service = default!;

        public MatchBarBackupRetryServiceTest()
        {
            _backupRepoMock = new Mock<ICandleMatchFlushBackupRepository>();
            _matchRepoMock = new Mock<ICandleMatchRepository>();
            _metricsMock = new Mock<ICandleMetrics>();

            _backupRepoMock.Setup(r => r.CountPendingAsync(It.IsAny<CancellationToken>())).ReturnsAsync(0);
            _backupRepoMock.Setup(r => r.GetPendingAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CandleMatchFlushBackup>());
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            await _service.StopAsync(CancellationToken.None);
            if (_service.ExecuteTask is not null)
                _service.ExecuteTask.IsFaulted.Should().BeFalse("the backup retry loop must not crash");
            _service.Dispose();
        }

        #region Pending Count Reporting

        [Fact]
        public async Task StartAsync_EachPass_ReportsPendingCountToMetrics()
        {
            _backupRepoMock.Setup(r => r.CountPendingAsync(It.IsAny<CancellationToken>())).ReturnsAsync(3);
            ArrangeService();

            await _service.StartAsync(CancellationToken.None);
            await WaitUntilAsync(() => CountInvocations(_metricsMock, nameof(ICandleMetrics.SetMatchBarBackupPending)) >= 1);

            _metricsMock.Verify(m => m.SetMatchBarBackupPending(3), Times.AtLeastOnce);
        }

        #endregion

        #region Relocation

        [Fact]
        public async Task StartAsync_NoPendingRows_NeverCallsUpsertOrRemove()
        {
            ArrangeService();

            await _service.StartAsync(CancellationToken.None);
            await WaitUntilAsync(() =>
                CountInvocations(_backupRepoMock, nameof(ICandleMatchFlushBackupRepository.CountPendingAsync)) >= 2);

            _matchRepoMock.Verify(r => r.UpsertAsync(It.IsAny<CandleMatch5s>(), It.IsAny<CancellationToken>()), Times.Never);
            _backupRepoMock.Verify(r => r.RemoveAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task StartAsync_PendingRowRelocatesSuccessfully_UpsertsRemovesAndRecordsMetric()
        {
            var row = new CandleMatchFlushBackupBuilder().WithSymbol("AAPL").Build();
            _backupRepoMock.Setup(r => r.GetPendingAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CandleMatchFlushBackup> { row });
            ArrangeService();

            await _service.StartAsync(CancellationToken.None);
            await WaitUntilAsync(() =>
                CountInvocations(_backupRepoMock, nameof(ICandleMatchFlushBackupRepository.RemoveAsync)) >= 1);

            _matchRepoMock.Verify(r => r.UpsertAsync(
                It.Is<CandleMatch5s>(c => c.Symbol == "AAPL"), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
            _backupRepoMock.Verify(r => r.RemoveAsync(row.Id, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
            _metricsMock.Verify(m => m.MatchBarBackupRecovered(), Times.AtLeastOnce);
        }

        [Fact]
        public async Task StartAsync_UpsertFailsForOneRow_RowIsNotRemoved_OtherRowsStillProcessed()
        {
            var failing = new CandleMatchFlushBackupBuilder().WithSymbol("FAIL").Build();
            var succeeding = new CandleMatchFlushBackupBuilder().WithSymbol("OK").Build();
            _backupRepoMock.Setup(r => r.GetPendingAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CandleMatchFlushBackup> { failing, succeeding });
            _matchRepoMock.Setup(r => r.UpsertAsync(
                    It.Is<CandleMatch5s>(c => c.Symbol == "FAIL"), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new TimeoutException("still unreachable"));
            ArrangeService();

            await _service.StartAsync(CancellationToken.None);
            await WaitUntilAsync(() =>
                CountInvocations(_backupRepoMock, nameof(ICandleMatchFlushBackupRepository.RemoveAsync)) >= 1);

            _backupRepoMock.Verify(r => r.RemoveAsync(failing.Id, It.IsAny<CancellationToken>()), Times.Never);
            _backupRepoMock.Verify(r => r.RemoveAsync(succeeding.Id, It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }

        #endregion

        #region Resilience

        [Fact]
        public async Task StartAsync_CountPendingThrows_KeepsRetryingOnNextInterval()
        {
            _backupRepoMock.SetupSequence(r => r.CountPendingAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("db unavailable"))
                .ReturnsAsync(0);
            ArrangeService();

            await _service.StartAsync(CancellationToken.None);
            await WaitUntilAsync(() =>
                CountInvocations(_backupRepoMock, nameof(ICandleMatchFlushBackupRepository.CountPendingAsync)) >= 2);

            CountInvocations(_backupRepoMock, nameof(ICandleMatchFlushBackupRepository.CountPendingAsync))
                .Should().BeGreaterThanOrEqualTo(2);
        }

        #endregion

        #region Helpers

        private void ArrangeService()
        {
            var services = new ServiceCollection();
            services.AddSingleton(_backupRepoMock.Object);
            services.AddSingleton(_matchRepoMock.Object);
            var provider = services.BuildServiceProvider();

            _service = new MatchBarBackupRetryService(
                provider.GetRequiredService<IServiceScopeFactory>(),
                _metricsMock.Object,
                Options.Create(new MatchBarBackupRetryOptions { Interval = ShortInterval, BatchSize = 100 }),
                Mock.Of<ILogger<MatchBarBackupRetryService>>());
        }

        #endregion
    }
}