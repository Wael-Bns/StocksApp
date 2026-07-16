using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using StocksApp.Domain.Events;
using StocksApp.OrdersWorker.Channels;
using StocksApp.OrdersWorker.MessageHandlers;
using StocksApp.OrdersWorker.Messages;
using StocksApp.OrdersWorker.Services;
using Xunit;

namespace StocksApp.Test.ServiceUnitTests
{
    public class OrderMessageProcessorTest
    {
        private readonly Mock<IWorkerMessageHandler> _priceUpdateHandlerMock;
        private readonly Mock<IWorkerMessageHandler> _sellOrderHandlerMock;

        public OrderMessageProcessorTest()
        {
            _priceUpdateHandlerMock = new Mock<IWorkerMessageHandler>();
            _priceUpdateHandlerMock.Setup(h => h.MessageType).Returns(typeof(PriceUpdateWorkerMessage));

            _sellOrderHandlerMock = new Mock<IWorkerMessageHandler>();
            _sellOrderHandlerMock.Setup(h => h.MessageType).Returns(typeof(SellOrderCreatedWorkerMessage));
        }

        [Fact]
        public async Task StartAsync_PriceUpdateMessage_DispatchesToPriceUpdateHandler()
        {
            // Arrange
            var message = new PriceUpdateWorkerMessage("AAPL", 100);
            var processor = CreateProcessor(message);

            // Act
            await processor.StartAsync(CancellationToken.None);

            // Assert
            _priceUpdateHandlerMock.Verify(
                handler => handler.HandleAsync(message, It.IsAny<CancellationToken>()),
                Times.Once);
            _sellOrderHandlerMock.Verify(
                handler => handler.HandleAsync(It.IsAny<SellOrderCreatedWorkerMessage>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task StartAsync_SellOrderCreatedMessage_DispatchesToSellOrderHandler()
        {
            // Arrange
            var message = new SellOrderCreatedWorkerMessage(CreateSellOrderCommand());
            var processor = CreateProcessor(message);

            // Act
            await processor.StartAsync(CancellationToken.None);

            // Assert
            _sellOrderHandlerMock.Verify(
                handler => handler.HandleAsync(message, It.IsAny<CancellationToken>()),
                Times.Once);
            _priceUpdateHandlerMock.Verify(
                handler => handler.HandleAsync(It.IsAny<PriceUpdateWorkerMessage>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task StartAsync_WhenHandlerThrows_ContinuesProcessingNextMessage()
        {
            // Arrange
            var failingMessage = new PriceUpdateWorkerMessage("AAPL", 100);
            var nextMessage = new SellOrderCreatedWorkerMessage(CreateSellOrderCommand());

            _priceUpdateHandlerMock
                .Setup(handler => handler.HandleAsync(failingMessage, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Handler failed"));

            var processor = CreateProcessor(failingMessage, nextMessage);

            // Act
            await processor.StartAsync(CancellationToken.None);

            // Assert
            _priceUpdateHandlerMock.Verify(
                handler => handler.HandleAsync(failingMessage, It.IsAny<CancellationToken>()),
                Times.Once);
            _sellOrderHandlerMock.Verify(
                handler => handler.HandleAsync(nextMessage, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        private OrderMessageProcessor CreateProcessor(params WorkerMessage[] messages)
        {
            return new OrderMessageProcessor(
                new TestWorkerChannel(messages),
                new[] { _priceUpdateHandlerMock.Object,
                _sellOrderHandlerMock.Object },
                NullLogger<OrderMessageProcessor>.Instance);
        }

        private static SellOrderCreatedCommand CreateSellOrderCommand()
        {
            return new SellOrderCreatedCommand
            {
                SellOrderId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                StockSymbol = "AAPL",
                Price = 100,
                Quantity = 10,
                CreatedAt = DateTime.UtcNow
            };
        }

        private sealed class TestWorkerChannel : IWorkerChannel
        {
            private readonly IReadOnlyCollection<WorkerMessage> _messages;

            public TestWorkerChannel(IReadOnlyCollection<WorkerMessage> messages)
            {
                _messages = messages;
            }

            public ValueTask EnqueueAsync(WorkerMessage message, CancellationToken cancellationToken = default)
            {
                return ValueTask.CompletedTask;
            }

            public async IAsyncEnumerable<WorkerMessage> ReadAllAsync(
                [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
            {
                foreach (var message in _messages)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    yield return message;
                    await Task.Yield();
                }
            }
        }
    }
}
