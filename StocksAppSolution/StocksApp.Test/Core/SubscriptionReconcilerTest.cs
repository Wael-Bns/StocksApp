using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using StocksApp.Core.ServiceContracts;
using StocksApp.Core.Services;
using StocksApp.IntegrationsTests.Fakes;
using Xunit;

namespace StocksApp.Test.Core
{
    public class SubscriptionReconcilerTest
    {
        private readonly Mock<ITrackedSymbolStore> _storeMock;
        private readonly RecordingFinnhubWebSocketClient _client;
        private readonly SubscriptionReconciler _reconciler;

        public SubscriptionReconcilerTest()
        {
            _storeMock = new Mock<ITrackedSymbolStore>();
            _client = new RecordingFinnhubWebSocketClient();
            _reconciler = new SubscriptionReconciler(
                _storeMock.Object, _client, Mock.Of<ILogger<SubscriptionReconciler>>());
        }

        [Fact]
        public async Task ReconcileAsync_FirstPass_SubscribesToEveryDesiredSymbol()
        {
            // Arrange
            SetupStore("AAPL", "MSFT");

            // Act
            await _reconciler.ReconcileAsync(CancellationToken.None);

            // Assert
            _client.SubscribeCalls.Should().BeEquivalentTo(new[] { "AAPL", "MSFT" });
            _client.UnsubscribeCalls.Should().BeEmpty();
        }

        [Fact]
        public async Task ReconcileAsync_SecondPass_OnlyAppliesTheDiff()
        {
            // Arrange
            SetupStore("AAPL", "MSFT");
            await _reconciler.ReconcileAsync(CancellationToken.None);
            SetupStore("AAPL", "NVDA"); // MSFT removed, NVDA added

            // Act
            await _reconciler.ReconcileAsync(CancellationToken.None);

            // Assert
            _client.SubscribeCalls.Should().BeEquivalentTo(new[] { "AAPL", "MSFT", "NVDA" });
            _client.UnsubscribeCalls.Should().BeEquivalentTo(new[] { "MSFT" });
        }

        [Fact]
        public async Task ReconcileAsync_NoChangeSinceLastPass_MakesNoClientCalls()
        {
            // Arrange
            SetupStore("AAPL");
            await _reconciler.ReconcileAsync(CancellationToken.None);

            // Act
            await _reconciler.ReconcileAsync(CancellationToken.None); // same set again

            // Assert
            _client.SubscribeCalls.Should().ContainSingle();
            _client.UnsubscribeCalls.Should().BeEmpty();
        }

        [Fact]
        public async Task ReconcileAsync_StoreThrows_LeavesActualSetUntouchedAndDoesNotThrow()
        {
            // Arrange
            SetupStore("AAPL");
            await _reconciler.ReconcileAsync(CancellationToken.None);
            _storeMock.Setup(s => s.GetActiveSymbolsAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new TimeoutException("db unreachable"));

            // Act
            var act = () => _reconciler.ReconcileAsync(CancellationToken.None);

            // Assert
            await act.Should().NotThrowAsync();

            // desired set unchanged once the DB is back: no new subscribe calls needed
            SetupStore("AAPL");
            await _reconciler.ReconcileAsync(CancellationToken.None);
            _client.SubscribeCalls.Should().ContainSingle();
        }

        [Fact]
        public async Task ReconcileAsync_PartialFailureMidPass_LeavesActualAccurate_NextPassFinishesTheRest()
        {
            // Arrange
            var failedOnce = false;
            _client.OnSubscribe = symbol =>
            {
                if (symbol == "MSFT" && !failedOnce) { failedOnce = true; throw new InvalidOperationException("boom"); }
                return Task.CompletedTask;
            };
            SetupStore("AAPL", "MSFT");

            // Act
            var act = () => _reconciler.ReconcileAsync(CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>();
            _client.SubscribeCalls.Should().ContainSingle(s => s == "AAPL"); // succeeded before the failure

            await _reconciler.ReconcileAsync(CancellationToken.None); // retry pass
            _client.SubscribeCalls.Should().BeEquivalentTo(new[] { "AAPL", "MSFT" });
        }

        [Fact]
        public async Task Reset_ClearsActualSet_SoEverythingResubscribesNextPass()
        {
            // Arrange — simulate: subscribed to AAPL on the old socket, then the socket dropped
            SetupStore("AAPL");
            await _reconciler.ReconcileAsync(CancellationToken.None);

            // Act
            _reconciler.Reset();
            await _reconciler.ReconcileAsync(CancellationToken.None);

            // Assert
            _client.SubscribeCalls.Should().BeEquivalentTo(new[] { "AAPL", "AAPL" });
        }
        [Fact]
        public async Task ReconcileAsync_SymbolRemoved_RaisesSymbolUnsubscribed()
        {
            // Arrange
            SetupStore("AAPL", "MSFT");
            await _reconciler.ReconcileAsync(CancellationToken.None);
            SetupStore("AAPL"); // MSFT removed

            var raisedFor = new List<string>();
            _reconciler.SymbolUnsubscribed += (symbol, _) => { raisedFor.Add(symbol); return Task.CompletedTask; };

            // Act
            await _reconciler.ReconcileAsync(CancellationToken.None);

            // Assert
            raisedFor.Should().BeEquivalentTo(new[] { "MSFT" });
        }

        [Fact]
        public async Task ReconcileAsync_HandlerThrows_DoesNotAbortReconcileOrResubscribe()
        {
            // Arrange
            SetupStore("AAPL", "MSFT", "TSLA");
            await _reconciler.ReconcileAsync(CancellationToken.None);
            SetupStore("AAPL");
            _reconciler.SymbolUnsubscribed += (_, _) => throw new InvalidOperationException("cleanup failed");

            // Act
            var act = () => _reconciler.ReconcileAsync(CancellationToken.None);

            // Assert — the reconcile pass itself must complete normally
            await act.Should().NotThrowAsync();
            _client.UnsubscribeCalls.Should().BeEquivalentTo(new[] { "MSFT", "TSLA" });
            _client.SubscribeCalls.Should().BeEquivalentTo(new[] { "AAPL", "MSFT", "TSLA" });
        }
        private void SetupStore(params string[] symbols) =>
            _storeMock.Setup(s => s.GetActiveSymbolsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlySet<string>)new HashSet<string>(symbols, StringComparer.Ordinal));
    }
}