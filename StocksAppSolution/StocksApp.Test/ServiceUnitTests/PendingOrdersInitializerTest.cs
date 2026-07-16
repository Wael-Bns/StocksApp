using Microsoft.Extensions.DependencyInjection;
using Moq;
using StocksApp.Domain.Entities;
using StocksApp.Domain.RepositoryContracts;
using StocksApp.Domain.Specifications;
using StocksApp.OrdersWorker.ServiceContracts;
using StocksApp.OrdersWorker.Services;
using StocksApp.OrdersWorker.Stores;
using StocksApp.Tests.Common.Builders;
using Xunit;

namespace StocksApp.Test.ServiceUnitTests
{
    public class PendingOrdersInitializerTest
    {
        private readonly Mock<ISellOrdersStore> _sellOrdersStoreMock;
        private readonly Mock<IGenericRepository<SellOrder>> _sellOrdersRepositoryMock;
        private readonly Mock<IWorkerSubscriptionsManager> _workerSubscriptionsManagerMock;
        private readonly ServiceProvider _serviceProvider;
        private readonly PendingOrdersInitializer _pendingOrdersInitializer;

        public PendingOrdersInitializerTest()
        {
            _sellOrdersStoreMock = new Mock<ISellOrdersStore>();
            _sellOrdersRepositoryMock = new Mock<IGenericRepository<SellOrder>>();
            _workerSubscriptionsManagerMock = new Mock<IWorkerSubscriptionsManager>();

            _serviceProvider = new ServiceCollection()
                .AddScoped(_ => _sellOrdersRepositoryMock.Object)
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
                new SellOrderBuilder().WithStockSymbol("AAPL").WithStockName("AAPL").Build(),
                new SellOrderBuilder().WithStockSymbol("MSFT").WithStockName("MSFT").Build()
            };

            _sellOrdersRepositoryMock
                .Setup(repo => repo.ListAsync(It.IsAny<ISpecification<SellOrder>>()))
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
            _sellOrdersRepositoryMock
                .Setup(repo => repo.ListAsync(It.IsAny<ISpecification<SellOrder>>()))
                .ReturnsAsync(new List<SellOrder>());

            // Act
            await _pendingOrdersInitializer.StartAsync(CancellationToken.None);

            // Assert
            _sellOrdersStoreMock.Verify(store => store.AddSellOrder(It.IsAny<Domain.Events.SellOrderCreatedCommand>()), Times.Never);
            _workerSubscriptionsManagerMock.Verify(
                manager => manager.AddStockSymbol(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}