using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using StocksApp.Domain.Events;
using StocksApp.OrdersWorker.MessageHandlers;
using StocksApp.OrdersWorker.Messages;
using StocksApp.OrdersWorker.ServiceContracts;
using StocksApp.OrdersWorker.Stores;
using Xunit;

namespace StocksApp.Test.ServiceUnitTests
{
    public class SellOrderCreatedMessageHandlerTest
    {
        private readonly Mock<IWorkerSubscriptionsManager> _workerSubscriptionsManagerMock;
        private readonly Mock<ISellOrdersStore> _sellOrdersStoreMock;
        private readonly SellOrderCreatedMessageHandler _handler;

        public SellOrderCreatedMessageHandlerTest()
        {
            _workerSubscriptionsManagerMock = new Mock<IWorkerSubscriptionsManager>();
            _sellOrdersStoreMock = new Mock<ISellOrdersStore>();

            _handler = new SellOrderCreatedMessageHandler(
                NullLogger<SellOrderCreatedMessageHandler>.Instance,
                _workerSubscriptionsManagerMock.Object,
                _sellOrdersStoreMock.Object);
        }

        [Fact]
        public async Task HandleAsync_ValidMessage_AddsOrderAndSubscribesSymbol()
        {
            // Arrange
            var cancellationToken = new CancellationTokenSource().Token;
            var sellOrder = CreateSellOrder("AAPL");
            var message = new SellOrderCreatedWorkerMessage(sellOrder);

            // Act
            await _handler.HandleAsync(message, cancellationToken);

            // Assert
            _sellOrdersStoreMock.Verify(store => store.AddSellOrder(sellOrder), Times.Once);
            _workerSubscriptionsManagerMock.Verify(
                manager => manager.AddStockSymbol(sellOrder.StockSymbol, cancellationToken),
                Times.Once);
        }

        private static SellOrderCreatedCommand CreateSellOrder(string stockSymbol)
        {
            return new SellOrderCreatedCommand
            {
                SellOrderId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                StockSymbol = stockSymbol,
                Price = 100,
                Quantity = 10,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}
