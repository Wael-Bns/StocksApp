using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using StocksApp.Domain.Events;
using StocksApp.OrdersWorker.MessageHandlers;
using StocksApp.OrdersWorker.Messages;
using StocksApp.OrdersWorker.ServiceContracts;
using StocksApp.OrdersWorker.Stores;
using StocksApp.Tests.Common.Builders;
using Xunit;

namespace StocksApp.Test.OrdersWorker
{
    public class SellOrderCreatedMessageHandlerTest
    {
        private readonly Mock<IPriceFeedSubscriptionRegistry> _priceFeedSubscriptionRegistry;
        private readonly Mock<IPendingOrdersStore> _sellOrdersStoreMock;
        private readonly SellOrderCreatedMessageHandler _handler;

        public SellOrderCreatedMessageHandlerTest()
        {
            _priceFeedSubscriptionRegistry = new Mock<IPriceFeedSubscriptionRegistry>();
            _sellOrdersStoreMock = new Mock<IPendingOrdersStore>();
            _handler = new SellOrderCreatedMessageHandler(
                NullLogger<SellOrderCreatedMessageHandler>.Instance,
                _priceFeedSubscriptionRegistry.Object,
                _sellOrdersStoreMock.Object);
        }

        [Fact]
        public async Task HandleAsync_ValidMessage_AddsOrderBeforeSubscribingSymbol()
        {
            // Arrange
            var cancellationToken = new CancellationTokenSource().Token;
            var sellOrder = new SellOrderCreatedCommandBuilder().WithStockSymbol("AAPL").Build();
            var message = new SellOrderCreatedWorkerMessage(sellOrder);

            var sequence = new MockSequence();
            _sellOrdersStoreMock.InSequence(sequence)
                .Setup(store => store.AddSellOrder(sellOrder));
            _priceFeedSubscriptionRegistry.InSequence(sequence)
                .Setup(manager => manager.EnsureSubscribedAsync(sellOrder.StockSymbol, cancellationToken))
                .Returns(Task.CompletedTask);

            // Act
            var act = () => _handler.HandleAsync(message, cancellationToken);

            // Assert
            await act.Should().NotThrowAsync();
            _sellOrdersStoreMock.Verify(store => store.AddSellOrder(sellOrder), Times.Once);
            _priceFeedSubscriptionRegistry.Verify(
                manager => manager.EnsureSubscribedAsync(sellOrder.StockSymbol, cancellationToken),
                Times.Once);
            _sellOrdersStoreMock.Verify(store => store.RemoveSellOrder(It.IsAny<SellOrderCreatedCommand>()), Times.Never);
        }

        [Fact]
        public async Task HandleAsync_SubscriptionFails_RollsBackOrderAndRethrows()
        {
            // Arrange
            var cancellationToken = new CancellationTokenSource().Token;
            var sellOrder = new SellOrderCreatedCommandBuilder().WithStockSymbol("AAPL").Build();
            var message = new SellOrderCreatedWorkerMessage(sellOrder);
            var subscriptionException = new InvalidOperationException("subscription failed");

            _priceFeedSubscriptionRegistry
                .Setup(manager => manager.EnsureSubscribedAsync(sellOrder.StockSymbol, cancellationToken))
                .ThrowsAsync(subscriptionException);

            // Act
            var act = () => _handler.HandleAsync(message, cancellationToken);

            // Assert
            (await act.Should().ThrowAsync<InvalidOperationException>())
                .Which.Should().BeSameAs(subscriptionException);
            _sellOrdersStoreMock.Verify(store => store.AddSellOrder(sellOrder), Times.Once);
            _sellOrdersStoreMock.Verify(store => store.RemoveSellOrder(sellOrder), Times.Once);
        }

        [Fact]
        public async Task HandleAsync_SubscriptionCanceled_DoesNotRollBackOrder()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            var sellOrder = new SellOrderCreatedCommandBuilder().WithStockSymbol("AAPL").Build();
            var message = new SellOrderCreatedWorkerMessage(sellOrder);

            _priceFeedSubscriptionRegistry
                .Setup(manager => manager.EnsureSubscribedAsync(sellOrder.StockSymbol, cts.Token))
                .ThrowsAsync(new OperationCanceledException());

            // Act
            var act = () => _handler.HandleAsync(message, cts.Token);

            // Assert
            await act.Should().ThrowAsync<OperationCanceledException>();
            _sellOrdersStoreMock.Verify(store => store.AddSellOrder(sellOrder), Times.Once);
            _sellOrdersStoreMock.Verify(store => store.RemoveSellOrder(It.IsAny<SellOrderCreatedCommand>()), Times.Never);
        }
    }
}