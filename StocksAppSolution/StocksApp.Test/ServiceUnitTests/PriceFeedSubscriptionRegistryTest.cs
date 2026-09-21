using FluentAssertions;
using Moq;
using Polly;
using Polly.Registry;
using StocksApp.Core.WebSocketClientAbstractions;
using StocksApp.OrdersWorker.Services;
using Xunit;

namespace StocksApp.Test.ServiceUnitTests
{
    public class PriceFeedSubscriptionRegistryTest
    {
        private readonly Mock<IFinnhubWebSocketClient> _finnhubWebSocketClientMock;
        private readonly PriceFeedSubscriptionRegistry _registry;

        public PriceFeedSubscriptionRegistryTest()
        {
            _finnhubWebSocketClientMock = new Mock<IFinnhubWebSocketClient>();

            _registry = new PriceFeedSubscriptionRegistry(_finnhubWebSocketClientMock.Object,
                CreatePipelineProvider());
        }

        private static ResiliencePipelineProvider<string> CreatePipelineProvider()
        {
            ResiliencePipeline? pipeline = ResiliencePipeline.Empty;

            var providerMock = new Mock<ResiliencePipelineProvider<string>> { CallBase = true };
            providerMock
                .Setup(p => p.TryGetPipeline(It.IsAny<string>(), out pipeline))
                .Returns(true);

            return providerMock.Object;
        }

        #region AddStockSymbol 
        [Fact]
        public async Task AddStockSymbol_NewSymbol_SubscribesToWebSocket()
        {
            // Act
            await _registry.EnsureSubscribedAsync("AAPL", CancellationToken.None);

            // Assert
            _finnhubWebSocketClientMock.Verify(
                client => client.SubscribeAsync("AAPL", It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task AddStockSymbol_DuplicateSymbol_SubscribesOnlyOnce()
        {
            // Act
            await _registry.EnsureSubscribedAsync("AAPL", CancellationToken.None);
            await _registry.EnsureSubscribedAsync("AAPL", CancellationToken.None);

            // Assert
            _finnhubWebSocketClientMock.Verify(
                client => client.SubscribeAsync("AAPL", It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task AddStockSymbol_NullOrEmptySymbol_DoesNotSubscribe(string? stockSymbol)
        {
            // Act
            await _registry.EnsureSubscribedAsync(stockSymbol!, CancellationToken.None);

            // Assert
            _finnhubWebSocketClientMock.Verify(
                client => client.SubscribeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
        #endregion

        #region EnsureSubscribedAsync
        [Fact]
        public async Task EnsureSubscribedAsync_NewSymbol_SubscribesToWebSocket()
        {
            await _registry.EnsureSubscribedAsync("AAPL", CancellationToken.None);

            _finnhubWebSocketClientMock.Verify(
                c => c.SubscribeAsync("AAPL", It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task EnsureSubscribedAsync_DuplicateSymbol_SubscribesOnlyOnce()
        {
            await _registry.EnsureSubscribedAsync("AAPL", CancellationToken.None);
            await _registry.EnsureSubscribedAsync("AAPL", CancellationToken.None);

            _finnhubWebSocketClientMock.Verify(
                c => c.SubscribeAsync("AAPL", It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task EnsureSubscribedAsync_NullOrEmptySymbol_DoesNotSubscribe(string? stockSymbol)
        {
            await _registry.EnsureSubscribedAsync(stockSymbol!, CancellationToken.None);

            _finnhubWebSocketClientMock.Verify(
                c => c.SubscribeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task EnsureSubscribedAsync_WhenClientFails_Throws_AndLaterCallRetriesSubscription()
        {
            _finnhubWebSocketClientMock
                .SetupSequence(c => c.SubscribeAsync("AAPL", It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException())
                .Returns(Task.CompletedTask);

            var act = () => _registry.EnsureSubscribedAsync("AAPL", CancellationToken.None);
            await act.Should().ThrowAsync<InvalidOperationException>();

            // The failure must not leave the symbol marked as subscribed.
            await _registry.EnsureSubscribedAsync("AAPL", CancellationToken.None);

            _finnhubWebSocketClientMock.Verify(
                c => c.SubscribeAsync("AAPL", It.IsAny<CancellationToken>()),
                Times.Exactly(2));
        }
        #endregion

        #region UnsubscribeAsync
        [Fact]
        public async Task UnsubscribeAsync_SubscribedSymbol_UnsubscribesFromWebSocket()
        {
            await _registry.EnsureSubscribedAsync("AAPL", CancellationToken.None);

            await _registry.UnsubscribeAsync("AAPL", CancellationToken.None);

            _finnhubWebSocketClientMock.Verify(
                c => c.UnsubscribeAsync("AAPL", It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task UnsubscribeAsync_SymbolNeverSubscribed_DoesNotCallClient()
        {
            await _registry.UnsubscribeAsync("AAPL", CancellationToken.None);

            _finnhubWebSocketClientMock.Verify(
                c => c.UnsubscribeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task UnsubscribeAsync_NullOrEmptySymbol_DoesNotCallClient(string? stockSymbol)
        {
            await _registry.UnsubscribeAsync(stockSymbol!, CancellationToken.None);

            _finnhubWebSocketClientMock.Verify(
                c => c.UnsubscribeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task UnsubscribeAsync_CalledTwice_UnsubscribesOnlyOnce()
        {
            await _registry.EnsureSubscribedAsync("AAPL", CancellationToken.None);

            await _registry.UnsubscribeAsync("AAPL", CancellationToken.None);
            await _registry.UnsubscribeAsync("AAPL", CancellationToken.None);

            _finnhubWebSocketClientMock.Verify(
                c => c.UnsubscribeAsync("AAPL", It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task UnsubscribeAsync_ThenEnsureSubscribed_SubscribesAgain()
        {
            await _registry.EnsureSubscribedAsync("AAPL", CancellationToken.None);
            await _registry.UnsubscribeAsync("AAPL", CancellationToken.None);
            await _registry.EnsureSubscribedAsync("AAPL", CancellationToken.None);

            _finnhubWebSocketClientMock.Verify(
                c => c.SubscribeAsync("AAPL", It.IsAny<CancellationToken>()),
                Times.Exactly(2));
        }

        [Fact]
        public async Task UnsubscribeAsync_WhenClientFails_Throws_AndSymbolStaysSubscribedSoLaterCallRetries()
        {
            await _registry.EnsureSubscribedAsync("AAPL", CancellationToken.None);

            _finnhubWebSocketClientMock
                .SetupSequence(c => c.UnsubscribeAsync("AAPL", It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException())
                .Returns(Task.CompletedTask);

            var act = () => _registry.UnsubscribeAsync("AAPL", CancellationToken.None);
            await act.Should().ThrowAsync<InvalidOperationException>();

            // The registry must still consider the symbol subscribed, so this call has to reach the client again.
            await _registry.UnsubscribeAsync("AAPL", CancellationToken.None);

            _finnhubWebSocketClientMock.Verify(
                c => c.UnsubscribeAsync("AAPL", It.IsAny<CancellationToken>()),
                Times.Exactly(2));
        }
    }
    #endregion 
}