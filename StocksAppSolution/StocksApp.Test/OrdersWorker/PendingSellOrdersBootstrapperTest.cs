using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using StocksApp.Domain.Entities;
using StocksApp.Domain.RepositoryContracts;
using StocksApp.Domain.Specifications;
using StocksApp.OrdersWorker.ServiceContracts;
using StocksApp.OrdersWorker.Services;
using StocksApp.OrdersWorker.Stores;
using StocksApp.Tests.Common.Builders;
using Xunit;

namespace StocksApp.Test.OrdersWorker
{
    public class PendingSellOrdersBootstrapperTest
    {
        private readonly Mock<IPendingOrdersStore> _sellOrdersStoreMock;
        private readonly Mock<IGenericRepository<SellOrder>> _sellOrdersRepositoryMock;
        private readonly Mock<IPriceFeedSubscriptionRegistry> _priceFeedSubscriptionRegistry;
        private readonly ServiceProvider _serviceProvider;
        private readonly PendingSellOrdersBootstrapper _pendingSellOrdersBootstrapper;

        public PendingSellOrdersBootstrapperTest()
        {
            _sellOrdersStoreMock = new Mock<IPendingOrdersStore>();
            _sellOrdersRepositoryMock = new Mock<IGenericRepository<SellOrder>>();
            _priceFeedSubscriptionRegistry = new Mock<IPriceFeedSubscriptionRegistry>();

            _serviceProvider = new ServiceCollection()
                .AddScoped(_ => _sellOrdersRepositoryMock.Object)
                .BuildServiceProvider();

            _pendingSellOrdersBootstrapper = new PendingSellOrdersBootstrapper(
                _sellOrdersStoreMock.Object,
                _serviceProvider.GetRequiredService<IServiceScopeFactory>(),
                _priceFeedSubscriptionRegistry.Object,
                NullLogger<PendingSellOrdersBootstrapper>.Instance);
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
            await _pendingSellOrdersBootstrapper.RestoreAsync(cancellationToken);

            // Assert
            _sellOrdersStoreMock.Verify(store => store.AddSellOrder(
                It.Is<Domain.Events.SellOrderCreatedCommand>(order => order.StockSymbol == "AAPL")), Times.Once);
            _sellOrdersStoreMock.Verify(store => store.AddSellOrder(
                It.Is<Domain.Events.SellOrderCreatedCommand>(order => order.StockSymbol == "MSFT")), Times.Once);

            _priceFeedSubscriptionRegistry.Verify(
                manager => manager.EnsureSubscribedAsync("AAPL", cancellationToken),
                Times.Once);
            _priceFeedSubscriptionRegistry.Verify(
                manager => manager.EnsureSubscribedAsync("MSFT", cancellationToken),
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
            await _pendingSellOrdersBootstrapper.RestoreAsync(CancellationToken.None);

            // Assert
            _sellOrdersStoreMock.Verify(store => store.AddSellOrder(It.IsAny<Domain.Events.SellOrderCreatedCommand>()), Times.Never);
            _priceFeedSubscriptionRegistry.Verify(
                manager => manager.EnsureSubscribedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}