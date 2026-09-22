using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using StocksApp.Core.ServiceContracts;
using StocksApp.Domain.Events;
using StocksApp.OrdersWorker.MessageHandlers;
using StocksApp.OrdersWorker.Messages;
using StocksApp.OrdersWorker.ServiceContracts;
using StocksApp.OrdersWorker.Stores;
using StocksApp.Tests.Common.Builders;
using Xunit;

namespace StocksApp.Test.OrdersWorker
{
    public class PriceUpdateMessageHandlerTest
    {
        private readonly Mock<IPendingOrdersStore> _sellOrdersStoreMock;
        private readonly Mock<IOrdersExecutionService> _ordersExecutionServiceMock;
        private readonly ServiceProvider _serviceProvider;
        private readonly PriceUpdateMessageHandler _handler;

        public PriceUpdateMessageHandlerTest()
        {
            _sellOrdersStoreMock = new Mock<IPendingOrdersStore>();
            _ordersExecutionServiceMock = new Mock<IOrdersExecutionService>();

            _serviceProvider = new ServiceCollection()
                .AddScoped(_ => _ordersExecutionServiceMock.Object)
                .BuildServiceProvider();

            _handler = new PriceUpdateMessageHandler(
                _serviceProvider.GetRequiredService<IServiceScopeFactory>(),
                _sellOrdersStoreMock.Object,
                new Mock<IPriceFeedSubscriptionRegistry>().Object,
                new Mock<ILogger<PriceUpdateMessageHandler>>().Object);
        }

        [Fact]
        public async Task HandleAsync_NoEligibleOrders_DoesNotExecuteOrders()
        {
            // Arrange
            var message = new PriceUpdateWorkerMessage("AAPL", 100);

            _sellOrdersStoreMock
                .Setup(store => store.TakeTriggeredOrders(message.StockSymbol, message.Price))
                .Returns(Array.Empty<SellOrderCreatedCommand>());

            // Act
            await _handler.HandleAsync(message, CancellationToken.None);

            // Assert
            _ordersExecutionServiceMock.Verify(
                service => service.ExecuteSellOrdersAsync(It.IsAny<IReadOnlyCollection<SellOrderCreatedCommand>>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task HandleAsync_EligibleOrders_ExecutesOrders()
        {
            // Arrange
            var cancellationToken = new CancellationTokenSource().Token;
            var message = new PriceUpdateWorkerMessage("AAPL", 120);
            var eligibleOrders = new List<SellOrderCreatedCommand>
            {
                new SellOrderCreatedCommandBuilder().WithStockSymbol("AAPL").WithPrice(100).Build(),
                new SellOrderCreatedCommandBuilder().WithStockSymbol("AAPL").WithPrice(110).Build()
            };

            _sellOrdersStoreMock
                .Setup(store => store.TakeTriggeredOrders(message.StockSymbol, message.Price))
                .Returns(eligibleOrders);

            // Act
            await _handler.HandleAsync(message, cancellationToken);

            // Assert
            _ordersExecutionServiceMock.Verify(
                service => service.ExecuteSellOrdersAsync(eligibleOrders, cancellationToken),
                Times.Once);
        }

        [Fact]
        public async Task HandleAsync_WhenExecutionFails_ReAddsOrdersToStoreAndDoesNotThrow()
        {
            // Arrange
            var message = new PriceUpdateWorkerMessage("AAPL", 120);
            var eligibleOrders = new List<SellOrderCreatedCommand>
    {
        new SellOrderCreatedCommandBuilder().WithStockSymbol("AAPL").WithPrice(100).Build()
    };

            _sellOrdersStoreMock
                .Setup(store => store.TakeTriggeredOrders(message.StockSymbol, message.Price))
                .Returns(eligibleOrders);

            _ordersExecutionServiceMock
                .Setup(service => service.ExecuteSellOrdersAsync(eligibleOrders, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Execution failed"));

            // Act
            Func<Task> actual = async () => await _handler.HandleAsync(message, CancellationToken.None);

            // Assert
            await actual.Should().NotThrowAsync();

            foreach (var order in eligibleOrders)
            {
                _sellOrdersStoreMock.Verify(store => store.AddSellOrder(order), Times.Once);
            }

            _sellOrdersStoreMock.Verify(store => store.HasPendingOrders(It.IsAny<string>()), Times.Never);
        }
    }
}
