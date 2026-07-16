using Microsoft.Extensions.DependencyInjection;
using Moq;
using StocksApp.Domain.Entities;
using StocksApp.Domain.Enums;
using StocksApp.Domain.RepositoryContracts;
using StocksApp.Domain.Specifications;
using StocksApp.OrdersWorker.ServiceContracts;
using StocksApp.OrdersWorker.Services;
using StocksApp.OrdersWorker.Stores;
using Xunit;

namespace StocksApp.Test.ServiceUnitTests
{
    public class PendingOrdersInitializerTest
    {
        private readonly Mock<ISellOrdersStore> _sellOrdersStoreMock;
        private readonly Mock<IOrderRepository> _orderRepositoryMock;
        private readonly Mock<IWorkerSubscriptionsManager> _workerSubscriptionsManagerMock;
        private readonly ServiceProvider _serviceProvider;
        private readonly PendingOrdersInitializer _pendingOrdersInitializer;

        public PendingOrdersInitializerTest()
        {
            _sellOrdersStoreMock = new Mock<ISellOrdersStore>();
            _orderRepositoryMock = new Mock<IOrderRepository>();
            _workerSubscriptionsManagerMock = new Mock<IWorkerSubscriptionsManager>();

            _serviceProvider = new ServiceCollection()
                .AddScoped(_ => _orderRepositoryMock.Object)
                .BuildServiceProvider();

            _pendingOrdersInitializer = new PendingOrdersInitializer(
                _sellOrdersStoreMock.Object,
                _serviceProvider.GetRequiredService<IServiceScopeFactory>(),
                _workerSubscriptionsManagerMock.Object);
        }

        [Fact]
        public async Task StartAsync_PendingOrders_AddsOrdersAndSubscribesSymbols()
        {
            // Arrange
            var cancellationToken = new CancellationTokenSource().Token;
            var pendingOrders = new List<SellOrder>
            {
                CreateSellOrder("AAPL"),
                CreateSellOrder("MSFT")
            };

            _orderRepositoryMock
                .Setup(repo => repo.GetSellOrdersBySpecificationAsNoTracking(It.IsAny<ISpecification<SellOrder>>()))
                .ReturnsAsync(pendingOrders);

            // Act
            await _pendingOrdersInitializer.StartAsync(cancellationToken);

            // Assert
            _sellOrdersStoreMock.Verify(store => store.AddSellOrder(
                It.Is<Domain.Events.SellOrderCreatedCommand>(order => order.StockSymbol == "AAPL")), Times.Once);
            _sellOrdersStoreMock.Verify(store => store.AddSellOrder(
                It.Is<Domain.Events.SellOrderCreatedCommand>(order => order.StockSymbol == "MSFT")), Times.Once);

            _workerSubscriptionsManagerMock.Verify(
                manager => manager.AddStockSymbol("AAPL", cancellationToken),
                Times.Once);
            _workerSubscriptionsManagerMock.Verify(
                manager => manager.AddStockSymbol("MSFT", cancellationToken),
                Times.Once);
        }

        [Fact]
        public async Task StartAsync_NoPendingOrders_DoesNotAddOrSubscribe()
        {
            // Arrange
            _orderRepositoryMock
                .Setup(repo => repo.GetSellOrdersBySpecificationAsNoTracking(It.IsAny<ISpecification<SellOrder>>()))
                .ReturnsAsync(new List<SellOrder>());

            // Act
            await _pendingOrdersInitializer.StartAsync(CancellationToken.None);

            // Assert
            _sellOrdersStoreMock.Verify(store => store.AddSellOrder(It.IsAny<Domain.Events.SellOrderCreatedCommand>()), Times.Never);
            _workerSubscriptionsManagerMock.Verify(
                manager => manager.AddStockSymbol(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        private static SellOrder CreateSellOrder(string stockSymbol)
        {
            return new SellOrder
            {
                SellOrderID = Guid.NewGuid(),
                StockSymbol = stockSymbol,
                StockName = stockSymbol,
                DateAndTimeOfOrder = DateTime.UtcNow,
                Price = 100,
                Quantity = 10,
                Status = SellOrderStatus.Pending,
                UserId = Guid.NewGuid()
            };
        }
    }
}
