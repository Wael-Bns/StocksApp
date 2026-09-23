using System.Text.Json;
using FluentAssertions;
using Moq;
using StocksApp.Core.Exceptions;
using StocksApp.Core.MessageBroker.Publisher;
using StocksApp.Domain.Events;
using StocksApp.OutboxDispatcher.EventHandlers;
using StocksApp.Tests.Common.Builders;
using Xunit;

namespace StocksApp.Test.OutboxDispatcher
{
    public class SellOrderCreatedCommandOutboxHandlerTest
    {
        private readonly Mock<ICommandSender> _commandSenderMock;
        private readonly SellOrderCreatedCommandOutboxHandler _outboxHandler;

        public SellOrderCreatedCommandOutboxHandlerTest()
        {
            _commandSenderMock = new Mock<ICommandSender>();
            _outboxHandler = new SellOrderCreatedCommandOutboxHandler(_commandSenderMock.Object);
        }

        [Fact]
        public void EventType_MatchesCommandsStaticEventName()
        {
            // Assert
            _outboxHandler.EventType.Should().Be(SellOrderCreatedCommand.EventName);
        }

        [Fact]
        public async Task HandleAsync_ValidPayload_SendsDeserializedCommand()
        {
            // Arrange
            var sellOrder = new SellOrderBuilder()
                .WithStockSymbol("AAPL")
                .WithStockName("Apple Inc")
                .WithPrice(50)
                .WithQuantity(3)
                .Build();
            var command = sellOrder.ToSellOrderCreatedCommand();
            var payload = JsonSerializer.Serialize(command);

            SellOrderCreatedCommand? sentCommand = null;
            _commandSenderMock
                .Setup(s => s.SendAsync(It.IsAny<SellOrderCreatedCommand>(), It.IsAny<CancellationToken>()))
                .Callback<SellOrderCreatedCommand, CancellationToken>((c, _) => sentCommand = c)
                .Returns(Task.CompletedTask);

            // Act
            await _outboxHandler.HandleAsync(payload);

            // Assert
            sentCommand.Should().NotBeNull();
            sentCommand!.SellOrderId.Should().Be(command.SellOrderId);
            sentCommand.UserId.Should().Be(command.UserId);
            sentCommand.StockSymbol.Should().Be(command.StockSymbol);
            sentCommand.Price.Should().Be(command.Price);
            sentCommand.Quantity.Should().Be(command.Quantity);
            _commandSenderMock.Verify(
                s => s.SendAsync(It.IsAny<SellOrderCreatedCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task HandleAsync_MalformedPayload_ThrowsJsonException_AndNeverSendsCommand()
        {
            // Arrange
            const string malformedPayload = "{ this is not valid json";

            // Act
            Func<Task> actual = async () => await _outboxHandler.HandleAsync(malformedPayload);

            // Assert
            await actual.Should().ThrowAsync<JsonException>();
            _commandSenderMock.Verify(
                s => s.SendAsync(It.IsAny<SellOrderCreatedCommand>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task HandleAsync_JsonNullLiteral_ThrowsJsonException()
        {
            // Arrange — syntactically valid JSON that deserializes to a null command
            const string nullPayload = "null";

            // Act
            Func<Task> actual = async () => await _outboxHandler.HandleAsync(nullPayload);

            // Assert
            await actual.Should().ThrowAsync<JsonException>();
        }
    }
}