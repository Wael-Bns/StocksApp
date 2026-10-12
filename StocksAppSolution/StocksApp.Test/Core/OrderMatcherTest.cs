// StocksApp.Test.Core/OrderMatcherTest.cs
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using StocksApp.Core.Options;
using StocksApp.Core.Services;
using StocksApp.Domain.Entities;
using StocksApp.Domain.Enums;
using StocksApp.Domain.Events;
using StocksApp.Domain.RepositoryContracts;
using StocksApp.Tests.Common.Builders;
using Xunit;

namespace StocksApp.Test.Core
{
    public class OrderMatcherTest
    {
        private readonly Mock<ISellOrderMatchRepository> _matchRepoMock = new();
        private readonly Mock<IGenericRepository<Outbox>> _outboxRepoMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly OrderMatcher _matcher;

        public OrderMatcherTest()
        {
            _matcher = new OrderMatcher(
                _matchRepoMock.Object, _outboxRepoMock.Object, _unitOfWorkMock.Object,
                Options.Create(new OrderMatchingOptions { MatcherGracePeriod = TimeSpan.FromSeconds(2) }),
                Mock.Of<ILogger<OrderMatcher>>());
        }

        [Fact]
        public async Task RunOnceAsync_NoMatchableOrders_CommitsWithoutTouchingOutbox()
        {
            // Arrange
            _matchRepoMock.Setup(r => r.GetMatchableOrderIdsAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<Guid>());

            // Act
            await _matcher.RunOnceAsync(CancellationToken.None);

            // Assert
            _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _outboxRepoMock.Verify(o => o.AddAsync(It.IsAny<Outbox>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task RunOnceAsync_MatchableOrders_ExecutesEachAndWritesOutboxEvent()
        {
            // Arrange
            var owner = new UserBuilder().WithCashBalance(1000).Build();
            var order = SellOrder.Create(owner.UserId, "AAPL", "Apple", 150, 10, DateTime.UtcNow, TimeSpan.FromSeconds(5));
            order.User = owner;

            _matchRepoMock.Setup(r => r.GetMatchableOrderIdsAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { order.SellOrderID });
            _matchRepoMock.Setup(r => r.ListTrackedAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { order });

            // Act
            await _matcher.RunOnceAsync(CancellationToken.None);

            // Assert
            order.Status.Should().Be(SellOrderStatus.Executed);
            _matchRepoMock.Verify(r => r.CreditCashAsync(It.IsAny<Guid>(), order.Price * order.Quantity, It.IsAny<CancellationToken>()), Times.Once);
            _outboxRepoMock.Verify(o => o.AddAsync(It.Is<Outbox>(e => e.EventName == SellOrderExecuted.EventName)), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task RunOnceAsync_RepositoryThrows_RollsBackAndRethrows()
        {
            // Arrange
            _matchRepoMock.Setup(r => r.GetMatchableOrderIdsAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("db error"));

            // Act
            var act = () => _matcher.RunOnceAsync(CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>();
            _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
        [Fact]
        public async Task RunOnceAsync_MatchableOrders_ExecutesEachAndCreditsCashAtomically()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var order = SellOrder.Create(userId, "AAPL", "Apple", 150, 10, DateTime.UtcNow, TimeSpan.FromSeconds(5));

            _matchRepoMock.Setup(r => r.GetMatchableOrderIdsAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { order.SellOrderID });
            _matchRepoMock.Setup(r => r.ListTrackedAsync(It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { order });

            // Act
            await _matcher.RunOnceAsync(CancellationToken.None);

            // Assert
            order.Status.Should().Be(SellOrderStatus.Executed);
            _matchRepoMock.Verify(r => r.CreditCashAsync(userId, 150 * 10, It.IsAny<CancellationToken>()), Times.Once);
            _outboxRepoMock.Verify(o => o.AddAsync(It.Is<Outbox>(e => e.EventName == SellOrderExecuted.EventName)), Times.Once);
        }

    }
}