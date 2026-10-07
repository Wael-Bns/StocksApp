using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using StocksApp.Core.Candles;
using StocksApp.Core.Diagnostics;
using StocksApp.Core.Options;
using StocksApp.Domain.Entities;
using StocksApp.Domain.RepositoryContracts;
using StocksApp.Infrastructure.Services;
using Xunit;

namespace StocksApp.Test.Infrastructure
{
    public class ResilientCandleMatchStoreTest
    {
        private readonly Mock<ICandleMatchRepository> _matchRepoMock = new();
        private readonly Mock<ICandleMatchFlushBackupRepository> _backupRepoMock = new();
        private readonly Mock<ICandleMetrics> _metricsMock = new();
        private readonly IServiceScopeFactory _scopeFactory;

        public ResilientCandleMatchStoreTest()
        {
            var services = new ServiceCollection();
            services.AddSingleton(_matchRepoMock.Object);
            services.AddSingleton(_backupRepoMock.Object);
            _scopeFactory = services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>();
        }

        private ResilientCandleMatchStore CreateSut(int inlineRetryCount = 3) => new(
            _scopeFactory, _metricsMock.Object,
            Options.Create(new MatchCandleStoreOptions
            {
                InlineRetryCount = inlineRetryCount,
                InitialBackoff = TimeSpan.FromMilliseconds(1)
            }),
            Mock.Of<ILogger<ResilientCandleMatchStore>>());

        private static OhlcBar SampleBar(string symbol = "AAPL") =>
            OhlcBar.FromSnapshot(symbol, DateTimeOffset.UtcNow, 100m, 105m, 99m, 102m, 500, 5);

        [Fact]
        public async Task UpsertClosedCandleAsync_RepositorySucceeds_NeverWritesBackup()
        {
            // Act
            await CreateSut().UpsertClosedCandleAsync(SampleBar(), CancellationToken.None);

            // Assert
            _matchRepoMock.Verify(r => r.UpsertAsync(It.IsAny<CandleMatch5s>(), It.IsAny<CancellationToken>()), Times.Once);
            _backupRepoMock.Verify(b => b.AddAsync(It.IsAny<CandleMatchFlushBackup>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpsertClosedCandleAsync_SucceedsOnSecondAttempt_RetriesThenReturns()
        {
            // Arrange
            var callCount = 0;
            _matchRepoMock.Setup(r => r.UpsertAsync(It.IsAny<CandleMatch5s>(), It.IsAny<CancellationToken>()))
                .Returns(() =>
                {
                    callCount++;
                    if (callCount == 1) throw new TimeoutException("transient");
                    return Task.CompletedTask;
                });

            // Act
            await CreateSut().UpsertClosedCandleAsync(SampleBar(), CancellationToken.None);

            // Assert
            callCount.Should().Be(2);
            _backupRepoMock.Verify(b => b.AddAsync(It.IsAny<CandleMatchFlushBackup>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpsertClosedCandleAsync_AllRetriesFail_FallsBackToBackupTable()
        {
            // Arrange
            _matchRepoMock.Setup(r => r.UpsertAsync(It.IsAny<CandleMatch5s>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new TimeoutException("db unreachable"));
            var bar = SampleBar("MSFT");

            // Act
            await CreateSut(inlineRetryCount: 3).UpsertClosedCandleAsync(bar, CancellationToken.None);

            // Assert
            _matchRepoMock.Verify(r => r.UpsertAsync(It.IsAny<CandleMatch5s>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
            _backupRepoMock.Verify(b => b.AddAsync(
                It.Is<CandleMatchFlushBackup>(x => x.Symbol == "MSFT"), It.IsAny<CancellationToken>()), Times.Once);
            _metricsMock.Verify(m => m.MatchBarFlushFailed(), Times.Once);
        }

        [Fact]
        public async Task UpsertClosedCandleAsync_BackupWriteAlsoFails_DoesNotThrow()
        {
            // Arrange — a lost match-bar is logged critically, never
            // allowed to escalate into crashing the tick pipeline itself
            _matchRepoMock.Setup(r => r.UpsertAsync(It.IsAny<CandleMatch5s>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new TimeoutException("db unreachable"));
            _backupRepoMock.Setup(b => b.AddAsync(It.IsAny<CandleMatchFlushBackup>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new TimeoutException("backup also unreachable"));

            // Act
            var act = () => CreateSut(inlineRetryCount: 1).UpsertClosedCandleAsync(SampleBar(), CancellationToken.None);

            // Assert
            await act.Should().NotThrowAsync();
        }
    }
}