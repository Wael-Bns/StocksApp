using FluentAssertions;
using Moq;
using StocksApp.Core.WebSocketClientAbstractions;
using StocksApp.OrdersWorker.Services;
using Xunit;

namespace StocksApp.Test.ServiceUnitTests
{
    public class WorkerSubscriptionsManagerTest
    {
        private readonly Mock<IFinnhubWebSocketClient> _finnhubWebSocketClientMock;
        private readonly WorkerSubscriptionsManager _manager;

        public WorkerSubscriptionsManagerTest()
        {
            _finnhubWebSocketClientMock = new Mock<IFinnhubWebSocketClient>();

            _manager = new WorkerSubscriptionsManager(_finnhubWebSocketClientMock.Object);
        }

        [Fact]
        public async Task AddStockSymbol_NewSymbol_SubscribesToWebSocket()
        {
            // Act
            await _manager.AddStockSymbol("AAPL", CancellationToken.None);

            // Assert
            _finnhubWebSocketClientMock.Verify(
                client => client.SubscribeAsync("AAPL", It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task AddStockSymbol_DuplicateSymbol_SubscribesOnlyOnce()
        {
            // Act
            await _manager.AddStockSymbol("AAPL", CancellationToken.None);
            await _manager.AddStockSymbol("AAPL", CancellationToken.None);

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
            await _manager.AddStockSymbol(stockSymbol!, CancellationToken.None);

            // Assert
            _finnhubWebSocketClientMock.Verify(
                client => client.SubscribeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
