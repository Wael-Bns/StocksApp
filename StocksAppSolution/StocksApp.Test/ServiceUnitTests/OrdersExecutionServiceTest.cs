using FluentAssertions;
using Moq;
using StocksApp.Core.Services;
using StocksApp.Domain.Entities;
using StocksApp.Domain.Enums;
using StocksApp.Domain.RepositoryContracts;
using StocksApp.Tests.Common.Builders;
using Xunit;
using StocksApp.Domain.Events;


namespace StocksApp.Test.ServiceUnitTests
{
    public class OrdersExecutionServiceTest
    {
        private readonly Mock<IOrderRepository> _orderRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly OrdersExecutionService _ordersExecutionService;

        public OrdersExecutionServiceTest()
        {
            _orderRepositoryMock = new Mock<IOrderRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _ordersExecutionService = new OrdersExecutionService(
                _orderRepositoryMock.Object,
                _unitOfWorkMock.Object);
        }

        [Fact]
        public async Task ExecuteSellOrdersAsync_ValidOrders_MarksOrdersAsExecutedAndCreditsUsers()
        {
            // Arrange
            var user = new UserBuilder().WithCashBalance(1000).Build();
            var sellOrder = new SellOrderBuilder()
                .WithStockSymbol("AAPL")
                .WithStockName("Apple Inc")
                .WithPrice(50)
                .WithQuantity(3)
                .WithUser(user)
                .Build();
            var command = sellOrder.ToSellOrderCreatedCommand();

            _orderRepositoryMock
                .Setup(repo => repo.GetSellOrdersByIds(It.Is<List<Guid>>(ids => ids.Single() == sellOrder.SellOrderID)))
                .ReturnsAsync(new List<SellOrder> { sellOrder });

            // Act
            await _ordersExecutionService.ExecuteSellOrdersAsync(new[] { command }, CancellationToken.None);

            // Assert
            sellOrder.Status.Should().Be(SellOrderStatus.Executed);
            user.CashBalance.Should().Be(1150);

            _unitOfWorkMock.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ExecuteSellOrdersAsync_WhenSaveFails_RollsBackTransaction()
        {
            // Arrange
            var user = new UserBuilder().WithCashBalance(1000).Build();
            var sellOrder = new SellOrderBuilder()
                .WithStockSymbol("AAPL")
                .WithStockName("Apple Inc")
                .WithPrice(25)
                .WithQuantity(2)
                .WithUser(user)
                .Build();
            var command = sellOrder.ToSellOrderCreatedCommand();

            _orderRepositoryMock
                .Setup(repo => repo.GetSellOrdersByIds(It.IsAny<List<Guid>>()))
                .ReturnsAsync(new List<SellOrder> { sellOrder });

            _unitOfWorkMock
                .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Database failed"));

            // Act
            Func<Task> actual = async () =>
                await _ordersExecutionService.ExecuteSellOrdersAsync(new[] { command }, CancellationToken.None);

            // Assert
            await actual.Should().ThrowAsync<Exception>()
                .WithMessage("An error occurred while executing sell orders.");

            _unitOfWorkMock.Verify(uow => uow.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(uow => uow.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
